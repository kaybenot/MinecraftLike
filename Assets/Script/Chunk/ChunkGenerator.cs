using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class ChunkGenerator
{
    public Chunk Chunk { get; }
    public TreeData TreeData { get; private set; }

    public ChunkGenerator(Chunk chunk)
    {
        Chunk = chunk;
    }
    
    public void GenerateChunk(Vector2Int mapSeedOffset)
    {
        BiomeGenerationSelection biomeSelection = selectBiomeGenerator(Chunk.WorldPosition, false);
        TreeData = biomeSelection.Biome.GetTreeData(Chunk, mapSeedOffset);
        for (int x = 0; x < Chunk.Size; x++)
        for (int z = 0; z < Chunk.Size; z++)
        {
            var selection = selectBiomeGenerator(new Vector3Int(Chunk.WorldPosition.x + x, 0, Chunk.WorldPosition.z + z));
            selection.Biome.GenerateBlockColumn(Chunk, mapSeedOffset, x, z, selection.TerrainSurfaceNoise);
        }
    }
    
    private BiomeGenerationSelection selectBiomeGenerator(Vector3Int worldPosition, bool useDomainWarping = true)
    {
        if (useDomainWarping)
        {
            Vector2Int domainOffset = Vector2Int.RoundToInt(
                GameManager.BiomeGenerator.BiomeWarping.GenerateDomainOffset(worldPosition.x, worldPosition.z));
            worldPosition += new Vector3Int(domainOffset.x, 0, domainOffset.y);
        }

        List<BiomeSelectionHelper> biomeSelectionHelpers = getBiomeGeneratorSelectionHelpers(worldPosition);
        Biome biome_1 = selectBiome(biomeSelectionHelpers[0].Index);
        Biome biome_2 = selectBiome(biomeSelectionHelpers[1].Index);
        float distance = Vector3.Distance(GameManager.BiomeGenerator.BiomeCenters[biomeSelectionHelpers[0].Index],
            GameManager.BiomeGenerator.BiomeCenters[biomeSelectionHelpers[1].Index]);
        float weight_0 = biomeSelectionHelpers[1].Distance / distance;
        float weight_1 = 1 - weight_0;
        int terrainHeightNoise_0 = biome_1.GetSurfaceHeight(worldPosition.x, worldPosition.z, Chunk.Height);
        int terrainHeightNoise_1 = biome_2.GetSurfaceHeight(worldPosition.x, worldPosition.z, Chunk.Height);
        return new BiomeGenerationSelection(biome_1, Mathf.RoundToInt(terrainHeightNoise_0 * weight_0 + terrainHeightNoise_1 * weight_1));
    }
    
    private Biome selectBiome(int index)
    {
        float temp = GameManager.BiomeGenerator.BiomeNoise[index];
        foreach (var data in GameManager.BiomeGenerator.BiomeDatas)
        {
            if (temp >= data.TemperatureStartThreshold && temp < data.TemperatureEndThreshold)
                return data.Biome;
        }

        return GameManager.BiomeGenerator.BiomeDatas[0].Biome;
    }
    
    private List<BiomeSelectionHelper> getBiomeGeneratorSelectionHelpers(Vector3Int worldPosition)
    {
        worldPosition.y = 0;
        return getClosestBiomeIndex(worldPosition);
    }

    private List<BiomeSelectionHelper> getClosestBiomeIndex(Vector3Int worldPosition)
    {
        return GameManager.BiomeGenerator.BiomeCenters.Select((center, index) => new BiomeSelectionHelper()
        {
            Index = index,
            Distance = Vector3.Distance(center, worldPosition)
        }).OrderBy(helper => helper.Distance).Take(4).ToList();
    }
    
    private struct BiomeSelectionHelper
    {
        public int Index;
        public float Distance;
    }
}