using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private GameObject image;
    [SerializeField] private TMP_Text text;
    
    public ItemType Type
    {
        get { return type; }
        set
        {
            type = value;
            UpdateSlot();
        }
    }
    
    public int Amount
    {
        get { return amount; }
        set
        {
            amount = value;
            UpdateSlot();
        }
    }

    private ItemType type;
    private int amount;
    
    private void UpdateSlot()
    {
        var uvSide = Block.BlockDatas[(BlockType)type].side;

        if(type != ItemType.Nothing)
        {
            var tileWidth = GameManager.BlockAtlas.TileWidth;
            var tileHeight = GameManager.BlockAtlas.TileHeight;

            image.SetActive(true);
            text.gameObject.SetActive(true);
            text.text = amount.ToString();

            RawImage rawImage = GetComponentInChildren<RawImage>();
            rawImage.uvRect = new Rect(uvSide.x * tileWidth, uvSide.y * tileHeight, tileWidth, tileHeight);
        }
    }
}
