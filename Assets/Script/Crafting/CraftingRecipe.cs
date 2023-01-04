using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingRecipe", menuName = "Engine/CraftingRecipe")]
public class CraftingRecipe : ScriptableObject
{
    public Vector2Int RecipeSize;
    public ItemType Output;
    public int OutputAmount;
    public ItemType[] Ingredients;
}
