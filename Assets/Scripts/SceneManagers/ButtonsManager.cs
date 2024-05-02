using Meta.WitAi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonsManager: MonoBehaviour
{
    public enum State
    {
        Main,
        Button1,
        Button2,
        Button3,
        Button4
    }
    public static ButtonsManager instance;//singleton
    
    State _currentState;
    private Button3D[] _button3Ds;
    private GameObject _currentEnvironment;

    //Skyboxes
    [SerializeField] Material skyboxMain;

    //[SerializeField] GameObject _buttons;
    //[SerializeField] GameObject _exitButton;
    [SerializeField] GameObject _environmentMain;
    [SerializeField] GameObject _waitingRoom;
    [SerializeField] GameObject _office;
    [SerializeField] GameObject _video2DScene;
    [SerializeField] GameObject _video180StereoScene;
    [SerializeField] OVROverlay _OVROverlayHome;
    public FadeScreen fadeScreen;
    

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        _currentState = State.Main;
        //RenderSettings.skybox = skyboxMain;

        _environmentMain.SetActive(true);
        //spegni tutto il resto
        _waitingRoom.SetActive(false);
        _office.SetActive(false);
        _video2DScene.SetActive(false);
        _video180StereoScene.SetActive(false);
        _currentEnvironment = _environmentMain;
    
        

        //BOTTONI
        _button3Ds = FindObjectsOfType<Button3D>(); //pesa meno con Public lista , ma sbatti dopo
        foreach(Button3D button3D in _button3Ds)
        {
            Debug.Log(button3D.getButtonName());
            button3D.OnButtonPressed += OnButtonPressedEffect;
        }
    }


    //FSM POSSIBILE: 
    //DA FARE DURANTE OGNI STATO : ANCORA DA DECIDERE
    //andrebbe messo nell'update così che nello stato corrente accadano cose
    private void UpdateState(Button3D buttonPressed)
    {
        switch (_currentState)
        {
            case State.Main:
                //_currentEnvironment.SetActive(false);
                //_currentEnvironment = _environmentMain;
                //_currentEnvironment.SetActive(true);
                break;
            case State.Button1:
                //OnButtonChangeEnvironment(buttonPressed);
                break;
            case State.Button2:
                //OnButtonChangeEnvironment(buttonPressed);
                break;
            case State.Button3:
                //OnButtonChangeEnvironment(buttonPressed);
                break;
            case State.Button4:
                //OnButtonChangeEnvironment(buttonPressed);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void CheckTransition()
    {
        State newState = _currentState;

        switch (_currentState)
        {
            case State.Main:
                //if () { } condizione per il cambio stato
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
            default:
                throw new ArgumentOutOfRangeException();
        }
        if(newState != _currentState)
        {
            Debug.Log($"Changing State FROM:{_currentState} --> TO:{newState}");
            _currentState = newState;
        }
    }

    //CORRISPONDE ALLA CHECK TRANSITION:
    public void OnButtonPressedEffect(Button3D buttonPressed, bool isButtonPressed )
    {
        //PUOI PROVARE A METTERE TALE LOGICA DIRETTAMENTE NEL BOTTONE
        Debug.Log($"Button {buttonPressed.getButtonName()} premuto --> cambio stato");
        String buttonPressedName = buttonPressed.getButtonName();
        
        State newState = _currentState;
        //premo lo stesso bottone di quello corrente
        if (_currentState.ToString() == buttonPressedName)
        {
             newState = State.Main;
            /*
            _currentEnvironment.SetActive(false);
            _currentEnvironment = _environmentMain;
            _currentEnvironment.SetActive(true);
            */
        }
        else //premo un bottone diverso da quello corrente (o la prima volta o le successive)
        {
            newState = (State)Enum.Parse(typeof(State), buttonPressedName);
            /*
            _currentEnvironment.SetActive(false);
            _currentEnvironment = buttonPressed.GetAssociatedEnvironment();
            _currentEnvironment.SetActive(true);
            */

        }
        if (newState != _currentState)
        {
            Debug.Log($"Changing State FROM:{_currentState} --> TO:{newState}");
            _currentState = newState;
        }
        /*se servirà la macchina a stati completa: va messo in Update() e la funzione 
        OnbuttonChangeEnvironment() dovrà essere chiamata nell IF qui sopra*/
        UpdateState(buttonPressed); 
        

    }

    public void OnButtonChangeEnvironment(Button3D buttonPressed)
    {
        _currentEnvironment.SetActive(false);
        _currentEnvironment = buttonPressed.GetAssociatedEnvironment();
        _currentEnvironment.SetActive(true);
    }




    private void ChangeState(Button3D buttonPressed)
    {
        //INSERIRE TRANSIZIONI
        State newButtonState = (State)Enum.Parse(typeof(State), buttonPressed.getButtonName());
        State newState = _currentState; //stato in cui sono
        switch (_currentState)
        {
            case State.Main:
                newState = newButtonState;
                break;
            case State.Button1:
                newState = (newButtonState == State.Button1) ? State.Main : newButtonState;
                break;
            case State.Button2:
                newState = (newButtonState == State.Button2) ? State.Main : newButtonState;
                break;
            case State.Button3:
                newState = (newButtonState == State.Button3) ? State.Main : newButtonState;
                break;
            case State.Button4:
                newState = (newButtonState == State.Button4) ? State.Main : newButtonState;
                break;
            default: throw new ArgumentOutOfRangeException();
        }

        if (newState != _currentState)
        {
            Debug.Log($"Changing State FROM:{_currentState} --> TO:{newState}");
            _currentState = newState;
        }
    }
}
