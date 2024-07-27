using Evereal.VRVideoPlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Jewel2Manager : MonoBehaviour
{
    public static Jewel2Manager instance;

    //FEATURES DA cStBase
    public Transform userInitPos;
    public Transform trLightJewel;

    //GESTIONE SCENA + IMMERSIONE
    public AudioSource envAudioSrc;
    public AudioSource interactAudioSrc;
    public AudioClip[] _envClips;
    private float clipPoint = 0;
    [Range(0, 60)]
    [SerializeField] private float _envExplainDelay = 1f;
    [Range(0, 60)]
    [SerializeField] private float _immersionDelay = 1f;
    [Range(0, 60)]
    [SerializeField] private float _activationDelay = 1f;
    [SerializeField] private GameObject[] _lateActivatedObj;

    //IMMAGINE/DESCRIZIONE
    [SerializeField] private GameObject goVideoPlayer;
    [SerializeField] private GameObject fireworksPicture;
    [SerializeField] private GameObject jewel2Informations;
    private bool bShowVideo = false;
    [Range(0.1f, 10)]
    [SerializeField] private float rotationVideoSpeed = 1;

    //JEWEL 2
    [SerializeField] private Jewel _jewel2;
    [SerializeField] private Transform _jewelInitPos;


    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
        _jewel2.OnJewelTouched += OnJewel2Touched;
        ResetUserPosition();
        fireworksPicture.SetActive(false);
        jewel2Informations.SetActive(false);
        foreach (GameObject lateObj in _lateActivatedObj)
        {
            lateObj.SetActive(false);
        }
    }

    void Start()
    {
        //StartCoroutine(PlayEnvMedia());
        //StartCoroutine(LateActivation(_lateActivatedObj, _activationDelay));
        StartCoroutine(LateActivationJewel(_lateActivatedObj, _immersionDelay)); //dopo 15 secondi compare gioiello + audio1 
        StartCoroutine(LateActivationButtons(_lateActivatedObj, _activationDelay));
    }

    // Update is called once per frame
    void Update()
    {
        //FUNZIONA ANCHE QUESTO:
        //Vector3 euler = Quaternion.LookRotation(goVideoPlayer.transform.position - cXRManager.GetTrCenterEye().position).eulerAngles;
        //goVideoPlayer.transform.eulerAngles = new Vector3(0, euler.y, 0);

        if (bShowVideo)
        {
            //Used Method lectures:
            Vector3 targetDirection = goVideoPlayer.transform.position - cXRManager.GetTrCenterEye().position;
            targetDirection.y = 0;
            targetDirection.Normalize();
            float rotationStep = rotationVideoSpeed * Time.deltaTime;

            Vector3 newDirection = Vector3.RotateTowards(goVideoPlayer.transform.forward, targetDirection, rotationStep, 0.0f);
            goVideoPlayer.transform.rotation = Quaternion.LookRotation(newDirection, goVideoPlayer.transform.up);
        }
        else 
        { //Quando ritocchi il gioiello per spegnerlo -> non viene continuamente calcolato
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
        _jewel2.transform.position = _jewelInitPos.position;
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
        _jewel2.transform.position = _jewelInitPos.position;
        StartCoroutine(FadeInAudio(interactAudioSrc, 3f, _envClips[1])); //Jewel explaination
        //envAudioSrc.PlayOneShot(_envClips[1], 1); //Jewel explaination
    }

    private IEnumerator LateActivationButtons(GameObject[] toActivate, float _activationDelay)
    {
        yield return new WaitForSeconds(_activationDelay);
        yield return new WaitUntil(() => !interactAudioSrc.isPlaying);

        toActivate[1].SetActive(true);
        StartCoroutine(FadeInAudio(interactAudioSrc, 2f, _envClips[2]));  //Buttons explanation
        /*if (!interactAudioSrc.isPlaying)
            interactAudioSrc.PlayOneShot(_envClips[2], 1); //Buttons explanation*/
    }


    private void OnJewel2Touched(Jewel jewel, bool isJewelTouched)
    {
        //riduci regolarmente l'audio dell'ambiente nel giro di 5 secondi
        fireworksPicture.SetActive(isJewelTouched);
        jewel2Informations.SetActive(!isJewelTouched);
        bShowVideo = true;
        if (isJewelTouched)
        {
            StartCoroutine(FadeOutAudio(interactAudioSrc, 2f));
            clipPoint = interactAudioSrc.time;
            Debug.Log("Clip point: " + clipPoint);
        }
        else if (clipPoint <= interactAudioSrc.clip.length && clipPoint != 0)
        {
            Debug.Log("TIME: " + interactAudioSrc.time + " CLIP : " + interactAudioSrc.clip.length + " condition: " + (interactAudioSrc.time >= interactAudioSrc.clip.length));
            StartCoroutine(FadeInAudio(interactAudioSrc, 2f));
        }
        //StartCoroutine(FadeOutAudio(envAudioSrc, 5f));
    }

    private IEnumerator FadeOutAudio(AudioSource audioSrc, float fadeTime, AudioClip clip = null)
    {
        if (clip != null)
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
    }
    private IEnumerator FadeInAudio(AudioSource audioSrc, float fadeTime, AudioClip clip = null)
    {
        if (clip != null)
            audioSrc.clip = clip;
        //audioSrc.clip = _envClips[1]; //decidi la clip da settare (da usare con 2 audio source)
        float startVolume = 1;
        audioSrc.volume = 0f;
        if (!audioSrc.isPlaying)
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
    }


    public AudioSource GetAudioSource()
    {
        return interactAudioSrc;
    }

    public AudioSource GetEnvAudioSource()
    {
        return envAudioSrc;
    }
    public AudioClip[] GetEnvAudioCLips()
    {
        return _envClips;
    }

    private void OnDestroy()
    {
        //videoPlayer.Stop();
        //envAudioSrc.Stop(); //non puoi farlo!
        _jewel2.OnJewelTouched -= OnJewel2Touched;
        StopAllCoroutines();
    }
}
