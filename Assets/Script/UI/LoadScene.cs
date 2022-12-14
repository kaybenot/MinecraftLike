using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private int slot = -1;
    [SerializeField] private int sceneIndex = 1;
    [SerializeField] private ProgressBar progressBar;
    
    private AsyncOperation operation;

    public void Load()
    {
        progressBar.gameObject.SetActive(true);
        operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;
        progressBar.SetDescription("Loading scene");

        GameState.SaveSlot = slot;

        progressBar.Coroutine(sceneLoading());
    }

    IEnumerator sceneLoading()
    {
        while (!operation.isDone)
        {
            progressBar.SetProgress(operation.progress);
            if (operation.progress >= 0.9f)
                operation.allowSceneActivation = true;
            yield return new WaitForEndOfFrame();
        }
    }
}
