using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using JetBrains.Annotations;
using UnityEditor.UI;

public class Button3D : MonoBehaviour
{
    //Notifiche cambio stato a FSM: scegliere Azione/Variabile
    public Action<Button3D, bool> OnButtonPressed;
    private bool isButtonPressed = false;
    public string ButtonName;

    //Elementi da spegnere e da accendere
    //[SerializeField] Material _skyboxOn;
    //[SerializeField] Material _skyboxMain;
    //[SerializeField] GameObject[] _envsOn;
    [SerializeField] GameObject _environmentOn;
    [SerializeField] GameObject _environmentMain;
    public static GameObject _currentEnvironment; //accessibile da qualunque altro script senza un rifeirmento necessario

    void Start ()
    {
        _currentEnvironment = _environmentMain;
    }

    public void Press()
    {
        if (isButtonPressed)
            return;

        isButtonPressed = true;

        //ACTION SE VUOI CAMBIO STATO/CHANGE SCENE: se bottoni fanno stessa cosa (gestione in HomeManager)
        if (OnButtonPressed != null)
            OnButtonPressed(this, isButtonPressed);
        

        //DEMO: logica nel bottone:
        Debug.Log(_currentEnvironment.name);
        Debug.Log(_environmentOn.name);
        //ChangeEnvironment(); //ATTIVARE SOLO PER DEMO, disattiare per gestione in HomeManager

        isButtonPressed = false;

    }

    public string getButtonName()
    {
        return ButtonName;
    }

    public void BackToHome()
    {
        //gameObject.SetActive(false); //sistemato con il trigger sul info canvas
        cAppManager.BackHome();
        /*Scenes scene = Scenes.HOME;
        cAppManager.LoadScene(scene);*/
    }

    public void CallConversationalAgent()
    {
        cAppManager.CallConversationalAgent();
        //FAI COSE SE E' CHIAMABILE SOLO DA BOTTONI
    }





    //PER DEMO:
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


    public GameObject GetAssociatedEnvironment()
    {
        return _environmentOn;
    }

    
}
