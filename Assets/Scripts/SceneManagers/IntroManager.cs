using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private AudioClip _voiceAudio;
    private AudioSource _audioSource;

    // Start is called before the first frame update
    void Start()
    {
        //START VOICE AUDIO
        _audioSource = GetComponentInChildren<AudioSource>();
        // START VIDEO
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
