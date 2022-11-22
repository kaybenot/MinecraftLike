using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class World
{
    public int SizeInChunks { get; }
    public int ChunkSize { get; }
    public int ChunkHeight { get; }
    public int WaterThreshold { get; }
    public GameObject ChunkPrefab { get; }
    public Vector2Int MapSeed { get; }

    private Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    private Dictionary<Vector3Int, ChunkRenderer> chunkRenderers = new Dictionary<Vector3Int, ChunkRenderer>();

    public World(GameObject chunkPrefab, Vector2Int mapSeed, int sizeInChunks = 10, int chunkSize = 16,
        int chunkHeight = 100, int waterThreshold = 10)
    {
        SizeInChunks = sizeInChunks;
        ChunkSize = chunkSize;
        ChunkHeight = chunkHeight;
        WaterThreshold = waterThreshold;
        ChunkPrefab = chunkPrefab;
        MapSeed = mapSeed;
    }
    
    public void GenerateWorld()
    {
        GameObject worldObj = new GameObject("World");
        
        chunks.Clear();
        foreach (var chunk in chunkRenderers.Values)
            Object.Destroy(chunk.gameObject);
        chunkRenderers.Clear();
        for (int x = 0; x < SizeInChunks; x++)
        {
            for (int y = 0; y < SizeInChunks; y++)
            {
                Chunk data = new Chunk(ChunkSize, ChunkHeight, this,
                    new Vector3Int(x * ChunkSize, 0, y * ChunkSize));
                data.GenerateChunk(MapSeed, WaterThreshold);
                chunks.Add(data.WorldPosition, data);
            }
        }

        foreach (var chunk in chunks.Values)
        {
            GameObject chunkObject = Object.Instantiate(ChunkPrefab, chunk.WorldPosition, Quaternion.identity);
            chunkObject.transform.parent = worldObj.transform;
            ChunkRenderer chunkRenderer = chunkObject.GetComponent<ChunkRenderer>();
            chunkRenderers.Add(chunk.WorldPosition, chunkRenderer);
            chunkRenderer.InitChunk(chunk);
            chunkRenderer.UpdateChunk(chunk.GetChunkMesh());
        }
    }

    public Block GetBlock(Vector3Int globalPosition)
    {
        Chunk chunk = GetChunk(globalPosition);
        if (chunk == null)
            return null;
        Vector3Int blockPosition = chunk.GetLocalPosition(globalPosition);
        return chunk.GetBlock(blockPosition);
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
        Debug.Log("Load more chunks");
        GameManager.OnNewChunksGenerated?.Invoke();
    }
}
