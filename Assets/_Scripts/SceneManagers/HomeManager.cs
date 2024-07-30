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
    public AudioSource[] envAudioSrc; //voce spiegazione
    private bool isRotated = false;
    private bool initialPlayDone = false;
    private bool isLateActive = false;
    public AudioClip[] _buttonExplainClips;
    [SerializeField] private GameObject[] _lateActivatedObj; //interagibili principali (bottoni main ecc)
    [Range(0,60)]
    [SerializeField] private float _activationDelay = 1f;
    [SerializeField] private Transform mainInteractablesInitPos;
    [SerializeField] private Button3D[] _buttonsMain3D;

    //GESTIONE FSM / AMBIENTI HOME
    State _currentState;
    public static GameObject _currentEnvironment;
    [SerializeField] GameObject _environmentMain;
    //[SerializeField] GameObject _envMyMotivation;
    [SerializeField] GameObject _envOffice;
    [SerializeField] GameObject _envMyExperience;
    public bool isEnvironmentChanged = false;
    [SerializeField] Transform chairInitPos;
    [Range(0.1f, 1)]
    [SerializeField] private float rotationChairSpeed = 0.6f;
    [Range(30, 200)]
    [SerializeField] public int angleSwitch = 80;

    //DEPRECATED
    //[SerializeField] GameObject _video2DScene;
    //[SerializeField] GameObject _video180StereoScene;

    //FEATURES DA cStBase
    public Transform userInitPos;
    public Transform trLightButton;
    //public cWatchManager scrWatch;

    //MY HISTORY + MI TALLER
    [SerializeField] GameObject informations;
    private bool isMyHistoryOpened=false;
    private bool isAgentCalled = false;


    private void Awake()
    {
        instance = this;

        foreach (Button3D button3D in _buttonsMain3D)
        {
            button3D.OnButtonPressed += OnButtonPressedEffect;
        }
        envAudioSrc[0].clip = _buttonExplainClips[0];
        envAudioSrc[1].clip = _buttonExplainClips[1];

        _currentState = State.Main;
        //Start environment HOME
        _environmentMain.SetActive(true);
        //spegni tutto il resto
        _envOffice.SetActive(false);
        _envMyExperience.SetActive(false);

        //_envMyMotivation.SetActive(false);
        //_video2DScene.SetActive(false);
        //_video180StereoScene.SetActive(false);
        _currentEnvironment = _environmentMain;
        //MY HISTORY + MI TALLER
        informations.SetActive(false);

        //BOTTONI
        //_buttonsMain3D = FindObjectsOfType<Button3D>(); //pesa meno con Public lista , ma sbatti dopo
        foreach (GameObject lateObj in _lateActivatedObj)
        {
            lateObj.SetActive(false);
        }

        //SEDIA
        chairInitPos.GetChild(0).gameObject.SetActive(true); //attivo sedia
    }

    private void Start()
    {
        ResetUserPosition();

        //ATTIVAZIONI RITARDATE
        StartCoroutine(LateActivation(_lateActivatedObj, _activationDelay));
    }
    private void Update()
    {
        trLightButton.position = cXRManager.GetTrCenterEye().position;
        trLightButton.rotation = cXRManager.GetTrCenterEye().rotation;

        //SEDIA
        Vector3 targetDirection = cXRManager.GetTrCenterEye().forward;
        targetDirection.y = 0;
        targetDirection.Normalize();
        float rotationStep = rotationChairSpeed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(chairInitPos.forward, targetDirection, rotationStep, 0.0f);
        chairInitPos.rotation = Quaternion.LookRotation(newDirection, chairInitPos.up);

        //Debug.Log(" ENV CHANGED: " + isEnvironmentChanged + " isBack home: " + cAppManager.isBackHome + "isRotated: " + isRotated);
        //QUANDO TI GIRI VERSO I BOTTINI SECONDARI -> AVVIA SECONDA CLIP
        if (!isEnvironmentChanged && !cAppManager.isBackHome && isLateActive && !isAgentCalled)
        {
            SwitchAudioRotation();
        }
        
        
    }
    private void SwitchAudioRotation()
    {
        var forwardCamx = new Vector3(cXRManager.GetTrCenterEye().forward.x, 0, cXRManager.GetTrCenterEye().forward.z);
        var forwardButtx = new Vector3(mainInteractablesInitPos.forward.x, 0, mainInteractablesInitPos.forward.z);
        var angleRotation = Vector3.Angle(forwardCamx, forwardButtx);
        if (angleRotation > angleSwitch && angleRotation < 270)
        {
            if (!isRotated) //!envAudioSrc[1].isPlaying
            {
                StartCoroutine(FadeOutAudio(envAudioSrc[0], 2f));
                StartCoroutine(FadeInAudio(envAudioSrc[1], 2f));
            }
            isRotated = true;

        }
        else
        {
            if (isRotated)//!envAudioSrc[0].isPlaying
            {
                StartCoroutine(FadeOutAudio(envAudioSrc[1], 2f));
                StartCoroutine(FadeInAudio(envAudioSrc[0], 2f));
            }
            isRotated = false;
        }
    }

    public IEnumerator FadeOutAudio(AudioSource audioSrc, float fadeTime)
    {
        //audioSrc.clip = _envClips[1]; //decidi la CLip da settare (da usare con 2 audio source)
        float startVolume = audioSrc.volume;

        while (audioSrc.volume > 0)
        {
            audioSrc.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSrc.Pause();
        audioSrc.volume = startVolume;
    }
    public IEnumerator FadeInAudio(AudioSource audioSrc, float fadeTime)
    {
        //audioSrc.clip = _envClips[1]; //decidi la clip da settare (da usare con 2 audio source)
        float startVolume = 1f;
        audioSrc.volume = 0f;
        if(!initialPlayDone)
        {
            audioSrc.Play();
            initialPlayDone = true;
        }
        else audioSrc.UnPause();

        float currentTime = 0f;
        while (currentTime < fadeTime)
        {
            currentTime += Time.deltaTime;
            audioSrc.volume = Mathf.Lerp(0f, startVolume, currentTime / fadeTime);
            yield return null;
        }

        audioSrc.volume = startVolume;
    }
    private IEnumerator LateActivation(GameObject[] toActivate, float _activationDelay)
    {
        yield return new WaitForSeconds(_activationDelay);

        for(int i=0; i<toActivate.Length; i++)
        {
            toActivate[i].SetActive(true);
            //toActivate[i].transform.position = mainInteractablesInitPos.position; //togliere se si usa cPanelHMDFollower
        }
        //SETTO E ATTIVO CLIP SPIEGAZIONE BOTTONI
        if (!cAppManager.isBackHome)
        {
            if (!isRotated)
            {
                envAudioSrc[0].PlayOneShot(_buttonExplainClips[0]); //start when the buttons appear
            }
            if (isRotated)
            {
                envAudioSrc[1].PlayOneShot(_buttonExplainClips[1]);
            }
            isLateActive = true;
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
        foreach(AudioSource audiosrc in envAudioSrc)
        {
            audiosrc.Stop();
        }
        isEnvironmentChanged = true;
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

    public AudioSource[] GetAudioSources()
    {
        return envAudioSrc;
    }
    public void PauseAudioScene()
    {
        //metti in pausa 
        foreach(AudioSource audioSrc in envAudioSrc)
        {
            if (audioSrc.isPlaying)
            {
                //audioSrc.Pause();
                StartCoroutine(FadeOutAudio(audioSrc, 2f));
            }
        }
        isAgentCalled = true;
    }
    public void UnPauseAudioScene()
    {
        isAgentCalled = false;
    }


    //USATO ON DESTROY PER DISINSCRIVERE
    private void OnDestroy()
    {
        foreach (Button3D button3D in _buttonsMain3D)
        {
            button3D.OnButtonPressed -= OnButtonPressedEffect;
        }
        StopAllCoroutines();

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
