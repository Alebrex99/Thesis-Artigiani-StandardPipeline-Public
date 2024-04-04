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
    private Button3D[] _button3Ds;


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
        RenderSettings.skybox = skyboxBase;
        _currentState = State.Main;
        _waitingRoom.SetActive(false);
        _office.SetActive(false);
        _environment.SetActive(true);

        //FindObjectsOfType --> da usare per prendere tutti i bottoni
        _button3Ds = FindObjectsOfType<Button3D>();
        foreach(Button3D button3D in _button3Ds)
        {
            Debug.Log(button3D.getButtonName());
            button3D.OnButtonPressed += OnButtonPressedEffect;
        }
 
        
    }

    //FSM POSSIBILE: 
    //DA FARE DURANTE OGNI STATO : ANCORA DA DECIDERE
    //andrebbe messo nell'update così che nello stato corrente accadano cose
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

    private void ChangeState(State buttonState)
    {
        //INSERIRE TRANSIZIONI
        //es. sono in button1 (newState) ed è stato premuto button2 (buttonState)
        State newState = _currentState; //stato in cui sono
        switch (_currentState)
        {
            case State.Main:
                newState = buttonState;
                break;
            case State.Button1:
                newState = (buttonState == State.Button1) ? State.Main : buttonState;
                break;
            case State.Button2:
                newState = (buttonState == State.Button2) ? State.Main : buttonState;
                break;
            case State.Button3:
                newState = (buttonState == State.Button3) ? State.Main : buttonState;
                break;
            case State.Button4:
                newState = (buttonState == State.Button4) ? State.Main : buttonState;
                break;
            default: throw new ArgumentOutOfRangeException();
        }

        if (newState != _currentState)
        {
            Debug.Log($"Changing State FROM:{_currentState} --> TO:{newState}");
            _currentState = newState;
        }
    }

   

    public void OnButtonPressedEffect(Button3D button, bool isButtonPressed )
    {
        Debug.Log($"Button {button.getButtonName()} premuto --> cambio stato");
        State ButtonToState;
        switch (button.getButtonName())
        {
            case "Button1":
                ButtonToState = State.Button1;
                break;
            case "Button2":
                ButtonToState = State.Button2;
                break;
            case "Button3":
                ButtonToState = State.Button3;
                break;
            case "Button4":
                ButtonToState = State.Button4;
                break;
            default: throw new ArgumentOutOfRangeException();
        }
        
        ChangeState(ButtonToState);
    }


    //FUNZIONI PER I PULSANTI -> CAMBIO STATO (TRANSIZIONI)
    //se clicco -> cambia stato
    public void OnButton1Pressed()
    {
        
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
