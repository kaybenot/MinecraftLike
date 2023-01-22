using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemAtlas", menuName = "Engine/Item Atlas")]
public class ItemAtlas : ScriptableObject
{
    public float TileWidth, TileHeight;
    public List<ItemData> ItemDatas;
}

[Serializable]
public class ItemData
{
    public ItemType Type;
    public Vector2Int TilePosition;
}
