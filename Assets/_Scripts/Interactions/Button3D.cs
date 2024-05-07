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
    [SerializeField] GameObject _environmentMain;
    public static GameObject _currentEnvironment; //accessibile da qualunque altro script senza un rifeirmento necessario

    //mettere la logica direttamente nel bottone

    void Start ()
    {
        _currentEnvironment = _environmentMain;
    }

    //Funzione specifica Buttone1
    public void Press()
    {
        if (isButtonPressed)
            return;

        isButtonPressed = true;

        //ACTION SE VUOI CAMBIO STATO:
        /*if (OnButtonPressed != null)
            OnButtonPressed(this, isButtonPressed);
        */
        //logica nel bottone:
        Debug.Log(_currentEnvironment.name);
        Debug.Log(_environmentOn.name);
        ChangeEnvironment(); //PER DEMO

        isButtonPressed = false;

    }


    public void ChangeEnvironment()
    {
        //IntroManager.instance.voiceAudio.Stop();
        //se l'ambiente corrente è quello da accendere, lo spegni e accendi quello principale
        if (_currentEnvironment.name == _environmentOn.name)
        {
            
            _currentEnvironment.SetActive(false);
            _currentEnvironment = _environmentMain;
            _currentEnvironment.SetActive(true);
        }
        //se l'ambiente corrente non è quello da accendere, spegni quello corrente e accendi quello da accendere
        else
        {
            _currentEnvironment.SetActive(false);
            _currentEnvironment = _environmentOn;
            _currentEnvironment.SetActive(true);
  

        }

        /*
        if (RenderSettings.skybox == _skyboxOn)
        {
            RenderSettings.skybox = _skyboxOff;
        }
        else
        {
            RenderSettings.skybox = _skyboxOn;
        }*/

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
