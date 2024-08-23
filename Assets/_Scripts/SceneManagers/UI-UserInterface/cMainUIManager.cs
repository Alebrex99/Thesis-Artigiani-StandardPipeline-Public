using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class cMainUIManager : MonoBehaviour{
    public GameObject goMainCanvas;
    public cLoading scrLoading;
    public cMenuLoad scrMenuLoad;

    [Range(0,10)]
    private float loadingTime= 1f;
    [Range(0, 2)]
    public float loadingDistance = 0.5f;
    //ALE public cAlertWindow scrAlert;
    //ALE public cUITutorial scrTutorial;
    //ALE public TextMeshProUGUI txLog;
    //ALE public GameObject pnLogs;

    public static cMainUIManager instance;
    public Action OnLoadingEnd;
    private static UnityEvent<string> logUpdated;

    private void Awake() {
        instance = this;
        goMainCanvas.SetActive(false); //all'inizio è tutto disattivato
        logUpdated = new UnityEvent<string>();
        //ALE pnLogs.SetActive(false);
    }

    public static void ShowLoading(string text = null) {
        instance.goMainCanvas.SetActive(true);
        //ORIGINAL VERSION:
        //instance.goMainCanvas.transform.position = cXRManager.GetTrCenterEye().position + cXRManager.GetTrCenterEye().forward * instance.loadingDistance; //0.5f
        //instance.goMainCanvas.transform.rotation = Quaternion.LookRotation(instance.goMainCanvas.transform.position - cXRManager.GetTrCenterEye().position);
        //instance.goMainCanvas.transform.eulerAngles = new Vector3(0, instance.goMainCanvas.transform.eulerAngles.y, 0);

        //ALE VERSION : IN FRONT OF THE USER
        //instance.goMainCanvas.transform.parent = cXRManager.GetTrCenterEye();//ALE, puoi anche non metterlo come Parent se vuoi
        instance.goMainCanvas.transform.position = cXRManager.GetTrCenterEye().position + cXRManager.GetTrCenterEye().forward* instance.loadingDistance; // ALE 0.5f
        instance.goMainCanvas.transform.rotation = cXRManager.GetTrCenterEye().localRotation; //ALE
        instance.goMainCanvas.transform.eulerAngles = new Vector3(0, instance.goMainCanvas.transform.eulerAngles.y, 0);
        instance.scrLoading.ShowLoading(text);
    }
    public static void HideLoading() {
        instance.scrLoading.HideLoading();
        instance.goMainCanvas.SetActive(false); //ALE
        //azione per gestire il caricamenrto meglio (riaccendere ciò che serve)
        instance.OnLoadingEnd?.Invoke();
        //cMainUIManager.instance.goMainCanvas.transform.parent = null; //ALE

        /*ALE if (!instance.scrAlert.IsShowing())
            instance.goMainCanvas.SetActive(false);*/
    }
    //GESTIONE MENU SCARICAMENTO CONFIG: cStMenu
    public static void ShowMenuCanvas()
    {
        instance.goMainCanvas.SetActive(true); //show canvas of the Menu (managed from cMenuLoad)
        //instance.goMainCanvas.transform.position = cXRManager.GetTrCenterEye().position + cXRManager.GetTrCenterEye().forward * instance.loadingDistance; // ALE 0.5f
        //instance.goMainCanvas.transform.rotation = Quaternion.LookRotation(instance.goMainCanvas.transform.position - cXRManager.GetTrCenterEye().position); //ALE
        //instance.goMainCanvas.transform.eulerAngles = new Vector3(0, instance.goMainCanvas.transform.eulerAngles.y, 0);
        //instance.scrMenuLoad.ShowMenu();

        Vector3 forwardDirection = new Vector3(cXRManager.GetTrCenterEye().forward.x, 0, cXRManager.GetTrCenterEye().forward.z).normalized;
        instance.goMainCanvas.transform.position = cXRManager.GetTrCenterEye().position + forwardDirection * instance.loadingDistance; // ALE 0.5f
        instance.goMainCanvas.transform.rotation = Quaternion.LookRotation(forwardDirection); //ALE
        instance.goMainCanvas.transform.eulerAngles = new Vector3(0, instance.goMainCanvas.transform.eulerAngles.y, 0);
        instance.scrMenuLoad.ShowMenu();
    }
    public static void HideMenuCanvas()
    {
        instance.scrMenuLoad.HideMenu();
        instance.goMainCanvas.SetActive(false); //ALE
    }






















    public static void ShowAlert(string mens, Action func = null, bool showCancel = false) {
        instance.goMainCanvas.SetActive(true);
        instance.goMainCanvas.transform.position = cXRManager.GetTrCenterEye().position + cXRManager.GetTrCenterEye().forward * 0.5f;
        instance.goMainCanvas.transform.rotation = Quaternion.LookRotation(instance.goMainCanvas.transform.position - cXRManager.GetTrCenterEye().position);
        instance.goMainCanvas.transform.eulerAngles = new Vector3(0, instance.goMainCanvas.transform.eulerAngles.y, 0);
        //ALE instance.scrAlert.ShowAlert(mens, func, showCancel);
        //ALE instance.pnLogs.SetActive(true);
    }
    public static void HideAlert() {
        Debug.Log("[UI] Hide Alert, showing loading: "+ instance.scrLoading.IsShowing());
        //ALE instance.pnLogs.SetActive(false);
        //ALE instance.scrAlert.HideAlert();
        if (!instance.scrLoading.IsShowing())
            instance.goMainCanvas.SetActive(false);
    }

    public static void ShowTutorial(eTutorial step) {
        instance.goMainCanvas.SetActive(true);
        instance.goMainCanvas.transform.position = cXRManager.GetTrCenterEye().position + cXRManager.GetTrCenterEye().forward * 0.5f;
        instance.goMainCanvas.transform.rotation = Quaternion.LookRotation(instance.goMainCanvas.transform.position - cXRManager.GetTrCenterEye().position);
        instance.goMainCanvas.transform.eulerAngles = new Vector3(0, instance.goMainCanvas.transform.eulerAngles.y, 0);
        //ALE instance.scrTutorial.Show(step);
    }
    public static void HideTutorial(eTutorial step) {
        //ALE instance.scrTutorial.Hide(step);
    }

    public static void AddListenerLogUpdated(UnityAction<string> call) { logUpdated.AddListener(call); }
    public static void RemoveListenerLogUpdated(UnityAction<string> call) { logUpdated.RemoveListener(call); }
    
    /*ALEpublic static void PrintLog(string key, string text) {
        instance.txLog.text += "\n<b>" + key;
        instance.txLog.text += "</b>: " + text;
        logUpdated?.Invoke(instance.txLog.text);
    }*/
    /*ALE public static void ResetLog() {
        instance.txLog.text = "";
        logUpdated?.Invoke(instance.txLog.text);
    }*/
    public static void ShowLogWin() {
        instance.goMainCanvas.SetActive(true);
        instance.goMainCanvas.transform.position = cXRManager.GetTrCenterEye().position + cXRManager.GetTrCenterEye().forward * 0.5f;
        instance.goMainCanvas.transform.rotation = Quaternion.LookRotation(instance.goMainCanvas.transform.position - cXRManager.GetTrCenterEye().position);
        instance.goMainCanvas.transform.eulerAngles = new Vector3(0, instance.goMainCanvas.transform.eulerAngles.y, 0);
        //ALE instance.pnLogs.SetActive(true);
    }
    public static void HideLogWin() {
        //ALE instance.pnLogs.SetActive(false);
        if (!instance.scrLoading.IsShowing())
            instance.goMainCanvas.SetActive(false);
    }
}
