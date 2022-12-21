using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingHandler : MonoBehaviour
{
    public List<CraftingRecipe> CraftingRecipes;

    public static CraftingHandler Singleton;

    private void Awake()
    {
        Singleton = this;
    }
}
