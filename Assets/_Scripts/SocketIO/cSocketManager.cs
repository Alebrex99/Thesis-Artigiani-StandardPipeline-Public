using System;
using System.IO;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections;
using static SocketIOUnity;
using System.Linq;
using UnityEditor;
using NLayer;
using Meta.WitAi.Data;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using System.Collections.Concurrent;
using UnityEngine.UIElements;
using UnityEngine.Profiling;
using Meta.Voice.Samples.Dictation;
using static OVRPlugin;
using NLayer.Decoder;


public class cSocketManager : MonoBehaviour
{
    //SOCKET IO UNITY
    public static cSocketManager instance;
    public SocketIOUnity socket;
    private bool isConnected = false;
    
    //CONVERSATIONAL AGENT
    [SerializeField] private cDictationActivation _dictationActivation;

    //VERSIONE CON LATENZA : RICEZIONE SERVER -> CLIENT
    public static List<byte> conversation = new List<byte>();
    private bool stopReceiving = false;
    private bool isReceiving = false;
    private float[] audioBufferFloat; //N bytes / 4byte (1 float = 4 byte)questi sono i SAMPLES dell' AudioClip
    private bool bufferReady = false;
    private int responseCounter = 0;
    public AudioSource receiverAudioSrc;
    private int sampleRate = 44100;
    private int channels = 1;
    //private MpegFile mpgFile;

    //VERSIONE RUN TIME PLAYBACK
    public ConcurrentQueue<byte[]> audioQueue = new ConcurrentQueue<byte[]>(); //usa concurrent queue per thread safety
    private AudioClip audioClip;
    private int totalSamples = 0;
    //private MemoryStream memStreamRT;
    //private BinaryWriter writerRT;
    //private MpegFile mpgFileRT;
    private int currentPosition = 0;
    private const int initClipLength = 60; //samples = freq * channels * seconds
    private float[] audioBuffRT;
    private int bufferOffset = 0;
    private bool bufferReadyRT = false;

    [SerializeField] private GameObject objectToSpin;


    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    private void Start() //mettere async void Start() per asincrono
    {
        //SETTING REAL TIME AUDIO CLIP
        /*totalSamples = sampleRate * initClipLength * channels;
        audioClip = AudioClip.Create("StreamingAudio", totalSamples, channels, sampleRate, true, OnAudioRead);
        //audioClip = AudioClip.Create("StreamingAudio", totalSamples, channels, sampleRate, false);
        receiverAudioSrc = GetComponent<AudioSource>();
        //receiverAudioSrc.loop = true; //CASINO: viene riprodotta ogni conversazione N volte
        receiverAudioSrc.clip = audioClip;
        audioBuffRT = new float[totalSamples]; //non è dinamico
        Debug.Log("[AI] Creata Audio clip: " + totalSamples + " channels: " + channels + " Sample Rate frequency: " + sampleRate);
        */
        
        receiverAudioSrc = GetComponent<AudioSource>();

        //------------------SOCKET IO UNITY--------------------------------
        //var uri = new Uri("http://192.168.1.107:11100"); //DEFAULT: IP non corretto
        //var uri = new Uri("http://localhost:11100"); //Funziona con SERVER: C:\Users\Utente\UnityProjects\SocketIOUnity\Samples~\Server
        var uri = new Uri("http://localhost:5000"); //Funziona con server MIKEL; bisognerà poi modificare l'indirizzo con uno internet
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
                {
                    {"token", "UNITY" }
                }
            ,
            EIO = 4
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();
        //------------------RESERVED SOCKETIO EVENTS-----------------------
        /*Un "ping" è un messaggio inviato dal client al server per verificare se il server è ancora raggiungibile e per misurare il tempo di latenza 
         * tra il client e il server. Il server risponderà quindi con un "pong" per confermare la ricezione del messaggio e includerà informazioni 
         * sulla latenza, come ad esempio il tempo impiegato per ricevere il messaggio di ping.*/
        socket.OnConnected += (sender, e) =>
        {
            //Debug.Log("socket.OnConnected");
            socket.Emit("chat_message", "hola"); //message to INIT, chiama internamente EmitAsync
            Debug.Log("Initial message sent from connect");
            isConnected = true; // Set isConnected to true when connected
        };
        socket.OnPing += (sender, e) =>
        {
            //Debug.Log("Ping");
        };
        socket.OnPong += (sender, e) =>
        {
            //Debug.Log("Pong: " + e.TotalMilliseconds); //e contiene messaggio con info per es su latenza
        };
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("Disconnected from server: " + e);
            isConnected = false;
        };
        socket.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log($"{DateTime.Now} Reconnecting: attempt = {e}");
        };
       

        //--------------------- CONNECTION -------------------------------
        Debug.Log("[SocketIO] Connecting...");
        socket.Connect(); //gestione interna di SocketIOUnity
        //await socket.ConnectAsync(); //se voglio usare direttamente il metodo di socketIO


        //------------------- RECEIVING = CLIENT REACTIONS TO SERVER (FOR AUDIO CHUNKS)-------------------------
        socket.On("audio_response_chunk", response =>
        {
            isReceiving = true;
            if (stopReceiving)
            {
                Debug.Log("DISCARD stopReceiving= " + stopReceiving);
                return;
            }

            //MODO1 : UNITY SERIALIZATION
            string json = response.ToString().Trim('[', ']');
            DataResponse stringChunk = JsonUtility.FromJson<DataResponse>(json);
            //Debug.Log("audio_response_chunk (UNITY Serialization): "+ stringChunk.audio_chunk);
            var base64String = stringChunk.audio_chunk;
            var chunk = Convert.FromBase64String(base64String); //prende una string e converte in bytes
            
            //MODO2 : JOBJECT .NET
            /*var audioChunkObject = response.GetValue<JObject>(); //prende un oggetto JSON
            JToken jtoken = audioChunkObject["audio_chunk"];
            var base64String = jtoken.ToString();
            //var base64String = response.GetValue<string>(); //prima: GetValue<string>("audio_chunk"); possible: data[index]
            var chunk = Convert.FromBase64String(base64String); //prende una string e converte in bytes */
            Debug.Log($"Received chunk of length {chunk.Length}");

            //GESTIONE AUDIO BUFFER STANDARD:
            conversation.AddRange(chunk); //lista perchè il buffer non è dinamico

            //GESTIONE AUDIO BUFFER REALTIME
            //OnReceiveAudioChunk(chunk); //PRODUTTORE per gestire il buffer in tempo reale
        });

        socket.On("audio_response_end", response =>
        {
            isReceiving = false;
            if (stopReceiving)
            {
                conversation.Clear();
                stopReceiving = false;
                Debug.Log("ALL SENT : CHANGE stop receiving -> " + stopReceiving);
                return;
            }
            Debug.Log("Audio response end: " + response.ToString());
            responseCounter++;

            //VERSIONE STANDARD : CONVERSIONE MANUALE BYTE -> FLOAT
            /*audioBuffer = new byte[conversation.Count];
            conversation.CopyTo(audioBuffer);
            //Debug.Log("Audio Buffer of BYTE created: " + audioBuffer.Length);
            audioBufferFloat = ConvertByteToFloat(audioBuffer); //per avere i samples dell'AudioClip
            //Debug.Log("Audio Buffer of FLOAT converted: " + audioBufferFloat.Length);*/

            //VERSIONE NLAYER LIBRARY
            LoadHelperNLayer();
        });

        //------------------- RECEIVING = CLIENT REACTIONS TO SERVER (FOR GAMEOBJECTS) ------------------------
        /*ON UNITY THREAD(Unity wrapper) = SOLO CON PLAYER PREFS SYSTEM, PER EFFETTO SU OGGETTI
         * N.B. NON PUOI USARE OnUnityThread e On contemporaneamente sullo stesso evento
         * Set (unityThreadScope) the thread scope function where the code should run. 
         * Options are: .Update, .LateUpdate or .FixedUpdate, default: UnityThreadScope.Update */
        //socket.unityThreadScope = UnityThreadScope.Update; //dove tale thread sta andando
        /*socket.OnUnityThread("spin", (response) =>
        {
            //objectToSpin.transform.Rotate(0, 45, 0);
            //objectToSpin.transform.position = new Vector3(2, 2, 2);
        });*/
        //Corrisponde esattamente a :
        /*socket.On("audio_response_end", response =>
        {
            Debug.Log("Audio response end: " + response.ToString());
            UnityThread.executeInUpdate(() => {
                objectToSpin.transform.Rotate(0, 45, 0);
            });
            // or  
            //UnityThread.executeInLateUpdate(() => { ... });
            // or 
            //UnityThread.executeInFixedUpdate(() => { ... });
        });*/
        
        //Per reagire a qualunque tipo di evento emesso (corrisponde a socket.onAny()):
        //ReceivedText.text = "";
        /*socket.OnAnyInUnityThread((name, response) =>
        {
            //ReceivedText.text += "Received On " + name + " : " + response.GetValue<string>() + "\n"; //response.GetValue().GetRawText()
            //objectToSpin.transform.Rotate(0, 45, 0);
            //objectToSpin.transform.position = new Vector3(2, 2, 2);
        });*/
    }


    void Update()
    {
        //POLLING AUDIO BUFFER (o usa un unity thread dispatcher): non permette di creare audio source in runtime altrimenti
        if (bufferReady && !receiverAudioSrc.isPlaying)
        {         
            PlayAudioBuffer(audioBufferFloat);
        }

        //REAL TIME AUDIO BUFFER
        /*if (audioQueue.Count > 2)
        {
            StartCoroutine(PlayAudioBufferRT()); //deve leggere il buffer
            //StartCoroutine(ProcessAudioQueue());
        }*/
    }

    private void LoadHelperNLayer()
    {
        Debug.Log("Status : Loading conversation...");
        try
        {
            using (var memStream = new MemoryStream(conversation.ToArray()))
            using(var mpgFile = new MpegFile(memStream))
            {
                audioBufferFloat = new float[mpgFile.Length];
                mpgFile.ReadSamples(audioBufferFloat, 0, (int)mpgFile.Length);
            }
            bufferReady = true;
        }
        catch (Exception ex) {
            Debug.LogError("Custom Error: error loading audio: " + ex.Message);
        }
        
    }

    private void PlayAudioBuffer(float[] audioBufferFloat)
    {
        Debug.Log($"[PLAY] AudioResponse: {responseCounter}");
        try
        {
            var clip = AudioClip.Create("AudioResponse", audioBufferFloat.Length, channels, sampleRate, false);
            bool setDataSuccess = clip.SetData(audioBufferFloat, 0); //DEVE ESSERE FATTO NEL MAIN THREAD
            receiverAudioSrc.clip = clip;
            receiverAudioSrc.PlayOneShot(clip, 1);
            Debug.Log("[MPEG AUDIO CONVERSION] samples: " + audioBufferFloat.Length + " channels: " + channels + " Sample Rate frequency: " + sampleRate);
            //INFO : [MPEG AUDIO CONVERSION] samples: 3497472 channels: 1 Sample Rate frequency: 44100
        }
        catch (Exception e)
        {
            Debug.LogError("Custom Error: Failed to create AudioClip: " + e.Message);
        }
        //PULIZIA DEI BUFFERS:
        conversation.Clear();
        bufferReady = false;
    }


    //NON FUNZIONA PERCHE' A CAUSA DELLA CONCORRENZA PRELEVANO CHUNKS DALAL CODA NON IN ORDINE RELATVIO
    IEnumerator PlayAudioBufferRT()
    {
        while (audioQueue.Count > 0)
        {

            if (audioQueue.TryDequeue(out byte[] chunk))
            {
                using (var memStreamRT = new MemoryStream(chunk)) //prendi un chunk di 10 byte
                using (var mpgFileRT = new MpegFile(memStreamRT)) //crei un file di 10 byte
                {
                    int readSamplesFloat = mpgFileRT.ReadSamples(audioBuffRT, bufferOffset, (int)mpgFileRT.Length); //leggi meno elementi perchè sono float
                    bufferOffset += readSamplesFloat;
                    mpgFileRT.Position = 0;
                    //Debug.Log(" samples read: " + samplesRead + "mpg length: " + mpgFileRT.Length + " bufferOffset: " + bufferOffset + " samplesToRead: " + samplesToRead);
                    if(bufferOffset > 0) bufferReadyRT = true;
                }
            }
            if (!receiverAudioSrc.isPlaying)
            {
                receiverAudioSrc.Play();
                Debug.Log("Audio source started playing");
            }
            yield return null;
        }
    }

    private void OnReceiveAudioChunk(byte[] chunk)
    {
        audioQueue.Enqueue(chunk); // coda per il real time
        /*using (var memStreamRT = new MemoryStream(chunk)) //prendi un chunk di N byte
        using (var mpgFileRT = new MpegFile(memStreamRT)) //crei un file di N byte
        {
            float[] chunkFloat = new float[mpgFileRT.Length];
            int readSamplesFloat = mpgFileRT.ReadSamples(chunkFloat, 0, (int)mpgFileRT.Length); //leggi meno elementi perchè sono float
            
            audioQueue.Enqueue(chunkFloat); // coda per il real time
            Debug.Log("[AI] Audio queue count (Enqueue): " + audioQueue.Count);
            //bufferOffset += readSamplesFloat;
            //mpgFileRT.Position = 0;
            //Debug.Log(" samples read: " + samplesRead + "mpg length: " + mpgFileRT.Length + " bufferOffset: " + bufferOffset + " samplesToRead: " + samplesToRead);
        }*/
    }

    /*private IEnumerator ProcessAudioQueue()
    {
        while (true)
        {
            if (audioQueue.TryDequeue(out float[] samples))
            {
                int samplesToCopy = Mathf.Min(samples.Length, audioBuffRT.Length - bufferOffset);
                Array.Copy(samples, 0, audioBuffRT, bufferOffset, samplesToCopy);
                bufferOffset += samplesToCopy;
                
                if(bufferOffset > 0) bufferReadyRT = true;
                // If the buffer overflows, wrap around
                if (bufferOffset >= audioBuffRT.Length)
                {
                    bufferOffset = 0;
                    bufferReadyRT = false;
                }
            }
            if (!receiverAudioSrc.isPlaying)
            {
                receiverAudioSrc.Play();
                Debug.Log("Audio source started playing");
            }
            yield return null;
        }
    }*/

    private void OnAudioRead(float[] data)
    {
        int count = 0;
        //data sono i singoli campioni dell'AudioClip
        
        while (count < data.Length && bufferReadyRT) 
        {
            if (currentPosition < bufferOffset)
            {
                data[count] = audioBuffRT[currentPosition];
                currentPosition++;
            } 
            else
            {
                Debug.Log("silence");
                data[count] = 0; // Fill with silence if buffer is empty
            }
            if(currentPosition >= bufferOffset)
            {
                Debug.Log("PROBLEMA: current: " + currentPosition + " bufferOffset: " + bufferOffset);
                bufferReadyRT = false;
            }
            count++;
            
        }

    }


    public void ToggleSocket()
    {
        if(isReceiving)
        {
            stopReceiving = true;
            Debug.Log("CHANGE stop receiving -> " + stopReceiving);
        }
        else
        {
            stopReceiving = false;
            Debug.Log("CHANGE stop Receiving -> " + stopReceiving);
        }
        
        if (receiverAudioSrc.isPlaying)
        {
            receiverAudioSrc.Stop();
            stopReceiving = false;
        }
        conversation.Clear();
        _dictationActivation.ToggleActivation();
    }



    void OnApplicationQuit()
    {
        //StartCoroutine(Disconnect());
        socket.Disconnect(); //gstione interna di SocketIOUnity
        //disconnessione completa dal server
        socket.Dispose();
        Debug.Log("Application ending after " + Time.realtimeSinceStartup + " seconds");
    }

    //Per UNITY JSON serialization:
    [System.Serializable]
    public class DataResponse
    {
        public string audio_chunk;
    }

    public void SendMessageToServer(string message, string evenName = "")
    {
        //TO SEND MESSAGES FROM CLIENT TO SERVER
        if (isConnected)
        {
            //TRASCRITION THE AUDIO
            Debug.Log("Enter message to send: ");
            if (message.ToLower() == "exit")
            {
                socket.Disconnect(); //gstione interna di SocketIOUnity
            }
            socket.Emit("chat_message", message); //.Wait() (usato nei thread per attendere che il thread finisca) ma si bloccava prima
            Debug.Log("Message sent");
        }
    }
















    //-----------------------------FUNZIONI DI SUPPLEMENTO-----------------------------------------------
    private float[] ConvertByteToFloat(byte[] array)
    {
        //DEBUG BYTE ARRAY:
        Debug.Log("Byte array: " + BitConverter.ToString(array));

        float[] floatArr = new float[array.Length / 4];
        for (int i = 0; i < floatArr.Length; i++)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(array, i * 4, 4);
            float value = BitConverter.ToSingle(array, i * 4); //float = 4 bytes, quindi converti a indice ogni 4
            floatArr[i] = Mathf.Clamp(value, -1f, 1f);
        }

        //DEBUG FLOAT ARRAY:
        string floatString = string.Join(", ", floatArr.Select(f => f.ToString()));
        Debug.Log("Float array: " + floatString);
        return floatArr;
    }
    private byte[] ConvertFloatToByte(float[] array)
    {
        byte[] byteArr = new byte[array.Length * 4];
        for (int i = 0; i < array.Length; i++)
        {
            var bytes = BitConverter.GetBytes(array[i] * 0x80000000);
            Array.Copy(bytes, 0, byteArr, i * 4, bytes.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(byteArr, i * 4, 4);
        }
        return byteArr;
    }
    public void EmitTest()
    {
        //string eventName = EventNameTxt.text.Trim().Length < 1 ? "hello" : EventNameTxt.text;
        //string txt = DataTxt.text;
        /*
        if (!IsJSON(txt))
        {
            socket.Emit(eventName, txt);
            //socket.Emit(txt); //il server da errore perchè vuole un JSON= json.decoder.JSONDecodeError: Expecting ',' delimiter: line 1 column 4 (char 3)

        }
        else
        {
            socket.EmitStringAsJSON(eventName, txt);
        }*/
    }
    public static bool IsJSON(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) { return false; }
        str = str.Trim();
        if ((str.StartsWith("{") && str.EndsWith("}")) || //For object
            (str.StartsWith("[") && str.EndsWith("]"))) //For array
        {
            try
            {
                var obj = JToken.Parse(str);
                return true;
            }catch (Exception ex) //some other exception
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    public void EmitSpin()
    {
        socket.Emit("spin");
    }

    public void EmitClass()
    {
        TestClass testClass = new TestClass(new string[] { "foo", "bar", "baz", "qux" });
        TestClass2 testClass2 = new TestClass2("lorem ipsum");
        socket.Emit("class", testClass2);
    }

    // Test class for emit
    class TestClass
    {
        public string[] arr;

        public TestClass(string[] arr)
        {
            this.arr = arr;
        }
    }

    [System.Serializable]
    class TestClass2
    {
        public string text;

        public TestClass2(string text)
        {
            this.text = text;
        }
    }

    //FUNZIONI ULTERIORI DA POTER USARE PER DISCONNESSIONE E INVIO MESSAGGI
    IEnumerator SendMessages(string message) //params -> ENTERO MESSAGE TO SEND
    {
        //TO SEND MESSAGES FROM USER NON CON CONSOLE
        if (isConnected)
        {
            //TRASCRITION THE AUDIO
            Debug.Log("Enter message to send: ");
            if (message.ToLower() == "exit")
            {
                StartCoroutine(Disconnect());
                yield break;
            }
            socket.EmitAsync("chat_message", message); //.Wait() ma si bloccava prima
            Debug.Log("Message sent");
        }
        yield return null;
    }
    IEnumerator Disconnect()
    {
        if (socket != null)
        {
            yield return socket.DisconnectAsync();
            isConnected = false;
        }
    }

    //SECOND STEP : REPRODUCE REAL TIME CHUNKS WHEN I RECEVICE THEM
    private IEnumerator UseAudioBuffer(byte[] audioBuffer)
    {
        /*
        while (audioQueue.Count>0)
        {
            var chunk = audioQueue.Dequeue();
            yield return StartCoroutine(PlayAudioBuffer(chunk));    
        }
        isPlaying = false;*/
        yield return new WaitForSeconds(5f);
        while (Buffer.ByteLength(audioBuffer) > 0) //ottimizzato
        {
            float[] audioBufferFloat = ConvertByteToFloat(audioBuffer);

            //yield return StartCoroutine(PlayAudioBuffer(audioBufferFloat)); //es) buffer fino a metà
        }
    }
}