using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RemoveSaveButton : MonoBehaviour
{
    [SerializeField] private int slot = -1;

    public void RemoveSave()
    {
        var path = "Saves/Save" + slot;
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
            GetComponentInParent<SaveManager>().CheckSavesAndRefreshButtons();
        }
        else
            Debug.Log("Save does not exist!");
    }
}
