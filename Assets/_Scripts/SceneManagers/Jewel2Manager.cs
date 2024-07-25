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
    public AudioClip[] _envClips;
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
        //StartCoroutine(PlayEnvMedia());
        jewel2Informations.SetActive(false);
        foreach (GameObject lateObj in _lateActivatedObj)
        {
            lateObj.SetActive(false);
        }
    }

    void Start()
    {
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
        envAudioSrc.PlayOneShot(_envClips[0], 1); //Environment explanation
        //yield return new WaitForSeconds(_immersionDelay);
        //PlayPicture(); //se verrà messo un video (per ora solo quadro)

    }

    private IEnumerator LateActivationJewel(GameObject[] toActivate, float _immersionDelay)
    {
        yield return new WaitForSeconds(_immersionDelay); //se c'è sostituisci _envExplainDelay
        toActivate[0].SetActive(true);
        //SETTA POSIZIONI
        _jewel2.transform.position = _jewelInitPos.position;
        envAudioSrc.PlayOneShot(_envClips[1], 1); //Jewel explaination
    }

    private IEnumerator LateActivationButtons(GameObject[] toActivate, float _activationDelay)
    {
        yield return new WaitForSeconds(_activationDelay);
        yield return new WaitUntil(() => !envAudioSrc.isPlaying);

        toActivate[1].SetActive(true);
        if (!envAudioSrc.isPlaying)
            envAudioSrc.PlayOneShot(_envClips[2], 1); //Buttons explanation
    }


    private void OnJewel2Touched(Jewel jewel, bool isJewelTouched)
    {
        //riduci regolarmente l'audio dell'ambiente nel giro di 5 secondi
        jewel2Informations.SetActive(isJewelTouched);
        bShowVideo = isJewelTouched;
        if (isJewelTouched)
        {
            StartCoroutine(FadeOutAudio(envAudioSrc, 2f));
        }
        else
        {
            StartCoroutine(FadeInAudio(envAudioSrc, 2f));
        }
        //StartCoroutine(FadeOutAudio(envAudioSrc, 5f));
    }

    private IEnumerator FadeOutAudio(AudioSource audioSrc, float fadeTime)
    {
        //audioSrc.clip = _envClips[1];
        float startVolume = audioSrc.volume;

        while (audioSrc.volume > 0)
        {
            audioSrc.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSrc.Pause();
        audioSrc.volume = startVolume;
    }
    private IEnumerator FadeInAudio(AudioSource audioSrc, float fadeTime)
    {
        //audioSrc.clip = _envClips[1];
        float startVolume = audioSrc.volume;
        audioSrc.volume = 0f;
        audioSrc.UnPause();

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
    }
}
