using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block 
{
    private static Dictionary<BlockType, BlockData> BlockDatas = new Dictionary<BlockType, BlockData>();
    
    public Vector3Int Location { get; }
    public BlockType BlockType { get; set; }
    public BlockData BlockData => BlockDatas[BlockType];

    public Block(Vector3Int location, BlockType blockType)
    {
        Location = location;
        BlockType = blockType;
    }
    
    public static void AddBlockData(BlockData blockData)
    {
        if (BlockDatas.ContainsKey(blockData.BlockType) == false)
            BlockDatas.Add(blockData.BlockType, blockData);
    }
}
