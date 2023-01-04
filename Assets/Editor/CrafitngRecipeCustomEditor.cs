using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CraftingRecipe))]
public class CrafitngRecipeCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        serializedObject.Update();
        
        var recipe = (CraftingRecipe) target;
        
        if (recipe.Ingredients == null)
            recipe.Ingredients = new ItemType[9];

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Recipe", EditorStyles.boldLabel);
        for(var y = 2; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (var x = 0; x < 3; x++)
                recipe.Ingredients[x + y * 3] = (ItemType)EditorGUILayout.EnumFlagsField(recipe.Ingredients[x + y * 3]);
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
