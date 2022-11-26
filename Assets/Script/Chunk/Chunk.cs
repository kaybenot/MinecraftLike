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
    public Biome Biome { get; private set; }
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
        
        // TODO: Implement picking biomes
        Biome = GameObject.FindWithTag("PlainsBiome").GetComponent<Biome>();
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

    public bool IsOnEdge(Vector3Int globalPosition)
    {
        Vector3Int localPos = GetLocalPosition(globalPosition);
        return localPos.x == 0 || localPos.x == ChunkSize - 1 || localPos.y == 0 || localPos.y == ChunkHeight - 1 ||
               localPos.z == 0 || localPos.z == ChunkSize - 1;
    }

    public IEnumerable<Chunk> GetEdgeNeighbourChunk(Vector3Int globalPosition)
    {
        List<Chunk> neighboursToUpdate = new List<Chunk>();
        var localPos = GetLocalPosition(globalPosition);
        if(localPos.x == 0)
            neighboursToUpdate.Add(GameManager.World.GetChunk(globalPosition + Vector3Int.left));
        else if(localPos.x == ChunkSize - 1)
            neighboursToUpdate.Add(GameManager.World.GetChunk(globalPosition + Vector3Int.right));
        else if(localPos.z == 0)
            neighboursToUpdate.Add(GameManager.World.GetChunk(globalPosition + Vector3Int.back));
        else if(localPos.z == ChunkSize - 1)
            neighboursToUpdate.Add(GameManager.World.GetChunk(globalPosition + Vector3Int.forward));
        return neighboursToUpdate;
    }

    private void processChunkColumn(Vector2Int mapSeedOffset, int x, int z, int waterThreshold)
    {
        GameManager.CustomNoiseSettings.WorldOffset = mapSeedOffset;
        int groundPosition = getSurfaceHeightNoise(WorldPosition.x + x, WorldPosition.z + z);
        
        for (int y = 0; y < ChunkHeight; y++)
            Biome.StartLayer.Handle(this, new Vector3Int(x, y, z), groundPosition, mapSeedOffset);
        
        foreach (var layer in Biome.AdditionalLayers)
            layer.Handle(this, new Vector3Int(x, WorldPosition.y, z), groundPosition, mapSeedOffset);
    }

    private int getSurfaceHeightNoise(int x, int z)
    {
        float terrainHeight;
        if (Biome.UseDomainWarping)
            terrainHeight = Biome.DomainWarping.GenerateDomainNoise(x, z, GameManager.CustomNoiseSettings);
        else
            terrainHeight = CustomNoise.OctavePerlin(x, z, GameManager.CustomNoiseSettings);
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
