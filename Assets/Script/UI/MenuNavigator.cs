using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuNavigator : MonoBehaviour
{
    [SerializeField] private GameObject menuToDisable;
    [SerializeField] private GameObject menuToEnable;

    public void OnClick()
    {
        menuToDisable.SetActive(false);
        menuToEnable.SetActive(true);
    }
}
