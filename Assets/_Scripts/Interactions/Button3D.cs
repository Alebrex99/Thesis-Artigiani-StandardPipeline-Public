using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using JetBrains.Annotations;
using UnityEngine.UI;

public class Button3D : MonoBehaviour
{
    //Notifiche cambio stato a FSM: scegliere Azione/Variabile
    public Action<Button3D, bool> OnButtonPressed;
    public Action OnAgentCall;
    //public Action OnEnvironmentChanged;
    private bool isButtonPressed = false;
    public string ButtonName;
    [SerializeField] Image infoimage;
    [SerializeField] Image closeimage;
    [SerializeField] GameObject _environmentOn;
    [SerializeField] GameObject _environmentMain;
    private static Button3D activeButton = null;
    //public static GameObject _currentEnvironment; //accessibile da qualunque altro script senza un rifeirmento necessario

    void Start ()
    {

    }

    public void Press()
    {
        if (isButtonPressed)
            return;

        isButtonPressed = true;

        //ACTION SE VUOI CAMBIO STATO/CHANGE SCENE: se bottoni fanno stessa cosa (gestione in HomeManager)
        if (OnButtonPressed != null)
            OnButtonPressed(this, isButtonPressed);

        Debug.Log(_environmentOn.name);
        isButtonPressed = false;

    }

    public string getButtonName()
    {
        return ButtonName;
    }

    public void BackToHome()
    {
        cAppManager.BackHome();
    }

    public void OpenCloseInformations()
    {
        HomeManager.instance.OpenCloseInformations();
        
    }

    //AI : CONVERSATIONAL AGENT
    public void CallConversationalAgent()
    {
        Debug.Log("Toggle Conversational Agent");
        //TOGGLE per zittire il conversational agent
        cSocketManager.instance.ToggleSocket();
        if (OnAgentCall != null)
            OnAgentCall();
    }



    //PER DEMO + PER MY HISTORY ECC.
    public void ChangeEnvironment()
    {
        /*if(OnEnvironmentChanged != null)
            OnEnvironmentChanged(); //scritto per utilità futura
        */
        HomeManager.instance.envAudioSrc[0].Pause();
        HomeManager.instance.envAudioSrc[1].Pause();

        // Se c'è già un altro bottone attivo, ripristina la sua icona
        if (activeButton != null && activeButton != this)
        {
            activeButton.ResetIcon();
        }

        //CAMBIO AMBIENTE:
        //se l'ambiente corrente è quello da accendere, lo spegni e accendi quello principale
        //DA ON -> HOME
        if (HomeManager._currentEnvironment == _environmentOn)
        {
            HomeManager._currentEnvironment.SetActive(false);
            HomeManager._currentEnvironment = _environmentMain;
            HomeManager._currentEnvironment.SetActive(true);
            HomeManager.instance.isEnvironmentChanged = false;
            infoimage.gameObject.SetActive(true);
            if(HandDetectionManager.instance!=null) HandDetectionManager.instance.Deactivate();
        }
        //se l'ambiente corrente non è quello da accendere, spegni quello corrente e accendi quello da accendere
        //DA HOME -> ON
        else
        {
            HomeManager._currentEnvironment.SetActive(false);
            HomeManager._currentEnvironment = _environmentOn;
            HomeManager._currentEnvironment.SetActive(true);
            HomeManager.instance.isEnvironmentChanged = true;
            infoimage.gameObject.SetActive(false);
            if(HandDetectionManager.instance!=null) HandDetectionManager.instance.Activate();
        }
        activeButton = this;
    }


    public void ResetIcon()
    {
        infoimage.gameObject.SetActive(true);
    }

    public GameObject GetAssociatedEnvironment()
    {
        return _environmentOn;
    }

    
}
