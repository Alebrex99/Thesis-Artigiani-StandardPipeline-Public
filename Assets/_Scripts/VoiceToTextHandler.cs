using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceToTextHandler : MonoBehaviour
{
    public static VoiceToTextHandler instance;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        
    }
    void Update()
    {
        
    }
    public static void VoiceToText(string voiceToText)
    {
        //CHIAMATA ALL'AI : METTO SE DEVE ESSERE POSSIBILE OVUNQUE; altrimenti solo da bottoni
        Debug.Log("Call Conversational Agent: " + voiceToText);
        //STOPPARE A MANO LA REGISTRAZIONE :
        //cSocketManager.instance.socket.Emit("voiceToText", voiceToText);
    }
}
