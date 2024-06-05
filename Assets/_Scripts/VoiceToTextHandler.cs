using Meta.Voice.Samples.Dictation;
using Oculus.Voice;
using Oculus.Voice.Dictation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VoiceToTextHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI text_window;
    [Header("Voice Experience")]
    [SerializeField] private AppDictationExperience appDictationExperience;
    private bool appVoiceActive = false;
    private DateTime startListeningTime;
    private int sentCount = 0;

    private string voiceToTextMessage ="";
    private void Awake()
    {
        appDictationExperience.TranscriptionEvents.OnFullTranscription.AddListener(OnFullTranscription);
        appDictationExperience.AudioEvents.OnMicStartedListening.AddListener(() =>
        {
            appVoiceActive = true;
            sentCount = 0;
            voiceToTextMessage = "";
            //START TIMER: Per esser sicuro di inviare al server ciò che ho preso finora
            startListeningTime = DateTime.Now;
            text_window.text = "Listening...";
        });
        appDictationExperience.AudioEvents.OnMicStoppedListening.AddListener(() =>
        {
            appVoiceActive = false;
            text_window.text = "Stopped Listening";
            if(sentCount == 0)
            {
                Debug.Log("[SEND TO SERVER]");
                cSocketManager.instance.SendMessageToServer(this.voiceToTextMessage);
                sentCount++;
            }
        });
    }

    private void Update()
    {
        if ((DateTime.Now - startListeningTime).TotalSeconds >= 5 && voiceToTextMessage.Length > 0 &&sentCount ==0)
        {
            Debug.Log("TEMPO : " + (DateTime.Now - startListeningTime).TotalSeconds);
            Debug.Log("[SEND TO SERVER]");
            cSocketManager.instance.SendMessageToServer(this.voiceToTextMessage);
            sentCount++;
        }
    }

    public void OnFullTranscription(string newVoiceToTextMessage)
    {
        startListeningTime = DateTime.Now; //RESET TIMER
        if (newVoiceToTextMessage.Length < 0)
        {
            //text_window.text = newVoiceToTextMessage; //fatto direttamente da EDITOR con evento 
            Debug.Log("No te he ecuchado bien, por favor vuelve a apretar el boton y repitelo");
            return;
        }
        voiceToTextMessage += newVoiceToTextMessage;
        text_window.text = voiceToTextMessage;
        //CHIAMATA ALL'AI : METTO SE DEVE ESSERE POSSIBILE OVUNQUE; altrimenti solo da bottoni
        Debug.Log("Call Conversational Agent: " + voiceToTextMessage);

    }
}
