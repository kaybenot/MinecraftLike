using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BlockAtlas blockAtlas;
    [SerializeField] private CustomNoiseSettings customNoiseSettings;
    [SerializeField] private float textureOffset = 0.001f;

    public static BlockAtlas BlockAtlas { get; private set; }
    public static float TextureOffset { get; private set; }
    public static CustomNoiseSettings CustomNoiseSettings { get; private set; }

    private void Awake()
    {
        BlockAtlas = blockAtlas;
        TextureOffset = textureOffset;
        CustomNoiseSettings = customNoiseSettings;
        
        loadBlockDatas();
    }

    private void loadBlockDatas()
    {
        foreach (var blockData in BlockAtlas.BlockDatas)
            Block.AddBlockData(blockData);
    }
}
