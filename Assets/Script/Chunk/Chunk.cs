using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public Block[,,] Blocks { get; }
    public int ChunkSize { get; }
    public int ChunkHeight { get; }
    public World World { get; }
    public Vector3Int WorldPosition { get; }
    public bool ModifiedByPlayer { get; set; }
    
    public Chunk(int chunkSize, int chunkHeight, World world, Vector3Int worldPosition)
    {
        ChunkSize = chunkSize;
        ChunkHeight = chunkHeight;
        World = world;
        WorldPosition = worldPosition;
        Blocks = new Block[chunkSize, chunkHeight, chunkSize];
        for (int x = 0; x < chunkSize; x++)
            for(int y = 0; y < chunkHeight; y++)
                for(int z = 0; z < chunkSize; z++)
                    Blocks[x, y, z] = new Block(new Vector3Int(x, y, z), BlockType.Air);
        ModifiedByPlayer = false;
    }

    public void GenerateChunk(Vector2Int mapSeedOffset, int waterThreshold)
    {
        for (int x = 0; x < ChunkSize; x++)
        {
            for (int z = 0; z < ChunkSize; z++)
            {
                processChunkColumn(mapSeedOffset, x, z, waterThreshold);
            }
        }
    }
    
    public ChunkMesh GetChunkMesh()
    {
        ChunkMesh chunkMesh = new ChunkMesh(true);

        foreach (var block in Blocks)
            chunkMesh.AddBlock(this, block);

        return chunkMesh;
    }

    public void SetBlock(Vector3Int localPosition, BlockType blockType)
    {
        if (inRange(localPosition))
            Blocks[localPosition.x, localPosition.y, localPosition.z].BlockType = blockType;
        else
            World.GetBlock(WorldPosition + localPosition).BlockType = blockType;
    }
    
    public Block GetBlock(Vector3Int localPosition)
    {
        return inRange(localPosition) ? Blocks[localPosition.x, localPosition.y, localPosition.z] : 
            World.GetBlock(WorldPosition + localPosition);
    }

    public Vector3Int GetLocalPosition(Vector3Int globalPosition)
    {
        return new Vector3Int(
            globalPosition.x - WorldPosition.x,
            globalPosition.y - WorldPosition.y,
            globalPosition.z - WorldPosition.z);
    }

    private void processChunkColumn(Vector2Int mapSeedOffset, int x, int z, int waterThreshold)
    {
        GameManager.CustomNoiseSettings.WorldOffset = mapSeedOffset;
        int groundPosition = getSurfaceHeightNoise(WorldPosition.x + x, WorldPosition.z + z);
        
        for (int y = 0; y < ChunkHeight; y++)
        {
            BlockType voxelType = BlockType.Dirt;
            if (y > groundPosition)
                voxelType = y < waterThreshold ? BlockType.Water : BlockType.Air;
            else if (y == groundPosition && y < waterThreshold)
                voxelType = BlockType.Sand;
            else if (y == groundPosition)
                voxelType = BlockType.GrassDirt;
            SetBlock(new Vector3Int(x, y, z), voxelType);
        }
    }

    private int getSurfaceHeightNoise(int x, int z)
    {
        float terrainHeight = CustomNoise.OctavePerlin(x, z, GameManager.CustomNoiseSettings);
        terrainHeight = CustomNoise.Redistribution(terrainHeight, GameManager.CustomNoiseSettings);
        int surfaceHeight = CustomNoise.RemapValue01Int(terrainHeight, 0f, ChunkHeight);
        return surfaceHeight;
    }
    
    private bool inRange(Vector3Int localPosition)
    {
        return inRangeHorizontal(localPosition.x) && inRangeHorizontal(localPosition.z) &&
               inRangeVertical(localPosition.y);
    }
    
    private bool inRangeHorizontal(int axisCoordinate)
    {
        if (axisCoordinate < 0 || axisCoordinate >= ChunkSize)
            return false;
        return true;
    }

    private bool inRangeVertical(int yCoord)
    {
        return yCoord >= 0 && yCoord < ChunkHeight;
    }
}
