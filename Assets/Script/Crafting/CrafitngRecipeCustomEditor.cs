using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CraftingRecipe))]
public class CrafitngRecipeCustomEditor : Editor
{
    private CraftingRecipe recipe;

    private void OnEnable()
    {
        recipe = (CraftingRecipe)target;
        if (recipe.Ingredients == null)
            recipe.Ingredients = new ItemType[3, 3];
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Recipe", EditorStyles.boldLabel);
        for(var x = 0; x < 3; x++)
        {
            EditorGUILayout.BeginHorizontal();
            for (var y = 0; y < 3; y++)
                recipe.Ingredients[x, y] = (ItemType)EditorGUILayout.EnumFlagsField(recipe.Ingredients[x, y]);
            EditorGUILayout.EndHorizontal();
        }
    }
}
