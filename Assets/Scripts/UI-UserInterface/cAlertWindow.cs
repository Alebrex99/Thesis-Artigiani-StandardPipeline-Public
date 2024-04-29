/*
 *  Ejemplo de llamada: 
 *      cAlerta.ShowAlerta("Prueba", () => {
 *          Debug.Log("Hola");
 *      }, true);
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class cAlertWindow : MonoBehaviour {
    public GameObject goBtCancel;
    public TextMeshProUGUI txMensaje;
    public GameObject goPanel;

    private static Action onBtAceptar;

    void Awake() {
        goPanel.SetActive(false);
    }

    public void ShowAlert(string mens, Action func = null, bool showCancel = false) {
        txMensaje.text = mens;
        onBtAceptar = func;
        goBtCancel.SetActive(showCancel);
        goPanel.SetActive(true);
        Debug.Log("[ALERT]: " + mens);
    }
    public void HideAlert() {
        goPanel.SetActive(false);
    }
    public bool IsShowing() {
        return goPanel.activeSelf;
    }

    public void ClickAceptar() {
        onBtAceptar?.Invoke();
        onBtAceptar = null;
        cMainUIManager.HideAlert();
    }
    public void ClickCancelar() {
        onBtAceptar = null;
        cMainUIManager.HideAlert();
    }
}
