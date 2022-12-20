using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameMouse : MonoBehaviour
{
    [SerializeField] private GameObject mouseItem;
    
    public static Item Item
    {
        set
        {
            item = value ?? new Item(ItemType.Nothing);
            if (value != null && value.ItemType != ItemType.Nothing)
            {
                isDragging = true;
                _mouseItem.SetActive(true);
                _mouseItem.GetComponentInChildren<RawImage>().uvRect = value.IconRect;
                _mouseItem.GetComponentInChildren<TMP_Text>().text = value.Amount.ToString();
                if (rt == null)
                    rt = _mouseItem.GetComponent<RectTransform>();
            }
            else
            {
                isDragging = false;
                _mouseItem.SetActive(false);
            }
        }
        get => item;
    }
    
    private static bool isDragging = false;
    private static Item item = new Item(ItemType.Nothing);
    private static GameObject _mouseItem;
    private static RectTransform rt;

    private void Awake()
    {
        _mouseItem = mouseItem;
    }

    private void Update()
    {
        if (isDragging)
        {
            rt.position = Input.mousePosition;
        }
    }
}
