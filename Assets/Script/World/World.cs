using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[RequireComponent(typeof(WorldGenerator))]
public class World : MonoBehaviour
{
    public int RenderDistance = 10;
    public int ChunkSize = 16;
    public int ChunkHeight = 100;
    public GameObject ChunkPrefab;
    public Vector2Int MapSeed;
    
    public bool IsWorldCreated { get; set; }
    public WorldGenerator WorldGenerator { get; private set; }

    private void Awake()
    {
        WorldGenerator = GetComponent<WorldGenerator>();
    }

    private void OnDisable()
    {
        WorldGenerator.TokenSource.Cancel();
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

    public void SetBlock(RaycastHit hit, BlockType blockType)
    {
        var chunkRenderer = hit.transform.GetComponent<ChunkRenderer>();
        if (chunkRenderer == null)
            return;
        var chunk = chunkRenderer.Chunk;
        var pos = getBlockPos(hit);
        SetBlock(pos, blockType);

        if (chunk.IsOnEdge(pos))
        {
            IEnumerable<Chunk> neighbourChunks = chunk.GetTouchedChunks(pos);
            foreach (var neigbourChunk in neighbourChunks)
                GetChunkRenderer(neigbourChunk.WorldPosition)?.UpdateMesh();
        }
        
        chunkRenderer.UpdateMesh();
    }
    
    public Vector3Int GetChunkFromPosition(Vector3Int globalPosition)
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
        Chunk.Chunks.TryGetValue(chunkStartPos, out Chunk chunk);
        return chunk;
    }

    public ChunkRenderer GetChunkRenderer(Vector3Int globalPosition)
    {
        return ChunkRenderer.ChunkRenderers.ContainsKey(globalPosition) ? ChunkRenderer.ChunkRenderers[globalPosition] : null;
    }

    public List<Vector3Int> GetChunkPositionsAround(Vector3Int globalPosition, int? range = null)
    {
        List<Vector3Int> chunksAround = new List<Vector3Int>();
        int distance = range ?? RenderDistance;
        int startX = globalPosition.x - distance * ChunkSize;
        int startZ = globalPosition.z - distance * ChunkSize;
        int endX = globalPosition.x + distance * ChunkSize;
        int endZ = globalPosition.z + distance * ChunkSize;

        for (int x = startX; x <= endX; x += ChunkSize)
        {
            for (int z = startZ; z <= endZ; z += ChunkSize)
            {
                var chunkPos = GetChunkFromPosition(new Vector3Int(x, 0, z));
                chunksAround.Add(chunkPos);
            }
        }
        return chunksAround;
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
}
