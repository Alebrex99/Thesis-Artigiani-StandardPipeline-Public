using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class WorldSpaceVideo : MonoBehaviour
{
    private VideoPlayer _videoPlayer;
    [SerializeField] private Slider _volumeSlider;
    //[SerializeField] private Slider _statusBar;


    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        //_statusBar.maxValue = (float)_videoPlayer.clip.length;
        //_statusBar.value = 0;
        _volumeSlider.value = 0.5f;
    }

 


    public void PlayPause()
    {
        if(_videoPlayer.isPlaying)
        {
            _videoPlayer.Pause();
        }
        else
        {
            _videoPlayer.Play();
        }
    }

    public void ChangeVideoVolume()
    {
        _videoPlayer.SetDirectAudioVolume(0, _volumeSlider.value);
    }

    public void ChangeVideoTime()
    {
        
        //_videoPlayer.time = _statusBar.value;
    }
}
