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


    public static Scenes GetActualEscena() {
        return actualScene;
    }

    public static void LoadScene(Scenes scene) {
        if (actualScene == scene) {
            Debug.LogWarning("[APP] Se esta intentando cargar la misma scene: " + scene);
            return;
        }
        Debug.Log("[App] Load Scene");
        actualScene = scene;
        instance.StartCoroutine(instance.LoadSceneCor((int)scene));
        //cDataManager.AddJuegosAction(eAcciones.LoadScene, -1, 0, (int)scene, scene.ToString());
    }
    private IEnumerator LoadSceneCor(int buildIndex) {
        //SHOW LOADING
        //ALE cMainUIManager.ShowLoading();
        float orig = 1;
        while (orig > 0) {
            //colorAdjustments.colorFilter.value = new Color(orig, orig, orig);
            orig -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        //colorAdjustments.colorFilter.value = new Color(0, 0, 0);

        AsyncOperation asyncLoadOperation = SceneManager.LoadSceneAsync(buildIndex);
        while (!asyncLoadOperation.isDone) {
            Debug.Log("Caricamento della scena " + buildIndex + " in corso...");
            yield return null;
        }
        actualBuildScene = SceneManager.GetActiveScene().buildIndex;

        Scene loadedScene = SceneManager.GetSceneByBuildIndex(buildIndex);
        SceneManager.SetActiveScene(loadedScene);
        Debug.Log("Scena " + buildIndex + " impostata come scena attiva.");

        asyncLoadOperation = null;

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

        orig = 0;
        while (orig < 1) {
            //colorAdjustments.colorFilter.value = new Color(orig, orig, orig);
            orig += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        //colorAdjustments.colorFilter.value = new Color(1, 1, 1);
    }
    public static int GetActualBuildScene() {
        return actualBuildScene;
    }
    public static int GetPrevBuildScene() {
        if (-1 == prevBuildScene)
            return actualBuildScene;
        return prevBuildScene;
    }


    //ALE: FUNZIONI GENERICHE CAMBIO SCENA
    public static void GoToSceneAsync(Scenes scene)
    {
        if (actualScene == scene)
        {
            Debug.LogWarning("[APP] Se esta intentando cargar la misma scene: " + scene);
            return;
        }
        Debug.Log("[App] Load Scene");
        actualScene = scene;
        instance.StartCoroutine(instance.GoToSceneAsyncRoutine((int)scene));
    }

    IEnumerator GoToSceneAsyncRoutine(int sceneIndex)
    {
        Scene sceneToLoad = SceneManager.GetSceneByBuildIndex(sceneIndex);
        if(sceneToLoad.IsValid() && sceneToLoad.isLoaded)
        {
            Debug.Log("La scena " + sceneToLoad.name + " è già caricata.");
            yield break;
        }
        AsyncOperation asyncLoadOperation = SceneManager.LoadSceneAsync(sceneIndex);
        while (!asyncLoadOperation.isDone)
        {
            Debug.Log("Caricamento della scena " + sceneToLoad.name + " in corso...");
            yield return null;
        }
        Debug.Log("Scena " + sceneToLoad.name + " caricata con successo.");
        OVRScreenFade.instance.FadeIn();

        Scene loadedScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
        SceneManager.SetActiveScene(loadedScene);
        Debug.Log("Scena " + sceneToLoad.name + " impostata come scena attiva.");

        asyncLoadOperation = null;
    }





    /*ALEpublic static void QuitApp() {
        //MOSTRAR CONFIRMACION
        //SI ES CLIENTE DESCONECTAR
        //SI ES SERVIDOR DESCONECTAR A CLIENTE
        //SI ESTA EN ESCENA->VOLVER A MENU
        //SI ESTA EN MENU->SALIR
        cMainUIManager.ShowAlert(cLanguageManager.GetString("_AlertExit", null), CierraApp, true);
    }*/

    /*ALE private static void CierraApp() {
        cMainUIManager.ResetLog();
        if (Scenes.MENU == GetActualEscena()) {
            Application.Quit();
        }
        else {
            cMultiplayerManager.Desconecta();
            LoadScene(Scenes.MENU);
        }
    }*/
}
