using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    public static IntroManager instance;
    [SerializeField] private Transform trInitPos;

    //VIDEO
    public VideoPlayer videoPlayer;
    int loopVideo = 0;

    //AUDIO
    public AudioSource voiceAudio;

    //cSceneInfo
    private bool bShownVideo = false;
    private float timeLastClick = 0;
    [SerializeField] private GameObject goVideoPlayer;
    [SerializeField] Animator animLogo;
    [SerializeField] private float rotationVideoSpeed = 1;
    


    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
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

    // Start is called before the first frame update
    void Start()
    {   
        //START VOICE AUDIO
        if (voiceAudio != null && videoPlayer !=null)
        {
            videoPlayer.Play();
            bShownVideo = true;
            voiceAudio.Play();
        }
        videoPlayer.loopPointReached += EndVideo;
        Invoke("EndAudio", voiceAudio.clip.length);
        ResetUserPosition();
    }

    private void EndVideo(VideoPlayer source)
    {
        loopVideo++;
        if(loopVideo>= 2)
        {
            source.loopPointReached -= EndVideo;
            source.Stop();

            //ANIMAZIONI POSSIBILI : LOGO -> VIDEO
            /*animLogo.ResetTrigger("ShowVideo");
            animLogo.SetTrigger("HideVideo");*/
            bShownVideo = false; //smetterà di seguire l'utente
        }
    }

    private void EndAudio()
    {
        cAppManager.GoToSceneAsync(Scenes.HOME);
        //ANIMAZIONI POSSIBILI : LOGO -> VIDEO
    }

    public Transform GetUserInitTr()
    {
        return trInitPos;
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
