using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingSlot : MonoBehaviour
{
    [SerializeField] private Vector2Int slotPosition;
    [SerializeField] private GameObject image;
    [SerializeField] private TMP_Text text;
    [SerializeField] private bool resultSlot;
    
    public static CraftingSlot ResultSlot { get; private set; }
    public static List<CraftingSlot> CraftingSlots { get; private set; } = new List<CraftingSlot>();
    
    public Item Item
    {
        get => item;
        set
        {
            item = value;
            UpdateSlot();
        }
    }

    private Item item = new Item(ItemType.Nothing);

    private void OnDisable()
    {
        GameManager.Player.DropFromPlayer(item);
        item = new Item(ItemType.Nothing);
        UpdateSlot();
    }

    private void Awake()
    {
        if (resultSlot)
            ResultSlot = this;
        else
            CraftingSlots.Add(this);
    }

    private void UpdateSlot()
    {
        CraftingUI.Singleton.CraftingGrid[slotPosition.x, slotPosition.y] = item;
        if (item != null && item.ItemType != ItemType.Nothing)
        {
            image.SetActive(true);
            text.gameObject.SetActive(true);
            text.text = item.Amount.ToString();

            var rawImage = GetComponentInChildren<RawImage>();
            rawImage.uvRect = item.IconRect;
        }
        else
        {
            image.SetActive(false);
            text.gameObject.SetActive(false);
        }
    }

    public void Click()
    {
        if(resultSlot && ResultSlot.Item != null && ResultSlot.Item.ItemType != ItemType.Nothing &&
           (GameMouse.Item == null || GameMouse.Item.ItemType == ItemType.Nothing))
        {
            CraftingSlots.ForEach(slot => slot.item = new Item(ItemType.Nothing));
            GameMouse.Item = ResultSlot.Item;
            ResultSlot.Item = new Item(ItemType.Nothing);
        }
        else
        {
            var mouseItem = GameMouse.Item;
            
            // If different items
            if(GameMouse.HasItem && Item.Exists && mouseItem.ItemType != Item.ItemType)
                return;
            
            
            
            if (!Item.Exists && GameMouse.HasItem)
            {
                Item = new Item(mouseItem.ItemType);
                mouseItem.DecreaseAmount();
                GameMouse.UpdateVisuals();
            }
            else if (Item.ItemType == mouseItem.ItemType)
            {
                Item.Amount++;
                UpdateSlot();
                mouseItem.DecreaseAmount();
                GameMouse.UpdateVisuals();
            }
            else if(Item.Exists && !GameMouse.HasItem)
            {
                GameMouse.Item = Item;
                Item = new Item(ItemType.Nothing);
            }

            var recipe = CraftingUI.Singleton.GetRecipe();
            if (recipe != ItemType.Nothing)
                ResultSlot.Item = new Item(recipe);
            else if(ResultSlot.Item != null && ResultSlot.Item.ItemType != ItemType.Nothing)
                ResultSlot.Item = new Item(ItemType.Nothing);
            ResultSlot.UpdateSlot();
        }
    }
}