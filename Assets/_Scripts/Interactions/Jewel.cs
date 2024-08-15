using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Jewel : MonoBehaviour
{
    [SerializeField] private AudioSource pictureAudioSrc;
    [SerializeField] private AudioClip pictureClip;
    public Action<Jewel, bool> OnJewelTouched;
    private bool isJewelTouched = false;
    public bool intJewelActivate = false;
    [SerializeField] Image playImg;
    [SerializeField] Image pauseImg;

    // Start is called before the first frame update
    private void Awake()
    {
        isJewelTouched = false;
        intJewelActivate = false;
        pictureAudioSrc.clip = pictureClip;
    }
    void Start()
    {
        if(pauseImg != null && playImg != null)
        {
            pauseImg.gameObject.SetActive(false);
            playImg.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TouchJewel()
    {
        if(intJewelActivate == false)
        {
            return;
        }
        Debug.Log("Jewel touched");
        isJewelTouched = !isJewelTouched;
        //Scatena l'azione in modo da fare cose nel rispettivo manager di scena
        if (OnJewelTouched != null)
            OnJewelTouched(this, isJewelTouched);

        if(playImg != null && pauseImg != null)
        {
            if (isJewelTouched)
            {
                playImg.gameObject.SetActive(false);
                pauseImg.gameObject.SetActive(true);
            }
            else
            {
                pauseImg.gameObject.SetActive(false);
                playImg.gameObject.SetActive(true);
            }
        }
        //se la clip non è già avviata
        /*if (isJewelTouched)
        {
            //pictureAudioSrc.PlayOneShot(pictureClip, 1f);
            StartCoroutine(FadeInAudio(pictureAudioSrc, 2f));

        }
        else
        {
            StartCoroutine(FadeOutAudio(pictureAudioSrc, 2f));
        }*/
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
        float startVolume = 1;
        audioSrc.volume = 0f;
        if (!audioSrc.isPlaying)
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

    public AudioSource GetAudioSource()
    {
        return pictureAudioSrc;
    }


}
