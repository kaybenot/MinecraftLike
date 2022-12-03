using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chunk
{
    public Block[,,] Blocks { get; }
    public int Size { get; }
    public int Height { get; }
    public World World => GameManager.World;
    public Vector3Int WorldPosition { get; }
    public TreeData TreeData { get; private set; }
    
    public static Dictionary<Vector3Int, Chunk> Chunks = new Dictionary<Vector3Int, Chunk>();

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

        // TODO: Implement picking biomes
    }
    
    public void GenerateChunk(Vector2Int mapSeedOffset)
    {
        BiomeGenerationSelection biomeSelection = selectBiomeGenerator(WorldPosition, false);
        TreeData = biomeSelection.Biome.GetTreeData(this, mapSeedOffset);
        for (int x = 0; x < Size; x++)
        for (int z = 0; z < Size; z++)
        {
            var selection = selectBiomeGenerator(new Vector3Int(WorldPosition.x + x, 0, WorldPosition.z + z));
            selection.Biome.GenerateBlockColumn(this, mapSeedOffset, x, z, selection.TerrainSurfaceNoise);
        }
    }
    
    public ChunkMesh GetChunkMesh()
    {
        ChunkMesh chunkMesh = new ChunkMesh(true);

        foreach (var block in Blocks)
            chunkMesh.TryAddBlock(this, block);

        return chunkMesh;
    }
    
    public void SetBlock(Vector3Int localPosition, BlockType blockType)
    {
        if (inRange(localPosition))
            Blocks[localPosition.x, localPosition.y, localPosition.z].BlockType = blockType;
        else
            World.SetBlock(WorldPosition + localPosition, blockType);
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
        return localPos.x == 0 || localPos.x == Size - 1 || localPos.y == 0 || localPos.y == Height - 1 ||
               localPos.z == 0 || localPos.z == Size - 1;
    }
    
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
        int terrainHeightNoise_0 = biome_1.GetSurfaceHeight(worldPosition.x, worldPosition.z, Height);
        int terrainHeightNoise_1 = biome_2.GetSurfaceHeight(worldPosition.x, worldPosition.z, Height);
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

    private struct BiomeSelectionHelper
    {
        public int Index;
        public float Distance;
    }

    private List<BiomeSelectionHelper> getClosestBiomeIndex(Vector3Int worldPosition)
    {
        return GameManager.BiomeGenerator.BiomeCenters.Select((center, index) => new BiomeSelectionHelper()
        {
            Index = index,
            Distance = Vector3.Distance(center, worldPosition)
        }).OrderBy(helper => helper.Distance).Take(4).ToList();
    }

    private bool inRange(Vector3Int localPosition)
    {
        return localPosition.x >= 0 && localPosition.x < Size && localPosition.z >= 0 && localPosition.z < Size &&
               localPosition.y >= 0 && localPosition.y < Height;
    }
}
