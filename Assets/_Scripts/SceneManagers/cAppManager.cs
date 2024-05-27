using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum Scenes
{
    INTRO,
    HOME,
    JEWEL1,
    JEWEL2,
    JEWEL3,
    JEWEL4
}

public class cAppManager : MonoBehaviour {
    public GameObject goPersistent;
    [Range(0, 10)]
    public float fadeSpeed = 1;

    public static bool loadLocalModels = false;
    public static cAppManager instance;
    private static bool isZurdo = false;
    private static string userID = "";
    private static string userFolder = "-88";
    private static int actualBuildScene = -1;
    private static int prevBuildScene = -1;
    //private static ColorAdjustments colorAdjustments;
    private static Scenes actualScene;
    private static int selectedScene = -1;
    private AsyncOperation asyncLoadOperation;


    void Awake() {
        if (null != instance) {
            Destroy(goPersistent);
            return;
        }
        DontDestroyOnLoad(goPersistent);
        //cDataManager.ReadConfigFile();
        instance = this;
        //ALE cLanguageManager.Init("ES");

        //postProcessing.profile.TryGet<ColorAdjustments>(out colorAdjustments);
        //LogOut();
    }

    public static string UserID {
        get { return userID; }
        set {
            userID = DateTime.Now.ToString("yyMMddHHmmss");// value;
            userFolder = userID.ToString();
            //Directory.CreateDirectory(Application.persistentDataPath + "/" + userFolder);
            Debug.Log("<b>[Load]</b>Set UserID: " + userID + ", userFolder: " + userFolder);
            //ALE cCestaManager.Restart();
            //cDataManager.Init();
        }
    }
    public static string UserDataFolder {
        get { return Application.persistentDataPath + "/" + userFolder; }
    }
    public static string DataFolder {
        get { return Application.persistentDataPath + "/"; }
    }
    public static string UserName {
        set; get;
    }
    public static int SelectedScene {
        get { return selectedScene; }
        set {
            selectedScene = value;
        }
    }
    public static bool IsZurdo {
        get { return isZurdo; }
        set {
            isZurdo = value;
        }
    }
    public static string FriendIP {
        get { return PlayerPrefs.GetString("FriendIP", "192.168.1.2"); }
        set {
            PlayerPrefs.SetString("FriendIP", value);
        }
    }
    public static Scenes GetActualScene() {
        return actualScene;
    }
    public static int GetActualBuildScene()
    {
        return actualBuildScene;
    }
    public static int GetPrevBuildScene()
    {
        if (-1 == prevBuildScene)
            return actualBuildScene;
        return prevBuildScene;
    }

    //ALE: Funzione cambio Scena
    public static void LoadScene(Scenes scene)
    {
        if (actualScene == scene)
        {
            Debug.LogWarning("[APP] Se esta intentando cargar la misma scene: " + scene);
            return;
        }
        Debug.Log("[App] Load Scene");
        actualScene = scene; //SET LA SCENA CORRENTE (es. Intro)
        actualBuildScene = (int)scene;
        instance.StartCoroutine(instance.ChangeSceneCor(actualBuildScene));
        //instance.StartCoroutine(instance.ChangeScene2(actualBuildScene));
        //instance.StartCoroutine(instance.GoToSceneAsyncRoutine((int)scene));
    }

    IEnumerator GoToSceneAsyncRoutine(int sceneIndex)
    {
        cMainUIManager.ShowLoading(); //cDontDestroy.instance.gameObject.SetActive(true);
        Scene sceneToLoad = SceneManager.GetSceneByBuildIndex(sceneIndex);
        if (sceneToLoad.IsValid() && sceneToLoad.isLoaded)
        {
            Debug.Log("La scena " + sceneToLoad.name + " è già caricata.");
            yield break;
        }
        cOVRScreenFade.instance.FadeOut();
        
        AsyncOperation asyncLoadOperation = SceneManager.LoadSceneAsync(sceneIndex);
        while (!asyncLoadOperation.isDone)
        {
            Debug.Log("Caricamento della scena " + sceneToLoad.name + " in corso...");
            yield return null;
        }
        Debug.Log("Scena " + sceneToLoad.name + " caricata con successo.");
        actualBuildScene = SceneManager.GetActiveScene().buildIndex;

        Scene loadedScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
        SceneManager.SetActiveScene(loadedScene);
        Debug.Log("Scena " + sceneToLoad.name + " impostata come scena attiva.");

        cMainUIManager.HideLoading();
        OVRScreenFade.instance.FadeIn();
        asyncLoadOperation = null;
    }
    //VERSIONE LOADING (ALE)
    IEnumerator ChangeSceneCor(int sceneIndex)
    {
        cMainUIManager.ShowLoading(); //cDontDestroy.instance.gameObject.SetActive(true);
        Scene sceneToLoad = SceneManager.GetSceneByBuildIndex(sceneIndex);
        Scene sceneToUnload = SceneManager.GetActiveScene(); //scena corrente

        //CARICAMENTO SCENA NUOVA: ADDITIVAMENTE
        if (sceneToLoad.IsValid() && sceneToLoad.isLoaded)
        {
            Debug.Log("La scena " + sceneToLoad.name + " è già caricata.");
            yield break;
        }
        cOVRScreenFade.instance.FadeOut();
        
        asyncLoadOperation = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        yield return new WaitForEndOfFrame();
        while (!asyncLoadOperation.isDone)
        {
            //Debug.Log("Caricamento della scena " + sceneToLoad.name + " in corso...");
            yield return new WaitForEndOfFrame();
        }
        //Debug.Log("Scena " + sceneToLoad.name + " caricata con successo.");
        asyncLoadOperation = null;
        
        //ATTIVA SCENA NUOVA 
        Scene loadedScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
        SceneManager.SetActiveScene(loadedScene);

        //SCARICAMENTO SCENA ORIGINALE
        if (!sceneToUnload.IsValid() || !sceneToUnload.isLoaded)
        {
            Debug.Log("La scena " + sceneToUnload.name + " è già scaricata.");
            yield break;
        }
        asyncLoadOperation = SceneManager.UnloadSceneAsync(sceneToUnload);
        while (!asyncLoadOperation.isDone)
        {
            //Debug.Log("Scaricamento della scena " + sceneToUnload.name + " in corso...");
            yield return new WaitForEndOfFrame();
        }

        actualBuildScene = SceneManager.GetActiveScene().buildIndex;
        asyncLoadOperation = null;
        cMainUIManager.HideLoading();
        cOVRScreenFade.instance.FadeIn();
    }
    /*IEnumerator ChangeScene2(int sceneIndex)
    {
        cMainUIManager.ShowLoading();
        Scene sceneToLoad = SceneManager.GetSceneByBuildIndex(sceneIndex);
        Scene sceneToUnload = SceneManager.GetActiveScene(); //scena corrente
        
        SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        yield return new WaitForEndOfFrame();

        while(!SceneManager.GetSceneByBuildIndex(sceneIndex).isLoaded)
        {
            yield return new WaitForEndOfFrame();
        }
        SceneManager.UnloadScene(sceneToUnload);

        actualBuildScene = SceneManager.GetActiveScene().buildIndex;
        cMainUIManager.HideLoading();
    }*/

    public static void BackHome()
    {
        //swtich in base alla scena attuale in cui sono 
        switch (actualScene)
        {
            case Scenes.INTRO:
                IntroManager.instance.videoPlayer.Stop();
                break;
            case Scenes.HOME:
                
                break;
            case Scenes.JEWEL1:
                
                break;
            case Scenes.JEWEL2:
                
                break;
            case Scenes.JEWEL3:
                
                break;
            case Scenes.JEWEL4:
                
                break;
            default:
                break;
        }
        LoadScene(Scenes.HOME);
    }

    //AI : CONVERSATIONAL AGENT
    public static void CallConversationalAgent()
    {
        //CHIAMATA ALL'AI : METTO SE DEVE ESSERE POSSIBILE OVUNQUE; altrimenti solo da bottoni
        //USO di socket.io: scrivi codice di inzializzazione

    }





    //OLD FUNCTIONS 
    /*public static void LoadScene(Scenes scene) {
       if (actualScene == scene) {
           Debug.LogWarning("[APP] Se esta intentando cargar la misma scene: " + scene);
           return;
       }
       Debug.Log("[App] Load Scene");
       actualScene = scene;
       instance.StartCoroutine(instance.LoadSceneCor((int)scene));
       //cDataManager.AddJuegosAction(eAcciones.LoadScene, -1, 0, (int)scene, scene.ToString());
   }*/

    private IEnumerator LoadSceneCor(int buildIndex) {
        //SHOW LOADING
        //ALE cMainUIManager.ShowLoading();

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(buildIndex);
        while (!loadOperation.isDone)
        {
            yield return null;
        }
        actualBuildScene = SceneManager.GetActiveScene().buildIndex;


        //HIDE LOADING
        //ALE cMainUIManager.HideLoading();
        /* ALE
        if (Scenes.EV_BASE == actualScene) {
            if (0 < cProductManager.GetErrorLoadList().Count) {
                string modelos = "";
                foreach(string mod in cProductManager.GetErrorLoadList()) {
                    modelos += mod + ", ";
                }
                cMainUIManager.ShowAlert("No se ha podido cargar los modelos: " + modelos);
            }
        }*/

    }

    public static void QuitApp() {
        //MOSTRAR CONFIRMACION
        //SI ES CLIENTE DESCONECTAR
        //SI ES SERVIDOR DESCONECTAR A CLIENTE
        //SI ESTA EN ESCENA->VOLVER A MENU
        //SI ESTA EN MENU->SALIR
       //ALE cMainUIManager.ShowAlert(cLanguageManager.GetString("_AlertExit", null), CierraApp, true);
    }

    /*ALE private static void CierraApp() {
        cMainUIManager.ResetLog();
        if (Scenes.MENU == GetActualScene()) {
            Application.Quit();
        }
        else {
            cMultiplayerManager.Desconecta();
            LoadScene(Scenes.MENU);
        }
    }*/
}
