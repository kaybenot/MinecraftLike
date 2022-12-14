using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> saveButtons;
    
    void Start()
    {
        CheckSavesAndRefreshButtons();
    }

    public void CheckSavesAndRefreshButtons()
    {
        var saves = Save.CheckIfSavesExist();
        for (int slot = 0; slot < 3; slot++)
        {
            TMP_Text text = saveButtons[slot].GetComponentInChildren<TMP_Text>();

            if (!saves[slot])
                text.text = "New World";
            else
                text.text = "Play";
        }
    }
    
}
