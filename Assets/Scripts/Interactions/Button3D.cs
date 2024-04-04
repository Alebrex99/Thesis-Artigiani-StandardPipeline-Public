using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Button3D : MonoBehaviour
{
    //Notifiche cambio stato a FSM: scegliere Azione/Variabile
    public Action<Button3D, bool> OnButtonPressed;
    private bool isButtonPressed = false;

    //Elementi da spegnere e da accendere
    [SerializeField] Material _skyboxOn;
    [SerializeField] Material _skyboxOff;
    [SerializeField] GameObject[] _envsOn;
    [SerializeField] GameObject[] _EnvsOff;
    [SerializeField] GameObject _environmentOn;
    [SerializeField] GameObject _environmentOff;

    void Start ()
    {
        
    }

    //Funzione specifica Buttone1
    public void Press()
    {
        if (isButtonPressed)
            return;
       isButtonPressed = true;
        //ACTION:
        if (OnButtonPressed != null)
            OnButtonPressed(this, isButtonPressed);
        
        isButtonPressed = !isButtonPressed;


        //PUOI UNIRE GLI IF PER SKYBOX E CAMBIO SCENA

        ChangeEnvironment();
        ChangeSkybox();

    }

    public void ChangeSkybox()
    {
        if (RenderSettings.skybox == _skyboxOn)
        {
            RenderSettings.skybox = _skyboxOff;
        }
        else
        {
            RenderSettings.skybox = _skyboxOn;
        }
    }
    
    public void ChangeEnvironment()
    {
        if (_environmentOn != null && _environmentOff != null)
        {
            if (_environmentOn.activeSelf)
            {
                _environmentOn.SetActive(false);
                _environmentOff.SetActive(true);
            }
            else
            {
                _environmentOff.SetActive(false);
                _environmentOn.SetActive(true);
            }

        }
    }

}
