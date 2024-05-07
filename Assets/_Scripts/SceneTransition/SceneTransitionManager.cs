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
    [SerializeField] private string _sceneIn;
    [SerializeField] private string _sceneOut;
    [SerializeField] private bool _unloadSceneOnExit;
    private bool _buttonInOut = true;
    private AsyncOperation asyncLoadOperation; //restituito dalle operazioni asincrone per determinare se op è completata, usabile per progress bar


    public void onButtonChangeScene()
    {
        //quando lo premi per la prima volta -> carica scena office + scarica environment
        //quando lo premi per la seconda volta -> carica scena environment + scarica office
        string sceneToLoad;
        string sceneToUnload;
        bool sceneWay = _buttonInOut;

        if (sceneWay)
        {
            sceneToLoad = _sceneIn;
            sceneToUnload = _sceneOut;
        }
        else
        {
            sceneToLoad = _sceneOut;
            sceneToUnload = _sceneIn;
        }
        StartCoroutine(ChangeScene(sceneToLoad, sceneToUnload));

        _buttonInOut = !_buttonInOut;

    }

    private IEnumerator ChangeScene(string sceneToLoadName, string sceneToUnloadName)
    {
        // Controlla se la scena da caricare è già caricata
        Scene sceneToLoad = SceneManager.GetSceneByName(sceneToLoadName);
        if (sceneToLoad.IsValid() && sceneToLoad.isLoaded)
        {
            Debug.Log("La scena " + sceneToLoadName + " è già caricata.");
            yield break;
        }

        // Carica la scena
        asyncLoadOperation = SceneManager.LoadSceneAsync(sceneToLoadName, _loadSceneMode);
        while (!asyncLoadOperation.isDone)
        {
            Debug.Log("Caricamento della scena " + sceneToLoadName + " in corso...");
            yield return null;
        }
        Debug.Log("Scena " + sceneToLoadName + " caricata con successo.");

        // Controlla se la scena da scaricare è già scaricata
        Scene sceneToUnload = SceneManager.GetSceneByName(sceneToUnloadName);
        if (!sceneToUnload.IsValid() || !sceneToUnload.isLoaded)
        {
            Debug.Log("La scena " + sceneToUnloadName + " è già scaricata.");
            yield break;
        }

        // Scarica la scena
        asyncLoadOperation = SceneManager.UnloadSceneAsync(sceneToUnloadName);
        while (!asyncLoadOperation.isDone)
        {
            Debug.Log("Scaricamento della scena " + sceneToUnloadName + " in corso...");
            yield return null;
        }
        Debug.Log("Scena " + sceneToUnloadName + " scaricata con successo.");

        // Imposta la scena caricata come scena attiva
        Scene loadedScene = SceneManager.GetSceneByName(sceneToLoadName);
        SceneManager.SetActiveScene(loadedScene);
        Debug.Log("Scena " + sceneToLoadName + " impostata come scena attiva.");

        asyncLoadOperation = null;

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
