using Evereal.VRVideoPlayer;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Jewel : MonoBehaviour
{
    [SerializeField] private AudioSource pictureAudioSrc;
    [SerializeField] private AudioClip pictureClip;
    public Action<Jewel, bool> OnJewelTouched;
    private bool isJewelTouched = false;
    public bool interactJewelActivate = false;
    [SerializeField] string jewelName;
    [SerializeField] Image playImg;
    [SerializeField] Image pauseImg;
    [SerializeField] private TMP_Text text_label;
    private Color text_label_color;

    // Start is called before the first frame update
    private void Awake()
    {
        isJewelTouched = false;
        interactJewelActivate = false;
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
        if(interactJewelActivate == false)
        {
            //possiamo fare qualcosa qui, prima che i bottoni nel gioiello si attivino, es. etichetta
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
                ChangeLabel();
            }
            else
            {
                pauseImg.gameObject.SetActive(false);
                playImg.gameObject.SetActive(true);
                ResetLabel();
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

    public void ChangeLabel()
    {
        if (text_label != null)
        {
            text_label_color = text_label.color;
            if(GetJewelName()== "Jewel1") text_label.color = new Color(0, 152, 255, 255); // Set the color to #0098FF
            if(GetJewelName() == "Jewel2") text_label.color = Color.white; // Set the color to #FF0000
            if (GetJewelName() == "Jewel3") text_label.color = new Color(146, 226, 0, 255); // Set the color to #92E200
            text_label.text = "Mira a derecha";
        }
    }
    public void ResetLabel()
    {
        if (text_label != null)
        {
            text_label.color = text_label_color;
            text_label.text = "Pulse o toca la joya";
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

    public void SetInteractJewel(bool value)
    {
        interactJewelActivate = value;
    }

    public string GetJewelName()
    {
        return jewelName;
    }

}
