using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    public static IntroManager instance;
    
    public VideoPlayer videoPlayer;
    

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        //QUANDO IL VIDEO FINISCE
        videoPlayer.loopPointReached += EndVideo;
    }

    private void EndVideo(VideoPlayer source)
    {
        videoPlayer.loopPointReached -= EndVideo;
        videoPlayer.Stop();
        cAppManager.instance.GoToSceneAsync(Scenes.HOME);
        //ANIMAZIONI POSSIBILI
    }



    
}
