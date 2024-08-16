using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Jewel1Manager : MonoBehaviour
{
    public static Jewel1Manager instance;

    //FEATURES DA cStBase
    public Transform userInitPos;
    public Transform trLightJewel;

    //GESTIONE SCENA + IMMERSIONE
    public AudioSource envAudioSrc;
    public AudioSource interactAudioSrc;
    public AudioClip[] _envClips;
    private float clipPoint = 0;
    private float clipPointJewel = 0;  
    private bool hasClipFinished = false;
    [Range(0, 60)]
    [SerializeField] private float _envExplainDelay = 1f;
    [Range(0, 60)]
    [SerializeField] private float _immersionDelay = 1f;
    [Range(0, 60)]
    [SerializeField] private float _activationDelay = 1f;
    [SerializeField] private GameObject[] _lateActivatedObj;

    //VIDEO/QUADRO SOROLLA
    [SerializeField] private GameObject goVideoPlayer;
    [SerializeField] private GameObject sorollaPicture;
    [SerializeField] private GameObject jewel1Informations;
    private bool bShowVideo = false;
    [Range(0.1f, 10)]
    [SerializeField] private float rotationVideoSpeed = 1;

    //JEWEL 1
    [SerializeField] private Jewel _jewel1;
    [SerializeField] private Transform _jewelInitPos;
    private bool isJewelTouched = false;
    private Coroutine currentCoroutine;
    private bool isFading = false;


    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;

        _jewel1.OnJewelTouched += OnJewel1Touched;
        sorollaPicture.SetActive(false);
        //jewel1Informations.SetActive(false);
        foreach (GameObject lateObj in _lateActivatedObj)
        {
            lateObj.SetActive(false);
        }
    }

    void Start()
    {
        //StartCoroutine(PlayEnvMedia());
        //StartCoroutine(LateActivation(_lateActivatedObj, _activationDelay));
        ResetUserPosition();
        envAudioSrc.Play(); //attivato sempre quando ritorno in scena anche
        StartCoroutine(LateActivationJewel(_lateActivatedObj, _immersionDelay)); //dopo 15 secondi compare gioiello + audio1 
        StartCoroutine(LateActivationButtons(_lateActivatedObj, _activationDelay));
    }

    // Update is called once per frame
    void Update()
    {
        if (bShowVideo)
        {
            //FUNZIONA ANCHE QUESTO:
            //Vector3 euler = Quaternion.LookRotation(goVideoPlayer.transform.position - cXRManager.GetTrCenterEye().position).eulerAngles;
            //goVideoPlayer.transform.eulerAngles = new Vector3(0, euler.y, 0);

            //Used Method lectures:
            Vector3 targetDirection = goVideoPlayer.transform.position - cXRManager.GetTrCenterEye().position;
            targetDirection.y = 0;
            targetDirection.Normalize();
            float rotationStep = rotationVideoSpeed * Time.deltaTime;

            Vector3 newDirection = Vector3.RotateTowards(goVideoPlayer.transform.forward, targetDirection, rotationStep, 0.0f);
            goVideoPlayer.transform.rotation = Quaternion.LookRotation(newDirection, goVideoPlayer.transform.up);
        }
        else
        {
            //Vector3 euler = Quaternion.LookRotation(goVideoPlayer.transform.position - cXRManager.GetTrCenterEye().position).eulerAngles;
            //SOSTITUIRE : goVideoPlayer se voglio un altro oggetto quando il video si spegne; es) goLogoCentral
            //goVideoPlayer.transform.eulerAngles = new Vector3(0, euler.y, 0);
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

    private IEnumerator LateActivation(GameObject[] toActivate, float _activationDelay)
    {
        yield return new WaitForSeconds(_activationDelay);
        //toActivate.transform.position = _jewelInitPos.position + new Vector3(0, toActivate.transform.position.y, 0);
        //ATTIVA TUTTO
        for (int i = 0; i < toActivate.Length; i++)
        {
            toActivate[i].SetActive(true);
            //toActivate[i].transform.position = mainInteractablesInitPos.position; //togliere se si usa cPanelHMDFollower
        }
        //SETTA POSIZIONI
        _jewel1.transform.position = _jewelInitPos.position;
        bShowVideo = true;
    }

    private IEnumerator PlayEnvMedia()
    {

        //envAudioSrc.PlayOneShot(_envClips[0], 1f); //Environment sounds (già nel video)
        yield return new WaitForSeconds(_immersionDelay);
        envAudioSrc.PlayOneShot(_envClips[0], 1f); //Environment explanation
        //yield return new WaitForSeconds(_immersionDelay);
        //PlayPicture(); //se verrà messo un video (per ora solo quadro)

    }
    private IEnumerator LateActivationJewel(GameObject[] toActivate, float _immersionDelay)
    {
        yield return new WaitForSeconds(_immersionDelay); //se c'è sostituisci _envExplainDelay
        toActivate[0].SetActive(true);
        //SETTA POSIZIONI
        _jewel1.transform.position = _jewelInitPos.position;
        envAudioSrc.volume = 0.3f;
        StartCoroutine(FadeInAudio(interactAudioSrc, 2f, _envClips[0])); //Jewel explaination
        //envAudioSrc.PlayOneShot(_envClips[1], 1); //Jewel explaination
    }

    private IEnumerator LateActivationButtons(GameObject[] toActivate, float _activationDelay)
    {
        yield return new WaitForSeconds(_immersionDelay);
        yield return new WaitUntil(() => !interactAudioSrc.isPlaying); //attendi che l'audio prec sia finito
        yield return new WaitForSeconds(_activationDelay);
        _jewel1.interactJewelActivate = true;
        toActivate[1].SetActive(true);
        if (!cAppManager.isBackHome)
        {
            envAudioSrc.volume = 0.3f;
            StartCoroutine(FadeInAudio(interactAudioSrc, 2f, _envClips[1]));  //Buttons explanation
            /*if(!envAudioSrc.isPlaying)
                envAudioSrc.PlayOneShot(_envClips[2], 1); //Buttons explanation*/
        }
        else
        {
            envAudioSrc.volume = 1f;
            interactAudioSrc.Stop();
        }
    }

    private void OnJewel1Touched(Jewel jewel, bool isJewelTouched)
    {
        this.isJewelTouched = isJewelTouched;
        //riduci regolarmente l'audio dell'ambiente nel giro di 5 secondi
        jewel1Informations.SetActive(false);
        sorollaPicture.SetActive(isJewelTouched);
        bShowVideo = true;

        //AUDIO
        if(cSocketManager.instance!=null && !cSocketManager.agentActivate)
        {
            //return; //se non è attivo l'agente non fa nulla
        }
        //lo spegnimento dell'agente è già gestito (sistemare i bip e la logica)
        if (isJewelTouched) {
            envAudioSrc.volume = 0.3f;
            //StartCoroutine(FadeOutAudio(interactAudioSrc, 2f));
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(SwitchAudio(interactAudioSrc, GetJewelAudioSource(), 2f));
            clipPoint = interactAudioSrc.time; //segna il punto del clip che sta uscendo
            Debug.Log("Clip point: " + clipPoint);
            //StartCoroutine(CheckIfClipFinished(GetJewelAudioSource()));
        }
        else
        {
            if (!cAppManager.isBackHome)
            {
                //se la clip non è terminata
                if (clipPoint <= interactAudioSrc.clip.length && clipPoint != 0)
                {
                    envAudioSrc.volume = 0.3f;
                    Debug.Log("TIME: " + interactAudioSrc.time + " CLIP : " + interactAudioSrc.clip.length + " condition: " + (interactAudioSrc.time >= interactAudioSrc.clip.length));
                    //StartCoroutine(FadeInAudio(interactAudioSrc, 2f));
                    if (currentCoroutine != null) StopCoroutine(currentCoroutine);
                    currentCoroutine = StartCoroutine(SwitchAudio(GetJewelAudioSource(), interactAudioSrc, 2f));
                    //clipPointJewel = GetJewelAudioSource().time; //appena tolgo l'audio del gioiello salvo il punto del clip
                    //Debug.Log("Clip point Jewel: " + clipPointJewel);
                    //StartCoroutine(CheckIfClipFinished(interactAudioSrc));
                }
                else
                {
                    StartCoroutine(FadeOutAudio(GetJewelAudioSource(), 2f));
                    envAudioSrc.volume = 1f;
                }
            }
            else
            {
                StartCoroutine(FadeOutAudio(GetJewelAudioSource(), 2f));
                envAudioSrc.volume = 1f;
            }

        }
    }

    private IEnumerator SwitchAudio(AudioSource fadeOutSrc, AudioSource fadeInSrc, float fadeTime)
    {
        while (isFading)
        {
            yield return null;  // Attendere un frame e riprovare
        }
        yield return StartCoroutine(FadeOutAudio(fadeOutSrc, fadeTime));
        yield return StartCoroutine(FadeInAudio(fadeInSrc, fadeTime));
    }

    private IEnumerator CheckIfClipFinished(AudioSource audioSource)
    {
        hasClipFinished = false;

        // Attendi che la clip termini considerando anche la pausa
        while (true)
        {
            // Controlla se la clip ha finito di riprodurre
            if (audioSource.time >= audioSource.clip.length)
            {
                hasClipFinished = true;
                break;
            }
            yield return null;
        }

        Debug.Log("Clip playback completed, setting hasClipFinished to true.");
    }

    public IEnumerator FadeOutAudio(AudioSource audioSrc, float fadeTime, AudioClip clip = null)
    {
        isFading = true;
        if (clip !=null)
            audioSrc.clip = clip;
        //audioSrc.clip = _envClips[1]; //decidi la CLip da settare (da usare con 2 audio source)
        float startVolume = audioSrc.volume;

        while (audioSrc.volume > 0)
        {
            audioSrc.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }
        audioSrc.Pause();
        audioSrc.volume = startVolume;
        isFading = false;
    }
    public IEnumerator FadeInAudio(AudioSource audioSrc, float fadeTime, AudioClip clip=null)
    {
        isFading = true;
        if(clip !=null)
            audioSrc.clip = clip;
        //audioSrc.clip = _envClips[1]; //decidi la clip da settare (da usare con 2 audio source)
        float startVolume = 1;
        audioSrc.volume = 0f;
        if(!audioSrc.isPlaying) //&&!cAppManager.isBackHome && audioSrc.clip == _envClips[0]
        {
            audioSrc.Play();
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
        isFading = false;
    }

    public AudioSource GetJewelAudioSource()
    {
       return _jewel1.GetAudioSource();
    }
    public AudioSource GetAudioSource()
    {
        return interactAudioSrc;
    }
    public AudioSource GetEnvAudioSource()
    {
        return envAudioSrc;
    }

    public void PauseAudioScene()
    {
        if (interactAudioSrc.isPlaying)
        {
            //audioSrc.Pause();
            StartCoroutine(FadeOutAudio(interactAudioSrc, 2f));
        }
        else if(GetJewelAudioSource().isPlaying)
        {
            StartCoroutine(FadeOutAudio(GetJewelAudioSource(), 2f));
        }
        envAudioSrc.volume = 0.3f;
    }
    public void UnPauseAudioScene()
    {
        /*if (lastAudioSrc != null && !lastAudioSrc.isPlaying) {
            StartCoroutine(FadeInAudio(lastAudioSrc, 2f));
        }*/
        if (isJewelTouched)
        {
            Debug.Log("UnPause Audio Source Sorolla");
            envAudioSrc.volume = 0.3f;
            StartCoroutine(FadeInAudio(GetJewelAudioSource(), 2f));
        }
        else
        {
            if (!cAppManager.isBackHome)
            {
                if (interactAudioSrc.time <= interactAudioSrc.clip.length && interactAudioSrc.time!=0) //se la clip non è finita e non è ripartira
                {
                    envAudioSrc.volume = 0.3f;
                    Debug.Log("UnPause Audio Source Jewel");
                    StartCoroutine(FadeInAudio(interactAudioSrc, 2f));
                }
                else
                {
                    envAudioSrc.volume = 1f;
                }
            }
            else
            {
                envAudioSrc.volume = 1f;
            }

        }
    }

    public AudioClip[] GetEnvAudioClips()
    {
       return _envClips;
    }

    private void OnDestroy()
    {
        //videoPlayer.Stop();
        //envAudioSrc.Stop(); //non puoi farlo!
        _jewel1.OnJewelTouched -= OnJewel1Touched;
        StopAllCoroutines();
    }




    /*private void PlayPicture()
    {
        if (bShowVideo)
        {
            videoPlayer.Stop();
            bShowVideo = false;
        }
        else
        {
            videoPlayer.Play();
            bShowVideo = true;
        }
    }*/

}
