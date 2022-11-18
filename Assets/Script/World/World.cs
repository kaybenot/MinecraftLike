using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int MapSizeInChunks = 6;
    public int ChunkSize = 16, chunkHeight = 100;
    public int WaterThreshold = 50;
    public float NoiseScale = 0.03f;
    public GameObject ChunkPrefab;
    
    private Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    private Dictionary<Vector3Int, ChunkRenderer> chunkRenderers = new Dictionary<Vector3Int, ChunkRenderer>();

    public void GenerateWorld()
    {
        chunks.Clear();
        foreach (var chunk in chunkRenderers.Values)
            Destroy(chunk.gameObject);
        chunkRenderers.Clear();
        for (int x = 0; x < MapSizeInChunks; x++)
        {
            for (int y = 0; y < MapSizeInChunks; y++)
            {
                Chunk data = new Chunk(ChunkSize, chunkHeight, this,
                    new Vector3Int(x * ChunkSize, 0, y * ChunkSize));
                generateVoxels(data);
                chunks.Add(data.WorldPosition, data);
            }
        }

        foreach (var chunk in chunks.Values)
        {
            GameObject chunkObject = Instantiate(ChunkPrefab, chunk.WorldPosition, Quaternion.identity);
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
            Mathf.FloorToInt(globalPosition.y / (float)chunkHeight) * chunkHeight,
            Mathf.FloorToInt(globalPosition.z / (float)ChunkSize) * ChunkSize);
        chunks.TryGetValue(chunkStartPos, out Chunk chunk);
        return chunk;
    }

    private void generateVoxels(Chunk chunk)
    {
        for (int x = 0; x < chunk.ChunkSize; x++)
        {
            for (int z = 0; z < chunk.ChunkSize; z++)
            {
                float noiseValue = Mathf.PerlinNoise((chunk.WorldPosition.x + x) * NoiseScale,
                    (chunk.WorldPosition.z + z) * NoiseScale);
                int groundPosition = Mathf.RoundToInt(noiseValue * chunkHeight);
                for (int y = 0; y < chunkHeight; y++)
                {
                    BlockType voxelType = BlockType.Dirt;
                    if (y > groundPosition)
                        voxelType = y < WaterThreshold ? BlockType.Water : BlockType.Air;
                    else if (y == groundPosition)
                        voxelType = BlockType.GrassDirt;
                    chunk.SetBlock(new Vector3Int(x, y, z), voxelType);
                }
            }
        }
    }
}
