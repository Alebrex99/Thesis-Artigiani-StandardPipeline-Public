using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum eTutorial {
    PISTOL,
    PALM,
    COGER,
    VOID
}

public class cUITutorial : MonoBehaviour {
    public GameObject goPanel;
    public TextMeshProUGUI txMens;
    public Image imGesto;
    public Sprite[] gestos;

    private eTutorial showing = eTutorial.VOID;
    private bool bCogido = false;

    void Awake() {
        goPanel.SetActive(false);
    }
    public void Show(eTutorial step) {
        if (eTutorial.PISTOL == step) {
            //SE MUESTRA SOLO CUANDO SE DETECTA NUEVA SESION
            //ALEstring mensaje = cLanguageManager.GetString("_TutPistol", null);
            string hand = "_TutRight";
            if (cAppManager.IsZurdo) hand = "_TutLeft";
            //ALE mensaje = mensaje.Replace("%%H%%", cLanguageManager.GetString(hand, null));
            //ALE txMens.text = mensaje;
            bCogido = false;
        }
        else if (eTutorial.PALM == step) {
            //SE MUESTRA SOLO CUANDO SE COMPRA EL PRIMER ITEM
            //ALE string mensaje = cLanguageManager.GetString("_TutReloj", null);
            string hand = "_TutLeft";
            if (cAppManager.IsZurdo) hand = "_TutRight";
            //ALE mensaje = mensaje.Replace("%%H%%", cLanguageManager.GetString(hand, null));
            //ALE txMens.text = mensaje;
        }
        else if (eTutorial.COGER == step && !bCogido) {
            //ALE txMens.text = cLanguageManager.GetString("_TutCoger", null); 
            bCogido = true;
        }
        else return;
        showing = step;
        imGesto.sprite = gestos[(int)step];
        goPanel.SetActive(true);
    }
    public void Hide(eTutorial step) {
        if (step==showing)
            goPanel.SetActive(false);
    }
    public void ClickAceptar() {
        goPanel.SetActive(false);
    }
}
