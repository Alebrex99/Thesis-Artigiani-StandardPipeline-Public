using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class Jewel1Manager : MonoBehaviour
{
    public static Jewel1Manager instance;

    //FEATURES DA cStBase
    public Transform userInitPos;
    public Transform trLightJewel;

    //GESTIONE SCENA + IMMERSIONE
    public AudioSource envAudioSrc;
    public AudioClip[] _envClips;
    [Range(0, 60)]
    [SerializeField] private float _immersionDelay = 1f;
    [Range(0, 60)]
    [SerializeField] private float _activationDelay = 1f;
    [SerializeField] private GameObject[] _lateActivatedObj;

    //VIDEO/QUADRO SOROLLA
    public VideoPlayer videoPlayer;
    [SerializeField] private GameObject goVideoPlayer;
    [SerializeField] private GameObject sorollaPicture;
    [SerializeField] private GameObject jewel1Informations;
    int loopVideo = 0;
    private bool bShowVideo = false;
    [Range(0.1f, 10)]
    [SerializeField] private float rotationVideoSpeed = 1;

    //JEWEL 1
    [SerializeField] private Jewel _jewel1;
    [SerializeField] private Transform _jewelInitPos;


    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
        _jewel1.OnJewelTouched += OnJewel1Touched;
    }

    void Start()
    {
        ResetUserPosition();
        StartCoroutine(PlayEnvMedia());
        jewel1Informations.SetActive(false);
        foreach (GameObject lateObj in _lateActivatedObj)
        {
            lateObj.SetActive(false);
        }
        StartCoroutine(LateActivation(_lateActivatedObj, _activationDelay));

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
            Vector3 euler = Quaternion.LookRotation(goVideoPlayer.transform.position - cXRManager.GetTrCenterEye().position).eulerAngles;
            //SOSTITUIRE : goVideoPlayer se voglio un altro oggetto quando il video si spegne; es) goLogoCentral
            goVideoPlayer.transform.eulerAngles = new Vector3(0, euler.y, 0);
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
        envAudioSrc.PlayOneShot(_envClips[1], 0.7f); //Environment explanation
        //yield return new WaitForSeconds(_immersionDelay);
        //PlayPicture(); //se verrà messo un video (per ora solo quadro)

    }

    private void OnJewel1Touched(Jewel jewel, bool isJewelTouched)
    {
        //riduci regolarmente l'audio dell'ambiente nel giro di 5 secondi
        sorollaPicture.SetActive(!isJewelTouched);
        bShowVideo = isJewelTouched;
        jewel1Informations.SetActive(isJewelTouched);
        if(isJewelTouched) {
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
    private IEnumerator FadeInAudio(AudioSource audioSrc, float fadeTime)
    {
        //audioSrc.clip = _envClips[1]; //decidi la clip da settare (da usare con 2 audio source)
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




    private void PlayPicture()
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
        _jewel1.OnJewelTouched -= OnJewel1Touched;
    }

}
