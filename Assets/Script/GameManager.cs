using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BlockAtlas blockAtlas;
    [SerializeField] private float textureOffset = 0.001f;

    public static BlockAtlas BlockAtlas { get; private set; }
    public static float TextureOffset { get; private set; }

    private void Awake()
    {
        BlockAtlas = blockAtlas;
        TextureOffset = textureOffset;
        
        loadBlockDatas();
    }

    private void loadBlockDatas()
    {
        foreach (var blockData in BlockAtlas.BlockDatas)
            Block.AddBlockData(blockData);
    }
}
