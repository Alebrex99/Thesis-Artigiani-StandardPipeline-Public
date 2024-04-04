using Meta.WitAi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using static GuardSimple;

public class GameManager: MonoBehaviour
{
    public enum State
    {
        Main,
        Button1,
        Button2,
        Button3,
        Button4
    }
    public static GameManager instance;
    State _currentState;
    private Button3D _button3D;


    //Skyboxes
    [SerializeField] Material skyboxImg360Mono;
    [SerializeField] Material skyboxImg180Stereo;
    [SerializeField] Material skyboxBase;
    [SerializeField] GameObject _environment;
    [SerializeField] GameObject _waitingRoom;
    [SerializeField] GameObject _office;
    public FadeScreen fadeScreen;
    

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        _currentState = State.Main;
        RenderSettings.skybox = skyboxBase;
        _waitingRoom.SetActive(false);
        _environment.SetActive(true);

        //FindObjectsOfType --> da usare per prendere tutti i bottoni
        _button3D = FindObjectOfType<Button3D>();
        _button3D.OnButtonPressed += OnButtonPressedEffect;
        
    }

    //FSM POSSIBILE: 
    //DA FARE DURANTE OGNI STATO : ANCORA DA DECIDERE
    private void UpdateState()
    {
        switch (_currentState)
        {
            case State.Main:
                break;
            case State.Button1:
                break;
            case State.Button2: 
                break;
            case State.Button3:
                break;
            case State.Button4:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ChangeState()
    {
        //INSERIRE TRANSIZIONI
        /*
        State newState = _currentState;
        switch(_currentState)
        {
            case State.Main:
                newState = State.Button1;
                break;
            case State.Button1:
                newState = State.Main;
                break;
            case State.Button2:
                newState = State.Main;
                break;
            case State.Button3:
                newState = State.Main;
                break;
            case State.Button4:
                newState = State.Main;
                break;
        }*/
    }
    //FUNZIONI PER I PULSANTI -> CAMBIO STATO (TRANSIZIONI)
    //se clicco -> cambia stato

    public void OnButtonPressedEffect(Button3D button, bool isButtonPressed )
    {
        Debug.Log($"Button1 {button} premuto --> cambio stato");
    }
    public void OnButton1Pressed()
    {
        ChangeState();
        
        //PUOI UNIRE GLI IF PER SKYBOX E CAMBIO SCENA

        if (_environment != null && _waitingRoom!=null)
        {
            if (_environment.activeSelf)
            {
                _environment.SetActive(false);
                _waitingRoom.SetActive(true);
            }
            else
            {
                _waitingRoom.SetActive(false);
                _environment.SetActive(true);
            }
           
        }
        if(RenderSettings.skybox == skyboxImg360Mono)
        {
            RenderSettings.skybox = skyboxBase;
        }
        else
        {   
            RenderSettings.skybox = skyboxImg360Mono;
        }
    }

    public void OnButton2Pressed()
    {
        if (_environment != null && _office != null)
        {
            if (_environment.activeSelf)
            {
                _environment.SetActive(false);
                _office.SetActive(true);
            }
            else
            {
                _office.SetActive(false);
                _environment.SetActive(true);
            }

        }
        if (RenderSettings.skybox == skyboxImg180Stereo)
        {
            RenderSettings.skybox = skyboxBase;
        }
        else
        {
            RenderSettings.skybox = skyboxImg180Stereo;
        }
    }

    public void OnButton3Pressed()
    {

    }

    public void OnButton4Pressed()
    {

    }

}
