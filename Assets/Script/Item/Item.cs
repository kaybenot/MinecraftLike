using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public ItemType ItemType { get; }
    public int Amount { get; set; }

    public Item(ItemType itemType)
    {
        ItemType = itemType;
        Amount = 1;
    }
    
    public Rect IconRect
    {
        get
        {
            var uvSide = Block.BlockDatas[(BlockType)ItemType].side;
            var tileWidth = GameManager.BlockAtlas.TileWidth;
            var tileHeight = GameManager.BlockAtlas.TileHeight;
            return new Rect(uvSide.x * tileWidth, uvSide.y * tileHeight, tileWidth, tileHeight);
        }
    }
}
