using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Block Atlas", menuName = "Engine/Block Atlas")]
public class BlockAtlas : ScriptableObject
{
    public float TileWidth, TileHeight;
    public List<BlockData> BlockDatas;
}

[Serializable]
public class BlockData
{
    public BlockType BlockType;
    public ItemType DropType;
    public int MinDropAmount, MaxDropAmount;
    public Vector2Int up, down, side;
    public bool isSolid = true;
    public bool generatesCollider = true;
}
