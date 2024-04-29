using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class cLoading : MonoBehaviour {
    public TextMeshProUGUI txMensaje;
    public TextMeshProUGUI txPuntos;
    public RectTransform icoLoading;
    public GameObject goPanel;


    void Awake() {
        goPanel.SetActive(false);
    }

    public void ShowLoading(string texto) {
        if (string.IsNullOrEmpty(texto)) { }
            //ALE txMensaje.text =  cLanguageManager.GetString("_Loading", null);
        else
            txMensaje.text = texto;
        goPanel.SetActive(true);
        StopCoroutine("AnimaTexto");
        StartCoroutine("AnimaTexto");
    }
    public void HideLoading() {
        StopCoroutine("AnimaTexto");
        goPanel.SetActive(false);
    }

    public bool IsShowing() {
        return goPanel.activeSelf;
    }

    private IEnumerator AnimaTexto() {
        float timeTexto = Time.time;
        float timeImag = Time.time;
        txPuntos.text = ".";
        icoLoading.rotation = Quaternion.identity;
        icoLoading.transform.localEulerAngles = Vector3.zero;
        yield return null;
        while (true) {
            if (Time.time > timeTexto + 1) {
                txPuntos.text += ".";
                if (txPuntos.text.Length > 8) {
                    txPuntos.text = ".";
                }
                timeTexto = Time.time;
            }
            //icoLoading.Rotate(Vector3.back, Time.deltaTime * 100);
            icoLoading.transform.localEulerAngles = new Vector3(0, 0, icoLoading.transform.eulerAngles.z - Time.deltaTime * 100);
            yield return null;
        }
    }

}