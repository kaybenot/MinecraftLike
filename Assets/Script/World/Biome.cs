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
    
    
}


