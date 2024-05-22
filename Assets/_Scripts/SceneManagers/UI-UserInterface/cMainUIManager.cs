using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class cMainUIManager : MonoBehaviour{
    public GameObject goMainCanvas;
    public cLoading scrLoading;
    [Range(0, 2)]
    public float loadingDistance = 0.5f;
    //ALE public cAlertWindow scrAlert;
    //ALE public cUITutorial scrTutorial;
    //ALE public TextMeshProUGUI txLog;
    //ALE public GameObject pnLogs;

    public static cMainUIManager instance;
    private static UnityEvent<string> logUpdated;

    private void Awake() {
        instance = this;
        goMainCanvas.SetActive(false);
        logUpdated = new UnityEvent<string>();
        //ALE pnLogs.SetActive(false);
    }

    public static void ShowLoading(string text = null) {
        instance.goMainCanvas.SetActive(true);
        //instance.goMainCanvas.transform.parent = cXRManager.GetTrCenterEye();//ALE
        //instance.goMainCanvas.transform.position = cXRManager.GetTrCenterEye().position + cXRManager.GetTrCenterEye().forward * instance.loadingDistance; //0.5f
        //instance.goMainCanvas.transform.rotation = Quaternion.LookRotation(instance.goMainCanvas.transform.position - cXRManager.GetTrCenterEye().position);
        //instance.goMainCanvas.transform.eulerAngles = new Vector3(0, instance.goMainCanvas.transform.eulerAngles.y, 0);

        //in posizione locale della camera : dove effettivamente guardo
        instance.goMainCanvas.transform.position = cXRManager.GetTrCenterEye().position + cXRManager.GetTrCenterEye().forward* instance.loadingDistance; // ALE 0.5f
        instance.goMainCanvas.transform.rotation = cXRManager.GetTrCenterEye().localRotation; //ALE
        instance.scrLoading.ShowLoading(text);
    }
    public static void HideLoading() {
        instance.scrLoading.HideLoading();
        instance.goMainCanvas.SetActive(false); //ALE
        // Reset goMainCanvas to its original parent
        //cMainUIManager.instance.goMainCanvas.transform.parent = null; //ALE
        /*ALE if (!instance.scrAlert.IsShowing())
            instance.goMainCanvas.SetActive(false);*/
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
