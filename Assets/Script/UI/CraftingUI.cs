using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingUI : MonoBehaviour
{
    public Item[,] CraftingGrid = new Item[3, 3];
    
    public static CraftingUI Singleton;

    public ItemType GetRecipe()
    {
        foreach (var recipe in CraftingHandler.Singleton.CraftingRecipes)
        {
            for (var x = 0; x <= 3 - recipe.RecipeSize.x; x++)
            for (var y = 0; y <= 3 - recipe.RecipeSize.y; y++)
            {
                var matches = true;

                for (var i = 0; i < recipe.RecipeSize.x; i++)
                for (var j = 0; j < recipe.RecipeSize.y; j++)
                {
                    CraftingGrid[x + i, y + j] ??= new Item(ItemType.Nothing);
                    if (CraftingGrid[x + i, y + j].ItemType != recipe.Ingredients[i + j * 3])
                        matches = false;
                }

                if (matches && otherEmpty(x, y, recipe))
                    return recipe.Output;
            }
        }

        return ItemType.Nothing;
    }

    private bool otherEmpty(int x, int y, CraftingRecipe recipe)
    {
        for (var i = 0; i <= 3 - recipe.RecipeSize.x; i++)
        for (var j = 0; j <= 3 - recipe.RecipeSize.y; j++)
        {
            if(i < x + recipe.RecipeSize.x && i >= x && j < y + recipe.RecipeSize.y && j >= y)
                continue;
            
            CraftingGrid[i, j] ??= new Item(ItemType.Nothing);
            if (CraftingGrid[i, j].ItemType != ItemType.Nothing)
                return false;
        }

        return true;
    }

    public CraftingUI()
    {
        Singleton = this;
    }
}
