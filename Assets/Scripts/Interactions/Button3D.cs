using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using JetBrains.Annotations;

public class Button3D : MonoBehaviour
{
    //Notifiche cambio stato a FSM: scegliere Azione/Variabile
    public Action<Button3D, bool> OnButtonPressed;
    private bool isButtonPressed = false;
    public string ButtonName;

    //Elementi da spegnere e da accendere
    [SerializeField] Material _skyboxOn;
    [SerializeField] Material _skyboxMain;
    //[SerializeField] GameObject[] _envsOn;
    [SerializeField] GameObject _environmentOn;
    
    //mettere la logica direttamente nel bottone
    private Button3D _currentButton;


    void Start ()
    {
        
    }

    //Funzione specifica Buttone1
    public void Press()
    {
        if (isButtonPressed)
            return;

        isButtonPressed = true;

        //ACTION CAMBIO STATO:
        if (OnButtonPressed != null)
            OnButtonPressed(this, isButtonPressed);
        isButtonPressed = false;
    }



    public void ChangeEnvironment()
    {
       /*
       if (_environmentOn != null && _environmentOff != null)
       {
           if (_environmentOn.activeSelf)
           {
               _environmentOn.SetActive(false);
               _environmentOff.SetActive(true);
           }
           else
           {
               _environmentOn.SetActive(true);
               _environmentOff.SetActive(false);
           }

       }
       if (RenderSettings.skybox == _skyboxOn)
       {
           RenderSettings.skybox = _skyboxOff;
       }
       else
       {
           RenderSettings.skybox = _skyboxOn;
       }
       */
    }

    public String getButtonName()
    {
        return ButtonName;
    }

    public GameObject GetAssociatedEnvironment()
    {
        return _environmentOn;
    }

}
