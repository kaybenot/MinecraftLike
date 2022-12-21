using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingRecipe", menuName = "Engine/CraftingRecipe")]
public class CraftingRecipe : ScriptableObject
{
    /// <summary>
    /// If recipe is not big, use only first 2 values!
    /// </summary>
    public bool BigRecipe = true;
    public ItemType Output;
    public ItemType[,] Ingredients;
}
