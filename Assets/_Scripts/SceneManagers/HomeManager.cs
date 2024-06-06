using Evereal.VRVideoPlayer;
using Meta.WitAi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class HomeManager: MonoBehaviour
{
    public enum State
    {
        Main,
        Button1,
        Button2,
        Button3,
        Button4
    }
    public static HomeManager instance;//singleton

    //GESTIONE BOTTONI
    public AudioSource envAudioSrc; //voce spiegazione
    public AudioClip _buttonExplainClip;
    [SerializeField] private GameObject[] _lateActivatedObj; //interagibili principali (bottoni main ecc)
    [Range(0,60)]
    [SerializeField] private float _interactableActivationDelay = 1f;
    [SerializeField] private Transform mainInteractablesInitPos;
    [SerializeField] private Button3D[] _buttonsMain3D;

    //GESTIONE FSM / AMBIENTI HOME
    State _currentState;
    private GameObject _currentEnvironment;
    [SerializeField] GameObject _environmentMain;
    [SerializeField] GameObject _envMyMotivation;
    [SerializeField] GameObject _envOffice;
    [SerializeField] GameObject _envMyExperience;
    [SerializeField] GameObject chairInitpos;

    //DEPRECATED
    [SerializeField] GameObject _video2DScene;
    [SerializeField] GameObject _video180StereoScene;

    //FEATURES DA cStBase
    public Transform userInitPos;
    public Transform trLightButton;
    //public cWatchManager scrWatch;

    //MY HISTORY + MI TALLER
    [SerializeField] GameObject informations;
    private bool isMyHistoryOpened=false;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ResetUserPosition();
        _currentState = State.Main;
        //RenderSettings.skybox = skyboxMain;
        
        //Start environment HOME
        _environmentMain.SetActive(true);
        //spegni tutto il resto
        _envMyMotivation.SetActive(false);
        _envOffice.SetActive(false);
        _envMyExperience.SetActive(false);
        
        _video2DScene.SetActive(false);
        _video180StereoScene.SetActive(false);
        _currentEnvironment = _environmentMain;
        //MY HISTORY + MI TALLER
        informations.SetActive(false);
        
        //BOTTONI
        //_buttonsMain3D = FindObjectsOfType<Button3D>(); //pesa meno con Public lista , ma sbatti dopo
        foreach (Button3D button3D in _buttonsMain3D)
        {
            button3D.OnButtonPressed += OnButtonPressedEffect;
        }
        foreach(GameObject lateObj in _lateActivatedObj)
        {
            lateObj.SetActive(false);
        }
        StartCoroutine(LateActivation(_lateActivatedObj, _interactableActivationDelay));
    }
    private void Update()
    {
        trLightButton.position = cXRManager.GetTrCenterEye().position;
        trLightButton.rotation = cXRManager.GetTrCenterEye().rotation;

        //La sedia segue il tuo sguardo
        Vector3 lookDirection = cXRManager.GetTrCenterEye().forward;
        // Calcola la rotazione target in base alla direzione dello sguardo
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        // Solo la rotazione attorno all'asse Y è necessaria
        Vector3 euler = targetRotation.eulerAngles;
        chairInitpos.transform.eulerAngles = new Vector3(0, euler.y, 0);
    }
    private IEnumerator LateActivation(GameObject[] toActivate, float _activationDelay)
    {
        yield return new WaitForSeconds(_activationDelay);
        //ATTIVO INTERAGIBILI MAIN
        for(int i=0; i<toActivate.Length; i++)
        {
            toActivate[i].SetActive(true);
            //toActivate[i].transform.position = mainInteractablesInitPos.position; //togliere se si usa cPanelHMDFollower
        }
        //SETTO E ATTIVO CLIP SPIEGAZIONE BOTTONI
        if(envAudioSrc!= null)
        {
            envAudioSrc.PlayOneShot(_buttonExplainClip); //start when the buttons appear
        }
    }
    public Transform GetUserInitTr()
    {
        return userInitPos;
    }
    public void ResetUserPosition()
    {
        cXRManager.SetUserPosition(GetUserInitTr().position, GetUserInitTr().rotation);
    }
    public static Scenes GetNextScene(Button3D buttonPressed)
    {
        switch (buttonPressed.getButtonName())
        {
            case "Button1":
                return Scenes.JEWEL1;
            case "Button2":
                return Scenes.JEWEL2;
            case "Button3":
                return Scenes.JEWEL3;
            case "Button4":
                return Scenes.JEWEL4;
            default:
                return Scenes.HOME;
        }
    }
    
    //CORRISPONDEREBBE ALLA CHECK TRANSITION NELLA FSM:
    public void OnButtonPressedEffect(Button3D buttonPressed, bool isButtonPressed)
    {
        //CAMBIO STATO: 
        Debug.Log($"Button {buttonPressed.getButtonName()} premuto --> cambio stato");
        String buttonPressedName = buttonPressed.getButtonName();

        //CHECK TRANSITION: FSM
        //CheckTransition(buttonPressedName);

        //UPDATE STATE: FSM
        //UpdateState(buttonPressed); 

        //CAMBIO SCENA:
        Scenes scene = GetNextScene(buttonPressed);
        cAppManager.SelectedScene = (int)scene;

        cAppManager.LoadScene(scene);

    }
    public void OpenCloseInformations()
    {
        if (isMyHistoryOpened)
        {
            informations.SetActive(false);
            isMyHistoryOpened = false;
        }
        else
        {
            informations.SetActive(true);
            isMyHistoryOpened = true;
        }
    }



    private void OnDisable()
    {
        foreach (Button3D button3D in _buttonsMain3D)
        {
            button3D.OnButtonPressed -= OnButtonPressedEffect;
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

    private void CheckTransition(string buttonPressedName)
    {
        State newState = _currentState;
        
        //premo lo stesso bottone di quello corrente
        if (newState.ToString() == buttonPressedName)
        {
            newState = State.Main;

        }
        else //premo un bottone diverso da quello corrente (o la prima volta o le successive)
        {
            newState = (State)Enum.Parse(typeof(State), buttonPressedName);

        }
        if (newState != _currentState)
        {
            Debug.Log($"Changing State FROM:{_currentState} --> TO:{newState}");
            _currentState = newState;
        }

        /*switch (_currentState)
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
       }*/
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
