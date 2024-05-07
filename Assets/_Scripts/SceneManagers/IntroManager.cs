using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    public static IntroManager instance;

    [SerializeField] private Transform trInitPos;
    public VideoPlayer videoPlayer;
    [SerializeField] private GameObject goVideoPlayer;
    [SerializeField] Animator animLogo;
    [SerializeField] private float rotationVideoSpeed = 1;
    public AudioSource voiceAudio;

    int loopVideo = 0;
    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (videoPlayer.isPlaying)
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
        //QUANDO IL VIDEO FINISCE
        
        //START VOICE AUDIO
        if (voiceAudio != null && videoPlayer !=null)
        {
            videoPlayer.Play();
            voiceAudio.Play();
        }
        videoPlayer.loopPointReached += EndVideo;
        Invoke("EndAudio", voiceAudio.clip.length);
        ResetUserPosition();

    }


    private void EndVideo(VideoPlayer source)
    {
        //cAppManager.GoToSceneAsync(Scenes.HOME);
        //ANIMAZIONI POSSIBILI
        loopVideo++;
        if(loopVideo>= 2)
        {
            source.Stop();
            source.loopPointReached -= EndVideo;
        }
    }

    private void EndAudio()
    {
        cAppManager.GoToSceneAsync(Scenes.HOME);
        //ANIMAZIONI POSSIBILI
    }


    public Transform GetUserInitTr()
    {
        return trInitPos;
    }

    public void ResetUserPosition()
    {
        cXRManager.SetUserPosition(GetUserInitTr().position, GetUserInitTr().rotation);
    }



}
