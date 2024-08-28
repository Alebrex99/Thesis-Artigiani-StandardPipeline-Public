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
    private bool isSent = false;

    private string voiceToTextMessage ="";
    private void Awake()
    {
        appDictationExperience.TranscriptionEvents.OnFullTranscription.AddListener(OnFullTranscription);
        appDictationExperience.AudioEvents.OnMicStartedListening.AddListener(() =>
        {
            if(appVoiceActive) return; //assicura 1 chiamata sola
            appVoiceActive = true;
            isSent = false;
            voiceToTextMessage = "";
            //START TIMER: Per esser sicuro di inviare al server ciò che ho preso finora
            startListeningTime = DateTime.Now;
            if(text_window != null) text_window.text = "Listening...";
            else Debug.Log("Listening...");
        });
        appDictationExperience.AudioEvents.OnMicStoppedListening.AddListener(() =>
        {
            if(!appVoiceActive) return; //assicura 1 chiamata sola
            appVoiceActive = false;
            if(text_window != null) text_window.text = "Stopped Listening";
            else Debug.Log("Stopped Listening");
            if(voiceToTextMessage.Length <= 0)
            {
                cSocketManager.instance.OnAgentExceptionLauncher(1);
            }
            //se toggle bottone -> stop listening -> cancelli dati e non invii al server
            //se attendi perchè sei sicuro di inviarli 4.5 secondi -> prima invii al server, poi pulisci (stop listening)
            voiceToTextMessage = "";
        });
    }

    private void Update()
    {
        //finchè i secondi totali sono minori di 4.5 e la lunghezza del messaggio è maggiore di 0 : mi assicuro di inviare solo una volta al server
        if ((DateTime.Now - startListeningTime).TotalSeconds >= 4 && voiceToTextMessage.Length > 0 &&!isSent)
        {
            if (!cSocketManager.agentActivate) return;
            Debug.Log("[SENT TO SERVER] time: " + (DateTime.Now - startListeningTime).TotalSeconds);
            StartCoroutine(cSocketManager.instance.SendMessageToServer(this.voiceToTextMessage));
            isSent = true;
        }
    }

    public void OnFullTranscription(string newVoiceToTextMessage)
    {
        if (newVoiceToTextMessage.Length < 0)
        {
            //text_window.text = newVoiceToTextMessage; //fatto direttamente da EDITOR con evento 
            Debug.Log("No te he ecuchado bien, por favor vuelve a apretar el boton y repitelo");
            return;
        }
        startListeningTime = DateTime.Now; //RESET TIMER nel caso in cui riconosca altro parlato
        voiceToTextMessage += newVoiceToTextMessage;
        if(text_window != null) text_window.text = voiceToTextMessage;
        //CHIAMATA ALL'AI : METTO SE DEVE ESSERE POSSIBILE OVUNQUE; altrimenti solo da bottoni
        Debug.Log("Conversational Agent Message: " + voiceToTextMessage);

    }

    public void OnStartedListening()
    {
        appVoiceActive = true;
        isSent = false;
        voiceToTextMessage = "";
        //START TIMER: Per esser sicuro di inviare al server ciò che ho preso finora
        startListeningTime = DateTime.Now;
        if (text_window != null) text_window.text = "Listening...";
        else Debug.Log("Listening...");
    }

    public void OnStoppedListening()
    {
        appVoiceActive = false;
        if (text_window != null) text_window.text = "Stopped Listening";
        else Debug.Log("Stopped Listening");
        if (voiceToTextMessage.Length <= 0)
        {
            cSocketManager.instance.ResetAgent();
            cSocketManager.instance.OnAgentActivation?.Invoke(false);
            cSocketManager.instance.OnAgentException?.Invoke(1);
        }
        //se toggle bottone -> stop listening -> cancelli dati e non invii al server
        //se attendi perchè sei sicuro di inviarli 4.5 secondi -> prima invii al server, poi pulisci (stop listening)
        voiceToTextMessage = "";
    }
}
