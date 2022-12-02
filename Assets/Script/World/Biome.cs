using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct BiomeData
{
    [Range(0f, 1f)] public float TemperatureStartThreshold, TemperatureEndThreshold;
    public Biome Biome;
}

public class Biome : MonoBehaviour
{
    /// <summary>
    /// A layer from which chunk generation begins.
    /// </summary>
    public BlockLayer StartLayer;
    /// <summary>
    /// Additional layers, called after main layers calculations.
    /// </summary>
    public List<BlockLayer> AdditionalLayers;
    public bool UseDomainWarping = true;
    /// <summary>
    /// Domain warping scriptable object.
    /// </summary>
    public DomainWarping DomainWarping;
    public TreeGenerator TreeGenerator;

    public TreeData GetTreeData(Chunk chunk, Vector2Int mapSeedOffset)
    {
        if (TreeGenerator == null)
            return new TreeData();;
        return TreeGenerator.GenerateTreeData(chunk, mapSeedOffset);
    }
    
    public int GetSurfaceHeight(int x, int z, int chunkHeight)
    {
        float surfaceHeight = UseDomainWarping ? DomainWarping.GenerateDomainNoise(x, z, GameManager.CustomNoiseSettings) : 
            CustomNoise.OctavePerlin(x, z, GameManager.CustomNoiseSettings);
        surfaceHeight = CustomNoise.Redistribution(surfaceHeight, GameManager.CustomNoiseSettings);
        
        return CustomNoise.RemapValue01Int(surfaceHeight, 0f, chunkHeight);
    }
    
    public void GenerateBlockColumn(Chunk chunk, Vector2Int mapSeedOffset, int x, int z, int? terrainSurfaceNoise)
    {
        GameManager.CustomNoiseSettings.WorldOffset = mapSeedOffset;
        int groundPosition;
        if (!terrainSurfaceNoise.HasValue)
            groundPosition = GetSurfaceHeight(chunk.WorldPosition.x + x, chunk.WorldPosition.z + z, chunk.Height);
        else
            groundPosition = terrainSurfaceNoise.Value;
        
        for (int y = 0; y < chunk.Height; y++)
            StartLayer.Handle(chunk, new Vector3Int(x, y, z), groundPosition, mapSeedOffset);
        
        foreach (var layer in AdditionalLayers)
            layer.Handle(chunk, new Vector3Int(x, chunk.WorldPosition.y, z), groundPosition, mapSeedOffset);
    }
}

public class BiomeGenerationSelection
{
    public Biome Biome;
    public int? TerrainSurfaceNoise;

    public BiomeGenerationSelection(Biome biome, int? terrainSurfaceNoise = null)
    {
        Biome = biome;
        TerrainSurfaceNoise = terrainSurfaceNoise;
    }
}
