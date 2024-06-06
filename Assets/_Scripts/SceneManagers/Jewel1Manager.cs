using System.Collections;
using System.Collections.Generic;
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

    //VIDEO SOROLLA
    public VideoPlayer videoPlayer;
    [SerializeField] private GameObject goVideoPlayer;
    int loopVideo = 0;
    private bool bShowVideo = false;
    private bool bShowJewel = false;
    [Range(0.1f, 10)]
    [SerializeField] private float rotationVideoSpeed = 1;

    //JEWEL 1
    [SerializeField] private Jewel _jewel1;
    [SerializeField] private Transform _jewelInitPos;


    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        ResetUserPosition();
        StartCoroutine(PlayEnvMedia());
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
        for (int i = 0; i < toActivate.Length; i++)
        {
            toActivate[i].SetActive(true);
            //toActivate[i].transform.position = mainInteractablesInitPos.position; //togliere se si usa cPanelHMDFollower
        }
        _jewel1.transform.position = _jewelInitPos.position;
    }

    private IEnumerator PlayEnvMedia()
    {

        envAudioSrc.PlayOneShot(_envClips[0], 1f); //Environment sounds
        yield return new WaitForSeconds(_immersionDelay);
        envAudioSrc.PlayOneShot(_envClips[1], 0.7f); //Environment explanation
        yield return new WaitForSeconds(_immersionDelay);
        //PlayPicture(); //se verrà messo un video (per ora solo quadro)

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

    private void OnDestroy()
    {
        videoPlayer.Stop();
        envAudioSrc.Stop();
    }

}
