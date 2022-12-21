using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

public class WorldGenerator : MonoBehaviour
{
    public Queue<ChunkRenderer> ChunkPool { get; } = new Queue<ChunkRenderer>();
    public CancellationTokenSource TokenSource { get; } = new CancellationTokenSource();
    public Action OnWorldCreated { get; set; }

    private World World => GameManager.World;
    
    private struct ChunkUpdateData
    {
        public List<Vector3Int> chunkDataToCreate;
        public List<Vector3Int> chunksToCreate;
        public List<Vector3Int> chunksToRemove;
        public List<Vector3Int> chunkDataToRemove;
    }

    public async void GenerateWorld()
    {
        await generateWorld(Vector3Int.RoundToInt(Save.SaveData.PlayerData.Position));
    }
    
    public async void LoadAdditionalChunksRequest(Player player)
    {
        await generateWorld(player.BlockPosition);
        GameManager.OnNewChunksGenerated?.Invoke();
    }
    
    public void ClearWorld()
    {
        foreach (var chunkRenderer in ChunkRenderer.ChunkRenderers.Values)
            Destroy(chunkRenderer.gameObject);
        ChunkPool.Clear();
        Chunk.Chunks.Clear();
        ChunkRenderer.ChunkRenderers.Clear();
    }
    
    private async Task generateWorld(Vector3Int position)
    {
        if (!World.IsWorldCreated)
            GameManager.ProgressBar.SetDescription("Generating world data");
        GameManager.BiomeGenerator.GenerateBiomePoints(World.MapSeed);
        ChunkUpdateData chunkUpdateData = await Task.Run(() => getGenerationData(position), TokenSource.Token);
        IEnumerable<Chunk> toRender = null;
        
        if (!World.IsWorldCreated)
            GameManager.ProgressBar.SetProgress(0.1f);

        List<ChunkRenderer> renderers = new List<ChunkRenderer>();
        await Task.Run(() =>
        {
            foreach (var pos in chunkUpdateData.chunksToRemove)
            {
                if (ChunkRenderer.ChunkRenderers.TryGetValue(pos, out ChunkRenderer chunkRenderer))
                {
                    renderers.Add(chunkRenderer);
                    ChunkPool.Enqueue(chunkRenderer);
                    ChunkRenderer.ChunkRenderers.Remove(pos);
                }
            }
        });
        foreach (var r in renderers)
            r.gameObject.SetActive(false);
        
        if (!World.IsWorldCreated)
            GameManager.ProgressBar.SetProgress(0.2f);

        ConcurrentDictionary<Vector3Int, Chunk> chunksDict = new ConcurrentDictionary<Vector3Int, Chunk>();
        await Task.Run(() =>
        {
            foreach (var pos in chunkUpdateData.chunkDataToRemove)
                Chunk.Chunks.Remove(pos);

            foreach (var pos in chunkUpdateData.chunkDataToCreate)
            {
                if (TokenSource.Token.IsCancellationRequested)
                    TokenSource.Token.ThrowIfCancellationRequested();
                Chunk chunk = new Chunk(World.ChunkSize, World.ChunkHeight, pos);
                chunk.ChunkGenerator.GenerateChunk(World.MapSeed);
                chunksDict.TryAdd(pos, chunk);
            }
        });

        if (!World.IsWorldCreated)
            GameManager.ProgressBar.SetProgress(0.4f);

        await Task.Run(() =>
        {
            foreach (var (pos, chunk) in chunksDict)
            {
                if (TokenSource.Token.IsCancellationRequested)
                    TokenSource.Token.ThrowIfCancellationRequested();
                Chunk.Chunks.Add(pos, chunk);
            }
        });

        if (!World.IsWorldCreated)
        {
            GameManager.ProgressBar.SetProgress(0.45f);
            GameManager.ProgressBar.SetDescription("Generating tree data");
        }
            
        await Task.Run(() =>
        {
            foreach (var chunk in Chunk.Chunks.Values)
            foreach (var treeLeaves in chunk.ChunkGenerator.TreeData.TreeLeavesSolid)
                chunk.SetBlock(treeLeaves, BlockType.TreeLeavesSolid, true);
        });
        
        if (!World.IsWorldCreated)
        {
            GameManager.ProgressBar.SetProgress(0.5f);
            GameManager.ProgressBar.SetDescription("Applying save data");
        }

        await Task.Run(() =>
        {
            foreach (var chunk in Chunk.Chunks.Values)
            {
                if (!Save.SaveData.ModifiedChunks.Contains(chunk.WorldPosition))
                    continue;
                foreach (var (pos, type) in Save.SaveData.ModifiedBlocks[chunk.WorldPosition])
                {
                    var localPos = chunk.GetLocalPosition(pos);
                    chunk.SetBlock(localPos, type, true);
                }
            }
        });
        
        if (!World.IsWorldCreated)
        {
            GameManager.ProgressBar.SetProgress(0.7f);
            GameManager.ProgressBar.SetDescription("Preparing chunks for rendering");
        }
        
        await Task.Run(() =>
        {
            toRender = Chunk.Chunks
                .Where((keyvalpair) => chunkUpdateData.chunksToCreate.Contains(keyvalpair.Key))
                .Select(keyvalpair => keyvalpair.Value);
        });
        
        if (!World.IsWorldCreated)
            GameManager.ProgressBar.SetProgress(0.9f);
        
        await Task.Run(() =>
        {
            foreach (var chunk in toRender)
            {
                if(TokenSource.Token.IsCancellationRequested)
                    TokenSource.Token.ThrowIfCancellationRequested();
                chunk.GenerateChunkMesh();
            }
        }, TokenSource.Token);

        if (!World.IsWorldCreated)
            GameManager.ProgressBar.SetProgress(1f);
        
        StartCoroutine(chunkCreationCoroutine(toRender));
    }
    
    IEnumerator chunkCreationCoroutine(IEnumerable<Chunk> toRender)
    {
        foreach (var chunk in toRender)
        {
            chunk.RenderChunk();
            ChunkRenderer.ChunkRenderers.Add(chunk.WorldPosition, chunk.ChunkRenderer);
            yield return new WaitForEndOfFrame();
        }

        if (!World.IsWorldCreated)
        {
            World.IsWorldCreated = true;
            OnWorldCreated?.Invoke();
        }
    }

    private ChunkUpdateData getGenerationData(Vector3Int playerPos)
    {
        var chunkDataAroundPlayer = World.GetChunkPositionsAround(playerPos, World.RenderDistance + 1);
        var chunksAroundPlayer = World.GetChunkPositionsAround(playerPos);

        return new ChunkUpdateData()
        {
            chunkDataToCreate = chunkDataAroundPlayer
                .Where(pos => !Chunk.Chunks.ContainsKey(pos))
                .OrderBy(pos => Vector3Int.Distance(playerPos, pos))
                .ToList(),
            chunksToCreate = chunksAroundPlayer
                .Where(pos => !ChunkRenderer.ChunkRenderers.ContainsKey(pos))
                .OrderBy(pos => Vector3Int.Distance(playerPos, pos))
                .ToList(),
            chunkDataToRemove = Chunk.Chunks.Keys.Where(pos => !chunkDataAroundPlayer.Contains(pos)).ToList(),
            chunksToRemove = ChunkRenderer.ChunkRenderers.Keys.Where(pos => !chunkDataAroundPlayer.Contains(pos))
                .Where(pos => ChunkRenderer.ChunkRenderers.ContainsKey(pos)).ToList()
        };
    }
}
