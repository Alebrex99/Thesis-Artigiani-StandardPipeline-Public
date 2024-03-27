using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    //Fade screen
    public FadeScreen fadeScreen;

    //Scene transition
    [SerializeField] private LoadSceneMode _loadSceneMode;
    [SerializeField] private string _sceneToLoad;
    [SerializeField] private string _sceneToUnload;
    [SerializeField] private bool _unloadSceneOnExit;
    private bool _buttonExit = false;
    private AsyncOperation asyncLoadOperation; //restituito dalle operazioni asincrone per determinare se op è completata, usabile per progress bar


    public void onButtonChangeScene()
    {
        //premi il bottone -> carica scena office + scarica environemnt
        if (_buttonExit) //sono in uscita dall'ufficio
        {
            if (!_unloadSceneOnExit)
                return;
            Scene sceneToUnload = SceneManager.GetSceneByName(_sceneToUnload);
            if (!sceneToUnload.IsValid() || !sceneToUnload.isLoaded)
                return;

            StartCoroutine(UnloadScene(sceneToUnload));
            
        }
        else //sono in entrata nell'ufficio
        {
            StartCoroutine(LoadScene(_sceneToLoad));
            _buttonExit = true;
        }


        //premi ancora il bottone -> carica scena environment + scarica office
    }

    private IEnumerator LoadScene(string sceneToLoadName)
    {
        Scene sceneToLoad = SceneManager.GetSceneByName(sceneToLoadName);
        if (sceneToLoad.IsValid() && sceneToLoad.isLoaded)
            yield break;

        asyncLoadOperation = SceneManager.LoadSceneAsync(sceneToLoadName, _loadSceneMode);
        while (!asyncLoadOperation.isDone)
        {
            yield return null;
        }
        
        asyncLoadOperation = null;
        Scene loadedScene = SceneManager.GetSceneByName(sceneToLoadName);
        SceneManager.SetActiveScene(loadedScene);

    }

    private IEnumerator UnloadScene(Scene sceneToUnload)
    {
        while (asyncLoadOperation != null && !asyncLoadOperation.isDone)
        {
            yield return null;
        }

        SceneManager.UnloadSceneAsync(sceneToUnload);
    }





    //FUNZIONI PER CAMBIO SCENA ESPOSTE:
    //unloading scene environment per eliminare l'ambiente base quando premo pulsante
    public void UnloadScene(int sceneIndex)
    {
        StartCoroutine(UnloadSceneRoutine(sceneIndex));
    }

    private IEnumerator UnloadSceneRoutine(int sceneIndex)
    {
        while (asyncLoadOperation != null && !asyncLoadOperation.isDone)
        {
            yield return null;
        }

        SceneManager.UnloadSceneAsync(sceneIndex);
    }

    //FUNZIONI GENERICHE PER CAMBIO SCENA:
    //Meglio usare le Async, perchè le base frizzano il gioco, caricano e fanno ripartire tutto
    public void GoToSceneAsync(int sceneIndex)
    {
        StartCoroutine(GoToSceneAsyncRoutine(sceneIndex));
    }

    IEnumerator GoToSceneAsyncRoutine(int sceneIndex)
    {
        fadeScreen.FadeOut();
        asyncLoadOperation.allowSceneActivation = false;

        float timer = 0;
        while (timer <= fadeScreen.fadeDuration && !asyncLoadOperation.isDone)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        asyncLoadOperation.allowSceneActivation = true;
    }

    //COROUTINES: eseguite (di default) una volta per frame (yield return null) quando lanciate 1 volta per es. con il bottone
  

}
