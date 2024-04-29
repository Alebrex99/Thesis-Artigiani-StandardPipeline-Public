using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum eScenes {
    MAIN,
    MENU,
    EV_BASE,
    EV_ALCUDIA,
    TEST_MULTIPLAYER
}

public enum Scenes
{
    INTRO,
    HOME,
    JEWEL1
}

public class cAppManager : MonoBehaviour {
    public GameObject goPersistent;
    [Range(0, 10)]
    public float fadeSpeed = 1;

    public static bool loadLocalModels = false;
    private static cAppManager instance;
    private static bool isZurdo = false;
    private static string userID = "";
    private static string userFolder = "-88";
    private static int actualBuildScene = -1;
    private static int prevBuildScene = -1;
    //private static ColorAdjustments colorAdjustments;
    private static eScenes actualEscena;
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


    public static eScenes GetActualEscena() {
        return actualEscena;
    }

    public static void LoadScene(eScenes escena) {
        if (actualEscena == escena) {
            Debug.LogWarning("[APP] Se esta intentando cargar la misma escena: " + escena);
            return;
        }
        if (eScenes.MENU == escena) {
            //ALE cProductManager.UnloadBundles();
        }
        else {
            //ALE cMainUIManager.HideLogWin();
        }
        Debug.Log("[App] Load Scene");
        actualEscena = escena;
        instance.StartCoroutine(instance.LoadSceneCor((int)escena));
        //cDataManager.AddJuegosAction(eAcciones.LoadScene, -1, 0, (int)escena, escena.ToString());
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

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(buildIndex);
        while (!loadOperation.isDone) {
            yield return null;
        }
        actualBuildScene = SceneManager.GetActiveScene().buildIndex;

        //HIDE LOADING
        //ALE cMainUIManager.HideLoading();
        /* ALE
        if (eScenes.EV_BASE == actualEscena) {
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
        if (eScenes.MENU == GetActualEscena()) {
            Application.Quit();
        }
        else {
            cMultiplayerManager.Desconecta();
            LoadScene(eScenes.MENU);
        }
    }*/
}
