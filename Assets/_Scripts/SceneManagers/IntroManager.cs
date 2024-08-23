using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using static Unity.VisualScripting.Member;

public class IntroManager : MonoBehaviour
{
    public static IntroManager instance;
    //INIT APPLICATION: config


    //VIDEO (cSceneInfo)
    public VideoPlayer videoPlayer;
    [SerializeField] private GameObject goVideoPlayer;
    int loopVideo = 0;
    [Range(0.1f, 10)]
    [SerializeField] private float rotationVideoSpeed = 1;
    private bool bShownVideo = false;

    //SCENE : cSceneInfo + MENU : video + animation logo
    public Transform userInitPos;
    public Transform chairInitPos;
    [Range(0.1f, 1)]
    [SerializeField] private float rotationChairSpeed = 0.6f;
    private float timeLastClick = 0;
    [SerializeField] Animator animLogo;
    //[SerializeField] private GameObject menuCanvas;
    //[SerializeField] private cMenuLoad srcMenuLoad; //solo se il pannello del menu ha comportamenti particolari

    //AUDIO
    public AudioSource voiceAudio;

    //BUTTON home
    [SerializeField] private Transform _buttonHomeInitPos;
    [SerializeField] private Button3D _buttonHome;
    [Range(0, 60)]
    [SerializeField] private float _activationButtonDelay = 1f;

    private void Awake()
    {
        instance = this;

        _buttonHome.gameObject.SetActive(false);
        goVideoPlayer.gameObject.SetActive(false);

    }

    void Start()
    {
        ResetUserPosition();
        //chairInitPos.transform.rotation = cXRManager.GetTrCenterEye().localRotation;
        //imposta la posizione in base all'utente
        //chairInitPos.position = cXRManager.GetTrCenterEye().position + cXRManager.GetTrCenterEye().up * 0.6f + cXRManager.GetTrCenterEye().forward * -0.125f; //ALE 0.5f
        StartCoroutine(InitMenuCanvas());
        //cMainUIManager.ShowMenuCanvas(); //Problema : qui la posizione dell'occhio è a terra, ecco perché il Menu compare a terra
        //SE FUNZIONA IL MENU : TUTTO LO START VIENE SPOSTATO DENTRO LA INIT APPLICATION

    }
    private void Update()
    {
        Vector3 targetDirectionChair = cXRManager.GetTrCenterEye().forward;
        targetDirectionChair.y = 0;
        targetDirectionChair.Normalize();
        float rotationStepChair = rotationChairSpeed * Time.deltaTime;
        Vector3 newDirectionChair = Vector3.RotateTowards(chairInitPos.forward, targetDirectionChair, rotationStepChair, 0.0f);
        chairInitPos.rotation = Quaternion.LookRotation(newDirectionChair, chairInitPos.up);
        
        if (bShownVideo)
        {
            //Vector3 euler = Quaternion.LookRotation(goVideoPlayer.transform.position - cXRManager.GetTrCenterEye().position).eulerAngles;
            //goVideoPlayer.transform.eulerAngles = new Vector3(0, euler.y, 0);
            
            //Method lectures:
            Vector3 targetDirection = goVideoPlayer.transform.position - cXRManager.GetTrCenterEye().position;
            targetDirection.y = 0;
            targetDirection.Normalize();
            float rotationStep = rotationVideoSpeed * Time.deltaTime;
            
            Vector3 newDirection = Vector3.RotateTowards(goVideoPlayer.transform.forward, targetDirection, rotationStep, 0.0f);
            goVideoPlayer.transform.rotation = Quaternion.LookRotation(newDirection, goVideoPlayer.transform.up);
        }
        else
        {
            Vector3 euler = Quaternion.LookRotation(goVideoPlayer.transform.position - cXRManager.GetTrCenterEye().position).eulerAngles;
            //SOSTITUIRE : goVideoPlayer se voglio un altro oggetto quando il video si spegne; es) goLogoCentral
            goVideoPlayer.transform.eulerAngles = new Vector3(0, euler.y, 0);
        }
    }
   
    public void InitApplication()
    {
        //INIT ALL CONFIGURATION THAT ARE IN BACKGROUND (READ CONFIG FILE): CHIAMATO DA cMenuLoad
        cMainUIManager.HideMenuCanvas();
        cMainUIManager.ShowLoading();
        StartCoroutine(InitApplicationCor());
    }
    private IEnumerator InitApplicationCor()
    {
        //LEGGO FILE CONFIG:
        Debug.Log("[Init Application]: -----FILE CONFIG.CSV START READ-----");
        yield return StartCoroutine(ReadConfig.ReadCSVFile()); //si attende fino alla fine del caricamento del file config
        yield return new WaitForSeconds(2); //Attesa per vedere il Loading (+carino)
        Debug.Log("[Init Application]: -----FILE CONFIG.CSV END READ-----");
        cMainUIManager.HideLoading();

        //READ FROM FILE CSV
        TrySetFileConfigData();

        //ACCENDO LA SCENA
        goVideoPlayer.gameObject.SetActive(true);
        videoPlayer.loopPointReached += EndVideo;
        //Start voice Audio
        if (voiceAudio != null && videoPlayer != null)
        {
            videoPlayer.Play();
            bShownVideo = true;
            voiceAudio.Play();
        }
        //Invoke(nameof(EndAudio), voiceAudio.clip.length);
        StartCoroutine(LateActivation(_buttonHome.gameObject, _activationButtonDelay));
    }

    private IEnumerator InitMenuCanvas()
    {
        yield return new WaitUntil(() => cXRManager.GetTrCenterEye().position != GetUserInitTr().position);
        cMainUIManager.ShowMenuCanvas();
        chairInitPos.gameObject.SetActive(true); //attivo sedia
        chairInitPos.position = new Vector3(chairInitPos.position.x, GetUserInitTr().position.y+0.6f, GetUserInitTr().position.z-0.2f);
        //chairInitPos.position = new Vector3(chairInitPos.position.x, cXRManager.GetTrCenterEye().position.y - 0.6f, cXRManager.GetTrCenterEye().position.z -0.2f);
    }

    /*private void OnButtonPressedEffect(Button3D buttonPressed, bool isButtonPressed)
    {
        //CAMBIO STATO: 
        Debug.Log($"Button {buttonPressed.getButtonName()} premuto --> torna a HOME");
        String buttonPressedName = buttonPressed.getButtonName();

    }*/

    private void EndVideo(VideoPlayer source)
    {
        loopVideo++;
        if(loopVideo>= 1)
        {
            source.loopPointReached -= EndVideo;
            source.Stop();

            //ANIMAZIONI POSSIBILI : LOGO -> VIDEO
            /*animLogo.ResetTrigger("ShowVideo");
            animLogo.SetTrigger("HideVideo");*/
            bShownVideo = false; //smetter� di seguire l'utente
            cAppManager.LoadScene(Scenes.HOME);
        }
    }
    public void ClickSkipVideo()
    {
        videoPlayer.loopPointReached -= EndVideo;
        videoPlayer.Stop();
        bShownVideo = false; 
        cAppManager.LoadScene(Scenes.HOME);
    }
    public Transform GetUserInitTr()
    {
        return userInitPos;
    }
    public void ResetUserPosition()
    {
        cXRManager.SetUserPosition(GetUserInitTr().position, GetUserInitTr().rotation);
    }
    public void SetVideo(VideoClip vc)
    {
        //setting video clip run time
        videoPlayer.clip = vc;
    }
    private IEnumerator LateActivation(GameObject toActivate, float _activationDelay)
    {
        yield return new WaitForSeconds(_activationDelay);
        toActivate.SetActive(true);
        toActivate.transform.position = _buttonHomeInitPos.position;
        toActivate.transform.rotation = _buttonHomeInitPos.rotation;
    }

    public void TrySetFileConfigData()
    {
        //string introData = ReadConfig.configData.Find((string line) => line.Contains("INTRO"));
        string currentSceneName = "INTRO";
        if (ReadConfig.configDataMap.ContainsKey(currentSceneName))
        {
            Dictionary<string, float> parameters = ReadConfig.configDataMap[currentSceneName];
            if (parameters.TryGetValue("ActivationButtonDelay", out float buttonDelay))
            {
                _activationButtonDelay = buttonDelay;
                Debug.Log($"activationButtonDelay set to {buttonDelay} from configDataMap.");
            }
            else
            {
                Debug.Log("No value found for activationButtonDelay, using default/editor value.");
            }
            if (parameters.TryGetValue("ChairSpeed", out float chairSpeed))
            {
                rotationChairSpeed = chairSpeed;
                Debug.Log($"rotationChairSpeed set to {chairSpeed} from configDataMap.");
            }
            else
            {
                Debug.Log("No value found for rotationChairSpeed, using default/editor value.");
            }
        }
    }

    private void OnDestroy()
    {
        videoPlayer.loopPointReached -= EndVideo;
        StopAllCoroutines();
    }


    //POSSIBILE GESTIONE DI UN CANVAS MENU SOLO INTRO (FATTO NEI PERSISTENTI)
    //GESTIONE MENU SCARICAMENTO CONFIG: cStMenu
    /*public void ShowMenuCanvas()
    {
        menuCanvas.SetActive(true); //show canvas of the Menu (managed from cMenuLoad)
        menuCanvas.transform.position = cXRManager.GetTrCenterEye().position + cXRManager.GetTrCenterEye().forward * 0.8f; // ALE 0.5f
        menuCanvas.transform.rotation = cXRManager.GetTrCenterEye().localRotation; //ALE
        Debug.Log("DOVREBBE ESSERE OK LA POSIZIONE DEL MENU");
        srcMenuLoad.ShowMenu();
    }
    public void HideMenuCanvas()
    {
        srcMenuLoad.HideMenu();
        menuCanvas.SetActive(false);
    }*/









    //SOLO SE SI VUOLE AGGIUNGERE ANIMAZIONE LOGO -> VIDEO
    public void ClickLogo()
    {
        Debug.Log("Tocado logo");
        animLogo.ResetTrigger("HideVideo");
        animLogo.SetTrigger("ShowVideo");
        videoPlayer.loopPointReached += EndVideo;
        videoPlayer.isLooping = true;
        videoPlayer.Play();
        bShownVideo = true;
    }

    public void ClickButtonVideo()
    {
        if (Time.realtimeSinceStartup - timeLastClick < 1)
        {
            return;
        }
        if (bShownVideo)
        {
            videoPlayer.loopPointReached -= EndVideo;
            videoPlayer.Stop();
            animLogo.ResetTrigger("ShowVideo");
            animLogo.SetTrigger("HideVideo");
            bShownVideo = false;
            timeLastClick = Time.realtimeSinceStartup;
        }
        else
        {
            Debug.Log("Tocado logo");
            animLogo.ResetTrigger("HideVideo");
            animLogo.SetTrigger("ShowVideo");
            videoPlayer.loopPointReached += EndVideo;
            videoPlayer.isLooping = true;
            videoPlayer.Play();
            bShownVideo = true;
            timeLastClick = Time.realtimeSinceStartup;
            //cDataManager.AddResponse(eDataSesionAction.VIDEO, "");
        }
    }


}
