using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class WorldSpaceVideo : MonoBehaviour
{
    private VideoPlayer _videoPlayer;
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Slider _progressBar;


    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        _volumeSlider.value = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void ChangeVolume()
    {
        _videoPlayer.SetDirectAudioVolume(0, _volumeSlider.value);
    }
}
