using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Jewel : MonoBehaviour
{
    [SerializeField] private AudioSource audioSourceJewel;
    [SerializeField] private AudioClip jewelClip;
    public Action<Jewel, bool> OnJewelTouched;
    private bool isJewelTouched = false;

    // Start is called before the first frame update
    void Start()
    {  
        isJewelTouched = false;
        audioSourceJewel.clip = jewelClip;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TouchJewel()
    {
        Debug.Log("Jewel touched");
        isJewelTouched = !isJewelTouched;
        //Scatena l'azione in modo da fare cose nel rispettivo manager di scena
        if (OnJewelTouched != null)
            OnJewelTouched(this, isJewelTouched);
        //se la clip non è già avviata
        if (isJewelTouched)
        {
            //audioSourceJewel.PlayOneShot(jewelClip, 1f);
            StartCoroutine(FadeInAudio(audioSourceJewel, 3f));
        }
        else
        {
            StartCoroutine(FadeOutAudio(audioSourceJewel, 3f));
        }
    }

    private IEnumerator FadeOutAudio(AudioSource audioSrc, float fadeTime)
    {
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
        float startVolume = audioSrc.volume;
        audioSrc.volume = 0f;
        if (isJewelTouched)
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



}
