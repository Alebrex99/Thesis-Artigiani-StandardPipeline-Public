using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class cMainUIManager : MonoBehaviour{
    public GameObject goMainCanvas;
    public cLoading scrLoading;
    public cAlertWindow scrAlert;
    public cUITutorial scrTutorial;
    public TextMeshProUGUI txLog;
    public GameObject pnLogs;

    private static cMainUIManager instance;
    private static UnityEvent<string> logUpdated;

    private void Awake() {
        instance = this;
        goMainCanvas.SetActive(false);
        logUpdated = new UnityEvent<string>();
        pnLogs.SetActive(false);
    }

    public static void ShowLoading(string texto = null) {
        instance.goMainCanvas.SetActive(true);
        instance.goMainCanvas.transform.position = cXRManager.GetTrCenterEye().position + cXRManager.GetTrCenterEye().forward * 0.5f;
        instance.goMainCanvas.transform.rotation = Quaternion.LookRotation(instance.goMainCanvas.transform.position - cXRManager.GetTrCenterEye().position);
        instance.goMainCanvas.transform.eulerAngles = new Vector3(0, instance.goMainCanvas.transform.eulerAngles.y, 0);
        instance.scrLoading.ShowLoading(texto);
    }
    public static void HideLoading() {
        instance.scrLoading.HideLoading();
        if (!instance.scrAlert.IsShowing())
            instance.goMainCanvas.SetActive(false);
    }
    public static void ShowAlert(string mens, Action func = null, bool showCancel = false) {
        instance.goMainCanvas.SetActive(true);
        instance.goMainCanvas.transform.position = cXRManager.GetTrCenterEye().position + cXRManager.GetTrCenterEye().forward * 0.5f;
        instance.goMainCanvas.transform.rotation = Quaternion.LookRotation(instance.goMainCanvas.transform.position - cXRManager.GetTrCenterEye().position);
        instance.goMainCanvas.transform.eulerAngles = new Vector3(0, instance.goMainCanvas.transform.eulerAngles.y, 0);
        instance.scrAlert.ShowAlert(mens, func, showCancel);
        instance.pnLogs.SetActive(true);
    }
    public static void HideAlert() {
        Debug.Log("[UI] Hide Alert, showing loading: "+ instance.scrLoading.IsShowing());
        instance.pnLogs.SetActive(false);
        instance.scrAlert.HideAlert();
        if (!instance.scrLoading.IsShowing())
            instance.goMainCanvas.SetActive(false);
    }

    public static void ShowTutorial(eTutorial step) {
        instance.goMainCanvas.SetActive(true);
        instance.goMainCanvas.transform.position = cXRManager.GetTrCenterEye().position + cXRManager.GetTrCenterEye().forward * 0.5f;
        instance.goMainCanvas.transform.rotation = Quaternion.LookRotation(instance.goMainCanvas.transform.position - cXRManager.GetTrCenterEye().position);
        instance.goMainCanvas.transform.eulerAngles = new Vector3(0, instance.goMainCanvas.transform.eulerAngles.y, 0);
        instance.scrTutorial.Show(step);
    }
    public static void HideTutorial(eTutorial step) {
        instance.scrTutorial.Hide(step);
    }

    public static void AddListenerLogUpdated(UnityAction<string> call) { logUpdated.AddListener(call); }
    public static void RemoveListenerLogUpdated(UnityAction<string> call) { logUpdated.RemoveListener(call); }
    public static void PrintLog(string key, string text) {
        instance.txLog.text += "\n<b>" + key;
        instance.txLog.text += "</b>: " + text;
        logUpdated?.Invoke(instance.txLog.text);
    }
    public static void ResetLog() {
        instance.txLog.text = "";
        logUpdated?.Invoke(instance.txLog.text);
    }
    public static void ShowLogWin() {
        instance.goMainCanvas.SetActive(true);
        instance.goMainCanvas.transform.position = cXRManager.GetTrCenterEye().position + cXRManager.GetTrCenterEye().forward * 0.5f;
        instance.goMainCanvas.transform.rotation = Quaternion.LookRotation(instance.goMainCanvas.transform.position - cXRManager.GetTrCenterEye().position);
        instance.goMainCanvas.transform.eulerAngles = new Vector3(0, instance.goMainCanvas.transform.eulerAngles.y, 0);
        instance.pnLogs.SetActive(true);
    }
    public static void HideLogWin() {
        instance.pnLogs.SetActive(false);
        if (!instance.scrLoading.IsShowing())
            instance.goMainCanvas.SetActive(false);
    }
}
