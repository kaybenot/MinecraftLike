using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

struct WorldGenData
{
    public List<Vector3Int> chunkDataToCreate;
    public List<Vector3Int> chunksToCreate;
    public List<Vector3Int> chunksToRemove;
    public List<Vector3Int> chunkDataToRemove;
}

public class World : MonoBehaviour
{
    public int RenderDistance = 10;
    public int ChunkSize = 16;
    public int ChunkHeight = 100;
    public GameObject ChunkPrefab;
    public Vector2Int MapSeed;
    
    public bool IsWorldCreated { get; private set; }
    public Action OnWorldCreated { get; }

    /// <summary>
    /// Dictionary of chunks bound with position.
    /// </summary>
    private Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    /// <summary>
    /// Dictionary of chunk renderers bound with position.
    /// </summary>
    private Dictionary<Vector3Int, ChunkRenderer> chunkRenderers = new Dictionary<Vector3Int, ChunkRenderer>();
    
    public async void GenerateWorld(Action spawnPlayer)
    {
        await generateWorld(Vector3Int.zero);
        spawnPlayer?.Invoke();
    }

    public Block GetBlock(Vector3Int globalPosition)
    {
        Chunk chunk = GetChunk(globalPosition);
        if (chunk == null)
            return null;
        Vector3Int blockPosition = chunk.GetLocalPosition(globalPosition);
        return chunk.GetBlock(blockPosition);
    }

    public void SetBlock(Vector3Int globalPosition, BlockType blockType)
    {
        Chunk chunk = GetChunk(globalPosition);
        if (chunk != null)
        {
            Vector3Int localPos = chunk.GetLocalPosition(globalPosition);
            chunk.SetBlock(localPos, blockType);
        }
    }

    public bool SetBlock(RaycastHit hit, BlockType blockType)
    {
        var chunkRenderer = hit.transform.GetComponent<ChunkRenderer>();
        if (chunkRenderer == null)
            return false;
        var chunk = chunkRenderer.Chunk;
        var pos = getBlockPos(hit);
        SetBlock(pos, blockType);
        chunk.ModifiedByPlayer = true;

        if (chunk.IsOnEdge(pos))
        {
            IEnumerable<Chunk> neighbourChunks = chunk.GetTouchedChunks(pos);
            foreach (var neigbourChunk in neighbourChunks)
            {
                neigbourChunk.ModifiedByPlayer = true;
                GetChunkRenderer(neigbourChunk.WorldPosition)?.UpdateMesh();
            }
        }
        
        chunkRenderer.UpdateMesh();
        return true;
    }
    
    public Vector3Int GetChunkPosition(Vector3Int globalPosition)
    {
        return new Vector3Int(
            Mathf.FloorToInt(globalPosition.x / (float)ChunkSize) * ChunkSize,
            Mathf.FloorToInt(globalPosition.y / (float)ChunkHeight) * ChunkHeight,
            Mathf.FloorToInt(globalPosition.z / (float)ChunkSize) * ChunkSize);
    }

    public Chunk GetChunk(Vector3Int globalPosition)
    {
        Vector3Int chunkStartPos = new Vector3Int(
            Mathf.FloorToInt(globalPosition.x / (float)ChunkSize) * ChunkSize,
            Mathf.FloorToInt(globalPosition.y / (float)ChunkHeight) * ChunkHeight,
            Mathf.FloorToInt(globalPosition.z / (float)ChunkSize) * ChunkSize);
        chunks.TryGetValue(chunkStartPos, out Chunk chunk);
        return chunk;
    }

    public ChunkRenderer GetChunkRenderer(Vector3Int globalPosition)
    {
        return chunkRenderers.ContainsKey(globalPosition) ? chunkRenderers[globalPosition] : null;
    }
    
    public async void LoadAdditionalChunksRequest(Player player)
    {
        await generateWorld(player.BlockPosition);
        GameManager.OnNewChunksGenerated?.Invoke();
    }
    
    private Vector3Int getBlockPos(RaycastHit hit)
    {
        var pos = new Vector3(
            GetBlockPositionIn(hit.point.x, hit.normal.x),
            GetBlockPositionIn(hit.point.y, hit.normal.y),
            GetBlockPositionIn(hit.point.z, hit.normal.z));
        return Vector3Int.RoundToInt(pos);
    }

    private float GetBlockPositionIn(float pos, float normal)
    {
        if (Math.Abs(Mathf.Abs(pos % 1) - 0.5f) < 0.001f)
            pos -= (normal / 2);
        return pos;
    }
    
    private async Task generateWorld(Vector3Int position)
    {
        WorldGenData worldGenData = await Task.Run(() => getGenerationData(position));

        foreach (var pos in worldGenData.chunksToRemove)
            removeChunk(pos);
        foreach (var pos in worldGenData.chunkDataToRemove)
            removeChunkData(pos);

        ConcurrentDictionary<Vector3Int, Chunk> chunksDict = await calculateWorldChunkData(worldGenData.chunkDataToCreate);
        foreach (var (pos, chunk) in chunksDict)
            chunks.Add(pos, chunk);

        ConcurrentDictionary<Vector3Int, ChunkMesh> chunkMeshDict = new ConcurrentDictionary<Vector3Int, ChunkMesh>();
        await Task.Run(() =>
        {
            foreach (var pos in worldGenData.chunksToCreate)
            {
                Chunk chunk = chunks[pos];
                ChunkMesh mesh = chunk.GetChunkMesh();
                chunkMeshDict.TryAdd(pos, mesh);
            } 
        });

        StartCoroutine(chunkCreationCoroutine(chunkMeshDict));
    }

    private Task<ConcurrentDictionary<Vector3Int, Chunk>> calculateWorldChunkData(List<Vector3Int> chunkDataToCreate)
    {
        ConcurrentDictionary<Vector3Int, Chunk> dictionary = new ConcurrentDictionary<Vector3Int, Chunk>();
        return Task.Run(() =>
        {
            foreach (var pos in chunkDataToCreate)
            {
                Chunk chunk = new Chunk(ChunkSize, ChunkHeight, pos);
                chunk.GenerateChunk(MapSeed);
                dictionary.TryAdd(pos, chunk);
            }
            return dictionary;
        });
    }

    IEnumerator chunkCreationCoroutine(ConcurrentDictionary<Vector3Int, ChunkMesh> chunkMeshDict)
    {
        foreach (var (key, value) in chunkMeshDict)
        {
            createChunk(key, value);
            yield return new WaitForEndOfFrame();
        }

        if (!IsWorldCreated)
        {
            IsWorldCreated = true;
            OnWorldCreated?.Invoke();
        }
    }

    private void createChunk(Vector3Int pos, ChunkMesh mesh)
    {
        GameObject chunkObj = Object.Instantiate(ChunkPrefab, pos, Quaternion.identity);
        chunkObj.transform.parent = transform;
        ChunkRenderer chunkRenderer = chunkObj.GetComponent<ChunkRenderer>();
        chunkRenderers.Add(pos, chunkRenderer);
        chunkRenderer.BindChunk(chunks[pos]);
        chunkRenderer.UpdateMesh(mesh);
    }

    private void removeChunk(Vector3Int pos)
    {
        if (chunkRenderers.TryGetValue(pos, out ChunkRenderer chunkRenderer))
        {
            chunkRenderer.gameObject.SetActive(false);
            chunkRenderers.Remove(pos);
        }
    }

    private void removeChunkData(Vector3Int pos)
    {
        chunks.Remove(pos);
    }

    private List<Vector3Int> getChunkPositionsAroundPlayer(Vector3Int playerPos)
    {
        List<Vector3Int> chunksAround = new List<Vector3Int>();

        int startX = playerPos.x - RenderDistance * ChunkSize;
        int startZ = playerPos.z - RenderDistance * ChunkSize;
        int endX = playerPos.x + RenderDistance * ChunkSize;
        int endZ = playerPos.z + RenderDistance * ChunkSize;

        for (int x = startX; x <= endX; x += ChunkSize)
        {
            for (int z = startZ; z <= endZ; z += ChunkSize)
            {
                var chunkPos = GetChunkPosition(new Vector3Int(x, 0, z));
                chunksAround.Add(chunkPos);
            }
        }
        
        return chunksAround;
    }
    
    private List<Vector3Int> getChunkDataAroundPlayer(Vector3Int playerPos)
    {
        List<Vector3Int> chunksAround = new List<Vector3Int>();

        int startX = playerPos.x - (RenderDistance + 1) * ChunkSize;
        int startZ = playerPos.z - (RenderDistance + 1) * ChunkSize;
        int endX = playerPos.x + (RenderDistance + 1) * ChunkSize;
        int endZ = playerPos.z + (RenderDistance + 1) * ChunkSize;

        for (int x = startX; x <= endX; x += ChunkSize)
        {
            for (int z = startZ; z <= endZ; z += ChunkSize)
            {
                var chunkPos = GetChunkPosition(new Vector3Int(x, 0, z));
                chunksAround.Add(chunkPos);
            }
        }
        
        return chunksAround;
    }
    
    private List<Vector3Int> selectNewChunkData(Vector3Int playerPos, IEnumerable<Vector3Int> chunkDataAroundPlayer)
    {
        return chunkDataAroundPlayer
            .Where(pos => !chunks.ContainsKey(pos))
            .OrderBy(pos => Vector3Int.Distance(playerPos, pos))
            .ToList();
    }

    private List<Vector3Int> selectChunksToCreate(Vector3Int playerPos, IEnumerable<Vector3Int> chunksAroundPlayer)
    {
        return chunksAroundPlayer
            .Where(pos => !chunkRenderers.ContainsKey(pos))
            .OrderBy(pos => Vector3Int.Distance(playerPos, pos))
            .ToList();
    }

    /// <summary>
    /// Lists chunk renderer positions not in draw distance.
    /// </summary>
    /// <param name="chunkDataAroundPlayer">Chunks currently around player</param>
    /// <returns>Chunk renderers loaded but not around player</returns>
    private List<Vector3Int> getRedundantChunks(ICollection<Vector3Int> chunkDataAroundPlayer)
    {
        return chunkRenderers.Keys.Where(pos => !chunkDataAroundPlayer.Contains(pos)).Where(pos => chunkRenderers.ContainsKey(pos)).ToList();
    }
    
    /// <summary>
    /// Lists chunks positions not in draw distance. (+1 to properly handle edges).
    /// </summary>
    /// <param name="chunkDataAroundPlayer">Chunks currently around player</param>
    /// <returns>Chunk positions loaded but not around player</returns>
    private List<Vector3Int> getRedundantChunkData(ICollection<Vector3Int> chunkDataAroundPlayer)
    {
        return chunks.Keys.Where(pos => !chunkDataAroundPlayer.Contains(pos)).ToList();
    }

    private WorldGenData getGenerationData(Vector3Int playerPos)
    {
        var chunkDataAroundPlayer = getChunkDataAroundPlayer(playerPos);
        var chunksAroundPlayer = getChunkPositionsAroundPlayer(playerPos);
        var chunkDataToCreate = selectNewChunkData(playerPos, chunkDataAroundPlayer);
        var chunksToCreate = selectChunksToCreate(playerPos, chunksAroundPlayer);
        
        return new WorldGenData()
        {
            chunkDataToCreate = chunkDataToCreate,
            chunksToCreate = chunksToCreate,
            chunkDataToRemove = getRedundantChunkData(chunkDataAroundPlayer),
            chunksToRemove = getRedundantChunks(chunksAroundPlayer)
        };
    }
}
