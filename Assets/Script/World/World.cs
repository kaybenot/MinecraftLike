using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class World
{
    public int RenderDistance { get; }
    public int ChunkSize { get; }
    public int ChunkHeight { get; }
    public int WaterThreshold { get; }
    public GameObject ChunkPrefab { get; }
    public Vector2Int MapSeed { get; }

    private Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    private Dictionary<Vector3Int, ChunkRenderer> chunkRenderers = new Dictionary<Vector3Int, ChunkRenderer>();

    private static GameObject worldObj;

    public World(GameObject chunkPrefab, Vector2Int mapSeed, int renderDistance = 10, int chunkSize = 16,
        int chunkHeight = 100, int waterThreshold = 10)
    {
        RenderDistance = renderDistance;
        ChunkSize = chunkSize;
        ChunkHeight = chunkHeight;
        WaterThreshold = waterThreshold;
        ChunkPrefab = chunkPrefab;
        MapSeed = mapSeed;
    }

    public void GenerateWorld()
    {
        generateWorld(Vector3Int.zero);
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
        var renderer = hit.transform.GetComponent<ChunkRenderer>();
        if (renderer == null)
            return false;
        var pos = getBlockPos(hit);
        SetBlock(pos, blockType);
        renderer.Chunk.ModifiedByPlayer = true;
        renderer.UpdateChunk();
        return true;
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

    public void LoadAdditionalChunksRequest(Player player)
    {
        generateWorld(player.BlockPosition);
        GameManager.OnNewChunksGenerated?.Invoke();
    }
    
    private void generateWorld(Vector3Int position)
    {
        if(worldObj == null)
            worldObj = new GameObject("World");
        
        WorldGenData worldGenData = getGenerationData(position);

        foreach (var pos in worldGenData.chunksToRemove)
            removeChunk(pos);
        foreach (var pos in worldGenData.chunkDataToRemove)
            removeChunkData(pos);
        
        foreach (var pos in worldGenData.chunkDataToCreate)
        {
            Chunk chunk = new Chunk(ChunkSize, ChunkHeight, this, pos);
            chunk.GenerateChunk(MapSeed, WaterThreshold);
            chunks.Add(pos, chunk);
        }
        foreach (var pos in worldGenData.chunksToCreate)
        {
            Chunk chunk = chunks[pos];
            ChunkMesh mesh = chunk.GetChunkMesh();
            GameObject chunkObj = Object.Instantiate(ChunkPrefab, pos, Quaternion.identity);
            chunkObj.transform.parent = worldObj.transform;
            ChunkRenderer chunkRenderer = chunkObj.GetComponent<ChunkRenderer>();
            chunkRenderers.Add(pos, chunkRenderer);
            chunkRenderer.InitChunk(chunk);
            chunkRenderer.UpdateChunk(mesh);
        }
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
        List<Vector3Int> chunks = new List<Vector3Int>();

        int startX = playerPos.x - RenderDistance * ChunkSize;
        int startZ = playerPos.z - RenderDistance * ChunkSize;
        int endX = playerPos.x + RenderDistance * ChunkSize;
        int endZ = playerPos.z + RenderDistance * ChunkSize;

        for (int x = startX; x <= endX; x += ChunkSize)
        {
            for (int z = startZ; z <= endZ; z += ChunkSize)
            {
                var chunkPos = GetChunkPosition(new Vector3Int(x, 0, z));
                chunks.Add(chunkPos);
            }
        }
        
        return chunks;
    }
    
    private List<Vector3Int> getChunkDataAroundPlayer(Vector3Int playerPos)
    {
        List<Vector3Int> chunks = new List<Vector3Int>();

        int startX = playerPos.x - (RenderDistance + 1) * ChunkSize;
        int startZ = playerPos.z - (RenderDistance + 1) * ChunkSize;
        int endX = playerPos.x + (RenderDistance + 1) * ChunkSize;
        int endZ = playerPos.z + (RenderDistance + 1) * ChunkSize;

        for (int x = startX; x <= endX; x += ChunkSize)
        {
            for (int z = startZ; z <= endZ; z += ChunkSize)
            {
                var chunkPos = GetChunkPosition(new Vector3Int(x, 0, z));
                chunks.Add(chunkPos);
            }
        }
        
        return chunks;
    }
    
    private List<Vector3Int> selectNewChunkData(Vector3Int playerPos, List<Vector3Int> chunkDataAroundPlayer)
    {
        return chunkDataAroundPlayer
            .Where(pos => !chunks.ContainsKey(pos))
            .OrderBy(pos => Vector3Int.Distance(playerPos, pos))
            .ToList();
    }

    private List<Vector3Int> selectChunksToCreate(Vector3Int playerPos, List<Vector3Int> chunksAroundPlayer)
    {
        return chunksAroundPlayer
            .Where(pos => !chunkRenderers.ContainsKey(pos))
            .OrderBy(pos => Vector3Int.Distance(playerPos, pos))
            .ToList();
    }

    private List<Vector3Int> getRedundantChunks(List<Vector3Int> chunkDataAroundPlayer)
    {
        List<Vector3Int> chunks = new List<Vector3Int>();
        foreach (var pos in chunkRenderers.Keys.Where(pos => !chunkDataAroundPlayer.Contains(pos)))
        {
            if(chunkRenderers.ContainsKey(pos))
                chunks.Add(pos);
        }

        return chunks;
    }
    
    private List<Vector3Int> getRedundantChunkData(List<Vector3Int> chunkDataAroundPlayer)
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
