using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public Block[,,] Blocks { get; }
    public int Size { get; }
    public int Height { get; }
    public World World => GameManager.World;
    public Vector3Int WorldPosition { get; }
    public static Biome Biome { get; set; }
    public bool ModifiedByPlayer { get; set; }
    public TreeData TreeData { get; private set; }

    public Chunk(int size, int height, Vector3Int worldPosition)
    {
        Size = size;
        Height = height;
        WorldPosition = worldPosition;
        Blocks = new Block[size, height, size];
        for (int x = 0; x < size; x++)
            for(int y = 0; y < height; y++)
                for(int z = 0; z < size; z++)
                    Blocks[x, y, z] = new Block(new Vector3Int(x, y, z), BlockType.Air);
        ModifiedByPlayer = false;
        
        // TODO: Implement picking biomes
    }

    /// <summary>
    /// Generates chunk.
    /// </summary>
    /// <param name="mapSeedOffset">Map seed</param>
    public void GenerateChunk(Vector2Int mapSeedOffset)
    {
        TreeData = Biome.GetTreeData(this, mapSeedOffset);
        for (int x = 0; x < Size; x++)
        for (int z = 0; z < Size; z++)
            generateBlockColumn(mapSeedOffset, x, z);
    }

    /// <summary>
    /// Generates mesh based on current chunk data.
    /// </summary>
    /// <returns>Chunk mesh.</returns>
    public ChunkMesh GetChunkMesh()
    {
        ChunkMesh chunkMesh = new ChunkMesh(true);

        foreach (var block in Blocks)
            chunkMesh.TryAddBlock(this, block);

        return chunkMesh;
    }

    /// <summary>
    /// Sets block to specific type.
    /// </summary>
    /// <param name="localPosition">Block position in local position</param>
    /// <param name="blockType">Type to which block has to be set</param>
    public void SetBlock(Vector3Int localPosition, BlockType blockType)
    {
        if (inRange(localPosition))
            Blocks[localPosition.x, localPosition.y, localPosition.z].BlockType = blockType;
        else
            World.SetBlock(WorldPosition + localPosition, blockType);
    }
    
    /// <summary>
    /// Gets block at local position.
    /// </summary>
    /// <param name="localPosition">Local position</param>
    /// <returns>Block</returns>
    public Block GetBlock(Vector3Int localPosition)
    {
        return inRange(localPosition) ? Blocks[localPosition.x, localPosition.y, localPosition.z] : 
            World.GetBlock(WorldPosition + localPosition);
    }

    /// <summary>
    /// Converts global position to chunk position.
    /// </summary>
    /// <param name="globalPosition">Global position</param>
    /// <returns>Local position</returns>
    public Vector3Int GetLocalPosition(Vector3Int globalPosition)
    {
        return new Vector3Int(
            globalPosition.x - WorldPosition.x,
            globalPosition.y - WorldPosition.y,
            globalPosition.z - WorldPosition.z);
    }

    /// <summary>
    /// Checks if block is on edge.
    /// </summary>
    /// <param name="globalPosition">Block global position</param>
    /// <returns>True/False</returns>
    public bool IsOnEdge(Vector3Int globalPosition)
    {
        Vector3Int localPos = GetLocalPosition(globalPosition);
        return localPos.x == 0 || localPos.x == Size - 1 || localPos.y == 0 || localPos.y == Height - 1 ||
               localPos.z == 0 || localPos.z == Size - 1;
    }

    /// <summary>
    /// List all chunks touched by block.
    /// </summary>
    /// <param name="globalPosition">Block global position</param>
    /// <returns>Chunks that blocks touches</returns>
    public IEnumerable<Chunk> GetTouchedChunks(Vector3Int globalPosition)
    {
        List<Chunk> neighboursToUpdate = new List<Chunk>();
        var localPos = GetLocalPosition(globalPosition);
        
        if(localPos.x == 0)
            neighboursToUpdate.Add(World.GetChunk(globalPosition + Vector3Int.left));
        else if(localPos.x == Size - 1)
            neighboursToUpdate.Add(World.GetChunk(globalPosition + Vector3Int.right));
        else if(localPos.z == 0)
            neighboursToUpdate.Add(World.GetChunk(globalPosition + Vector3Int.back));
        else if(localPos.z == Size - 1)
            neighboursToUpdate.Add(World.GetChunk(globalPosition + Vector3Int.forward));
        
        return neighboursToUpdate;
    }

    private void generateBlockColumn(Vector2Int mapSeedOffset, int x, int z)
    {
        GameManager.CustomNoiseSettings.WorldOffset = mapSeedOffset;
        int groundPosition = getSurfaceHeight(WorldPosition.x + x, WorldPosition.z + z);
        
        for (int y = 0; y < Height; y++)
            Biome.StartLayer.Handle(this, new Vector3Int(x, y, z), groundPosition, mapSeedOffset);
        
        foreach (var layer in Biome.AdditionalLayers)
            layer.Handle(this, new Vector3Int(x, WorldPosition.y, z), groundPosition, mapSeedOffset);
    }

    private int getSurfaceHeight(int x, int z)
    {
        float surfaceHeight = Biome.UseDomainWarping ? Biome.DomainWarping.GenerateDomainNoise(x, z, GameManager.CustomNoiseSettings) : 
            CustomNoise.OctavePerlin(x, z, GameManager.CustomNoiseSettings);
        surfaceHeight = CustomNoise.Redistribution(surfaceHeight, GameManager.CustomNoiseSettings);
        
        return CustomNoise.RemapValue01Int(surfaceHeight, 0f, Height);
    }

    private bool inRange(Vector3Int localPosition)
    {
        return localPosition.x >= 0 && localPosition.x < Size && localPosition.z >= 0 && localPosition.z < Size &&
               localPosition.y >= 0 && localPosition.y < Height;
    }
}
