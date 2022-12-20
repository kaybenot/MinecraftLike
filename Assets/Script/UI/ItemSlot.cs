using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private GameObject image;
    [SerializeField] private GameObject selectImage;
    [SerializeField] private TMP_Text text;
    
    public Item Item
    {
        get => item;
        set
        {
            item = value;
            UpdateSlot();
        }
    }
    public int SlotIndex { get; set; }
    public Inventory Inventory { get; set; }

    private Item item;

    private void UpdateSlot()
    {
        Inventory.Items[SlotIndex] = item;
        if (item != null && item.ItemType != ItemType.Nothing)
        {
            image.SetActive(true);
            text.gameObject.SetActive(true);
            text.text = item.Amount.ToString();

            RawImage rawImage = GetComponentInChildren<RawImage>();
            rawImage.uvRect = item.IconRect;
        }
        else
        {
            image.SetActive(false);
            text.gameObject.SetActive(false);
        }
    }

    public void Select()
    {
        selectImage.SetActive(true);
    }

    public void Deselect()
    {
        selectImage.SetActive(false);
    }

    public void Click()
    {
        var mouseItem = GameMouse.Item;
        GameMouse.Item = item;
        Item = mouseItem;
    }
}
