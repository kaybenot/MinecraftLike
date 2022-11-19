using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int MapSizeInChunks = 6;
    public int ChunkSize = 16, ChunkHeight = 100;
    public int WaterThreshold = 50;
    public GameObject ChunkPrefab;
    public Vector2Int MapSeedOffset;

    private Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    private Dictionary<Vector3Int, ChunkRenderer> chunkRenderers = new Dictionary<Vector3Int, ChunkRenderer>();

    public void GenerateWorld()
    {
        GameObject world = GameObject.FindWithTag("World");
        
        chunks.Clear();
        foreach (var chunk in chunkRenderers.Values)
            Destroy(chunk.gameObject);
        chunkRenderers.Clear();
        for (int x = 0; x < MapSizeInChunks; x++)
        {
            for (int y = 0; y < MapSizeInChunks; y++)
            {
                Chunk data = new Chunk(ChunkSize, ChunkHeight, this,
                    new Vector3Int(x * ChunkSize, 0, y * ChunkSize));
                data.GenerateChunk(MapSeedOffset, WaterThreshold);
                chunks.Add(data.WorldPosition, data);
            }
        }

        foreach (var chunk in chunks.Values)
        {
            GameObject chunkObject = Instantiate(ChunkPrefab, chunk.WorldPosition, Quaternion.identity);
            chunkObject.transform.parent = world.transform;
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
}
