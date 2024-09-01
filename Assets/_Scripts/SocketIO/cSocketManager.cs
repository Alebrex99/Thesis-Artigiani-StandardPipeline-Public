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
using NAudio.Wave;


public class cSocketManager : MonoBehaviour
{
    /*public enum SocketState
    {
        IDLE,
        LISTENING,
        SPEAKING,
        RECEIVING,
    }
    private SocketState _currentSocketState;*/

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
    private Coroutine playAudioBufferCor;
    public static bool serverException = false;
    public Action<int> OnAgentException;

    //VERSIONE CON LATENZA COSTANTE : RICEZIONE SERVER -> CLIENT
    private Dictionary<int, AudioClip> audioClipDictionary = new Dictionary<int, AudioClip>();
    private AudioClip lastAudioClip;
    private int chunksBufferNumber = 10;
    private int chunkCounter = 0;
    private bool isAudioResponseEnd = false;
    private bool isPlayingBuffer = false;
    private int audioClipIndex = -1;
    private int currentClipIndex = 0;

    //VERSIONE RUN TIME PLAYBACK
    /*public ConcurrentQueue<byte[]> audioQueue = new ConcurrentQueue<byte[]>(); //usa concurrent queue per thread safety
    private AudioClip audioClip;
    private int totalSamples = 0;
    //private MemoryStream memStreamRT;
    //private BinaryWriter writerRT;
    //private MpegFile mpgFileRT;
    private int currentPosition = 0;
    private const int initClipLength = 60; //samples = freq * channels * seconds
    private float[] audioBuffRT;
    private int bufferOffset = 0;
    private bool bufferReadyRT = false;*/

    //EFFETTI SULLA SCENA
    [Header("Audio Source")]
    [SerializeField] private AudioSource agentBipSrc;
    [Header("Bip Clips")]
    [SerializeField] private AudioClip[] agentBipClips; //0 ON , 1 OFF, 2 SENT, 3 WAIT
    public static bool agentActivate = false;
    public Action<bool> OnAgentActivation;
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
        //var uri = new Uri("http://localhost:5000"); //Funziona con server MIKEL; bisognerà poi modificare l'indirizzo con uno internet
        var uri = new Uri("http://192.168.1.30:5000"); //OK IP wi fi locale casa Bergamo (controlla sempre)
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
            Debug.Log("socket.OnConnected");
            //socket.Emit("chat_message", "hola"); //message to INIT, chiama internamente EmitAsync
            //Debug.Log("Initial message sent from connect");
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
            //Debug.Log($"Received chunk of length {chunk.Length}");

            //GESTIONE AUDIO BUFFER STANDARD:
            chunkCounter++; //high numer = 110 chunks ; low number = 15 chunks
            conversation.AddRange(chunk);
            //Per Latenza costante a 5 secondi circa (10 chunks)
            if (chunkCounter == chunksBufferNumber) //se arrivo a 10 chunks
            {
                Debug.Log("[SOCKET V.2] Load on Chunk counter = " + chunkCounter + " total length: " + conversation.Count);
                chunkCounter = 0; //resetti il contatore
                audioClipIndex++; //parte da 0
                var audioClipIndexLoc = audioClipIndex; //salvo nella action corrente
                var conversationArray = conversation.ToArray();
                conversation.Clear(); //svuoti la conversazione
                //Passaggio a Unity Main Thread: esegue 1 volta in update
                UnityThread.executeInUpdate(() =>
                {
                    //audioClipIndex++; //parte da 0
                    //var audioClipIndexLoc = audioClipIndex; //salvo nella action corrente
                    var audioClip = LoadHelperNLayer(conversationArray);
                    //lock(audioClipDictionary){ } //se vuoi fare un lock
                    lock (audioClipDictionary)
                    {
                        if (!audioClipDictionary.ContainsKey(audioClipIndexLoc))
                        {
                            audioClipDictionary[audioClipIndexLoc] = audioClip;
                            Debug.Log($"Put clip {audioClipIndexLoc} in DICTIONARY");
                        }
                        else
                        {
                            Debug.LogWarning("Received duplicate clip index: " + audioClipIndexLoc);
                        }
                    }
                });
            }

            //GESTIONE AUDIO BUFFER REALTIME
            //OnReceiveAudioChunk(chunk); //PRODUTTORE per gestire il buffer in tempo reale
        });

        socket.On("audio_response_end", response =>
        {
            //Audio response end: {'status': 'end'} -> non c'è audio_chunk
            isAudioResponseEnd = true;
            isReceiving = false;
            if (stopReceiving)
            {
                //conversation.Clear(); //non serve, messo in reset
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
            audioClipIndex++; //parte da 0
            var audioClipIndexLoc = audioClipIndex; //salvo nella action corrente
            var conversationArray = conversation.ToArray();
            if (chunkCounter == 0) //caso in cui ho esattamente multiplo di 10 chunks (se no lancerebbe socketException)
            {
                UnityThread.executeInUpdate(() =>
                {
                    lastAudioClip = null;
                    lock (audioClipDictionary)
                    {
                        if (!audioClipDictionary.ContainsKey(audioClipIndexLoc))
                        {
                            audioClipDictionary[audioClipIndexLoc] = lastAudioClip;
                            Debug.Log($"Put LAST clip {audioClipIndexLoc} in DICTIONARY");
                        }
                        else
                        {
                            Debug.LogWarning("Received duplicate clip index: " + audioClipIndexLoc);
                        }
                    }
                });
                return;
            }
            //Passaggio a Unity Main Thread: esegue 1 volta in update
            UnityThread.executeInUpdate(() => {
                //audioClipIndex++;
                //var audioClipIndexLoc = audioClipIndex; //salvo nella action corrente
                lastAudioClip = LoadHelperNLayer(conversationArray);
                //lock(audioClipDictionary){ } //se vuoi fare un lock
                lock (audioClipDictionary)
                {
                    if (!audioClipDictionary.ContainsKey(audioClipIndexLoc))
                    {
                        audioClipDictionary[audioClipIndexLoc] = lastAudioClip;
                        Debug.Log($"Put LAST clip {audioClipIndexLoc} in DICTIONARY");
                    }
                    else
                    {
                        Debug.LogWarning("Received duplicate clip index: " + audioClipIndexLoc);
                    }
                }
            });
            Debug.Log("[SOCKET V.2] END Load on Chunk counter = " + chunkCounter + " total length: " + conversation.Count);
            chunkCounter = 0;
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
        if (playAudioBufferCor == null && audioClipDictionary.ContainsKey(currentClipIndex))
        {
            //lock(audioClipDictionary){ } //se vuoi fare un lock
            lock (audioClipDictionary)
            {
                var clipToPlay = audioClipDictionary[currentClipIndex];
                audioClipDictionary.Remove(currentClipIndex);
                if (agentBipSrc.clip == agentBipClips[3])
                {
                    agentBipSrc.loop = false;
                    agentBipSrc.Stop();
                }
                playAudioBufferCor = StartCoroutine(PlayAudioBufferCor(clipToPlay));
            }
        }
        /*else if(currentClipIndex == audioClipDictionary.Count && isLastClipReady && !receiverAudioSrc.isPlaying && isAudioResponseEnd)
        {
            Debug.Log("Play END audio");
            agentBipSrc.loop = false;
            agentBipSrc.Stop();
            StartCoroutine(PlayLastBufferCor(audioBufferFloat));
        }*/

        if (serverException)
        {
            OnAgentExceptionLauncher(0);
            serverException = false;
        }

        //REAL TIME AUDIO BUFFER
        /*if (audioQueue.Count > 2)
        {
            StartCoroutine(PlayAudioBufferRT()); //deve leggere il buffer
            //StartCoroutine(ProcessAudioQueue());
        }*/
    }

    //---------------------------VERSIONE LATENZA COSTANTE-------------------------------
    private AudioClip LoadHelperNLayer(byte[] conversationArray)
    {
        Debug.Log("Status : Loading conversation..." + audioClipIndex);
        try
        {
            var audioBufferFloat = new float[0];
            using (var memStream = new MemoryStream(conversationArray))
            using (var mpgFile = new MpegFile(memStream)) //crea un mpgFile in byte (es 100)
            {
                int samplesCount = (int)mpgFile.Length / 4; //lunghezza in float
                int tollerance = 4; //4 float in più
                audioBufferFloat = new float[samplesCount + tollerance]; //crei un buffer di float (1 float = 4 byte), es 100 floats
                mpgFile.ReadSamples(audioBufferFloat, 0, samplesCount); //leggi 
            }
            var clip = AudioClip.Create($"AudioResponse_{audioClipIndex}", audioBufferFloat.Length, channels, sampleRate, false);
            bool setDataSuccess = clip.SetData(audioBufferFloat, 0); //DEVE ESSERE FATTO NEL MAIN THREAD
            Debug.Log("Status : Conversation loaded " + audioClipIndex);
            return clip;
        }
        catch (Exception ex)
        {
            Debug.LogError("Custom Error: error loading audio: " + ex.Message);
            if (conversationArray.Length == 0) Debug.LogWarning("Conversation is empty");
            else Debug.Log("Conversation length: " + conversationArray.Length);
            serverException = true;
            return null;
        }
    }
    private IEnumerator PlayAudioBufferCor(AudioClip clip)
    {
        yield return new WaitUntil(() => isPlayingBuffer == false && !receiverAudioSrc.isPlaying);
        isPlayingBuffer = true;
        Debug.Log($"[PLAY] AudioClip: {currentClipIndex} length: {clip.length}");
        receiverAudioSrc.Stop();
        receiverAudioSrc.clip = null;
        receiverAudioSrc.clip = clip;
        if (clip != null) receiverAudioSrc.PlayOneShot(clip, 1f);
        else Debug.Log("Clip is null");

        if (clip == lastAudioClip && isAudioResponseEnd) Debug.Log("Play END audio");
        //Debug.Log("[MPEG AUDIO CONVERSION] samples: " + audioBufferFloat.Length + " clip duration: " + clip.length + " channels: " + channels + " Sample Rate frequency: " + sampleRate);
        //INFO : [MPEG AUDIO CONVERSION] samples: 3497472 channels: 1 Sample Rate frequency: 44100

        yield return new WaitForSeconds(clip.length);
        Debug.Log("[STOP] AudioClip: " + currentClipIndex + " length: " + clip.length);

        //lock(audioClipDictionary){ } //se vuoi fare un lock
        currentClipIndex++;
        playAudioBufferCor = null;

        if (clip == lastAudioClip && isAudioResponseEnd)
        {
            Debug.Log("----------- AUTOMATIC RESET -----------");
            agentBipSrc.loop = false;
            agentBipSrc.Stop();
            ResetAgent();
            OnAgentActivation?.Invoke(agentActivate);
        }
        isPlayingBuffer = false;
    }


    //-----------------------------------VERSIONE BASE--------------------------------------
    private void LoadHelperNLayer()
    {
        Debug.Log("Status : Loading conversation...");
        try
        {
            using (var memStream = new MemoryStream(conversation.ToArray()))
            using (var mpgFile = new MpegFile(memStream)) //crea un mpgFile in byte (es 100)
            {
                int samplesCount = (int)mpgFile.Length / 4; //lunghezza in float
                audioBufferFloat = new float[samplesCount]; //crei un buffer di float (1 float = 4 byte), es 100 floats
                mpgFile.ReadSamples(audioBufferFloat, 0, samplesCount); //leggi 
            }
            Debug.Log("Status : Conversation loaded");
            //bufferReady = true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Custom Error: error loading audio: " + ex.Message);
            if (conversation.Count == 0) Debug.LogWarning("Conversation is empty");
            else Debug.Log("Conversation length: " + conversation.Count);
            serverException = true;
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
            receiverAudioSrc.PlayOneShot(clip, 1f);
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
        //agentActivate = false; //RESET permette nuovamente di parlare
        //OnCallToggleManagerAudios(false); // quando comincia a parlare silenzia tutto
    }
    private IEnumerator PlayLastBufferCor(float[] audioBufferFloat)
    {
        Debug.Log($"[PLAY] AudioResponse: {responseCounter}");
        try
        {
            var clip = AudioClip.Create("AudioResponse", audioBufferFloat.Length, channels, sampleRate, false);
            bool setDataSuccess = clip.SetData(audioBufferFloat, 0); //DEVE ESSERE FATTO NEL MAIN THREAD
            receiverAudioSrc.clip = clip;
            receiverAudioSrc.PlayOneShot(clip, 1f);
            Debug.Log("[MPEG AUDIO CONVERSION] samples: " + audioBufferFloat.Length + " clip duration: " + clip.length + " channels: " + channels + " Sample Rate frequency: " + sampleRate);
            //INFO : [MPEG AUDIO CONVERSION] samples: 3497472 channels: 1 Sample Rate frequency: 44100
        }
        catch (Exception e)
        {
            Debug.LogError("Custom Error: Failed to create AudioClip: " + e.Message);
        }

        yield return new WaitForSeconds(receiverAudioSrc.clip.length); //se crei il buffer con lunghezza in byte (4 volte la lenght in floats) -> receiverAudioSrc.clip.length/4
        Debug.Log("Audio source stopped playing");
        ResetAgent();
        OnAgentActivation?.Invoke(agentActivate);
        //OnCallToggleManagerAudios(true); //accendi audio scena (puoi convertirlo ad azione nel manager)
    }


    //PROVA VERSIONE RUN TIME
    /*IEnumerator PlayAudioBufferRT()
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
    }*/

    /*private void OnReceiveAudioChunk(byte[] chunk)
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
        }
    }*/

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

    /*private void OnAudioRead(float[] data)
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

    }*/

    //CONTROLLO DELLA RICEZIONE E ABILITAZIONE A PARLARE
    public void ToggleSocket()
    {
        Debug.Log("Toggle SOCKET Conversational Agent");
        Debug.Log("Socket Connected: " + isConnected);
        if (!isConnected)
        {
            OnAgentExceptionLauncher(2);
            return;
        }
        //CheckTransition();

        //OPERAZIONI GENERALI DEL SOCKET
        if (isReceiving)
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
            //scegli se disattivare
        }
        if(agentBipSrc.isPlaying)
        {
            agentBipSrc.Stop();
        }

        //CONTROLLO AGENTE :
        if (!agentActivate) //se agente è disattivato -> lo attivi
        {
            ActivateAgent();
            OnAgentActivation?.Invoke(agentActivate);
            //OnCallToggleManagerAudios(false); //spegni audio scena (puoi convertirlo ad azione nel manager)
        }
        else // se agente è attivato -> lo spegni
        {
            ResetAgent();
            OnAgentActivation?.Invoke(agentActivate);
            //OnCallToggleManagerAudios(true); //accendo audio scena (puoi convertirlo ad azione nel manager)
        }
        //conversation.Clear(); //messo in reset
    }

    //POSSIBLE : Trasform into Action
    public void OnCallToggleManagerAudios(bool isToggled = false)
    {
        Debug.Log("Scena : " + cAppManager.GetActualScene());
        switch (cAppManager.GetActualScene())
        {
            case Scenes.HOME:
                if (HomeManager.instance != null)
                {
                    if (isToggled == false)
                        HomeManager.instance.PauseAudioScene();
                    else HomeManager.instance.UnPauseAudioScene();
                }
                break;
            case Scenes.JEWEL1:
                if (Jewel1Manager.instance != null)
                {
                    if (isToggled == false)
                        Jewel1Manager.instance.PauseAudioScene();
                    else Jewel1Manager.instance.UnPauseAudioScene();
                }
                break;
            case Scenes.JEWEL2:
                if (Jewel2Manager.instance != null)
                {
                    if (isToggled == false)
                        Jewel2Manager.instance.PauseAudioScene();
                    else Jewel2Manager.instance.UnPauseAudioScene();
                }
                break;
            case Scenes.JEWEL3:
                if (Jewel3Manager.instance != null)
                {
                    if (isToggled == false)
                        Jewel3Manager.instance.PauseAudioScene();
                    else Jewel3Manager.instance.UnPauseAudioScene();
                }
                break;
            case Scenes.JEWEL4:

                break;
            default:
                break;
        }
    }

    public void ActivateAgent()
    {
        Debug.Log("[CONV AGENT]--------ATTIVO CONVERSATIONAL AGENT------- " + agentActivate);
        agentActivate = true; // attivi agente
        _dictationActivation.ToggleActivation(agentActivate); //attivi microfono
        //BIP ATTIVAZIONE -> parli -> invii -> ricevi -> agente parla
        if (agentBipSrc != null)
            agentBipSrc.PlayOneShot(agentBipClips[0], 1f);
    }

    public void ResetAgent()
    {
        agentActivate = false; //disattivi agente
        conversation.Clear();
        audioClipDictionary.Clear();
        isAudioResponseEnd = false;
        audioClipIndex = -1;
        currentClipIndex = 0;

        stopReceiving = true;
        Debug.Log("CHANGE stop Receiving -> " + stopReceiving);

        _dictationActivation.ToggleActivation(agentActivate); //disattivi microfono
        
        agentBipSrc.loop = false;
        if (agentBipSrc.isPlaying) agentBipSrc.Stop();
        if (playAudioBufferCor!=null) StopCoroutine(playAudioBufferCor);
        if (receiverAudioSrc.isPlaying) receiverAudioSrc.Stop();

        Debug.Log("[CONV AGENT]--------DISATTIVO CONVERSATIONAL AGENT------ " + agentActivate);
        //BIP DISATTIVAZIONE -> quando lo disattivi: mentre parli / mentre ricevi / mentre agente parla
        if (agentBipSrc != null)
            agentBipSrc.PlayOneShot(agentBipClips[1], 1f);
    }

    //EXCEPTIONS:
    /*0 : server exception = fail creation MP3
      1 : message legth = 0
      2 : socket not connected*/
    public void OnAgentExceptionLauncher(int exceptionNumber)
    {
        if (agentActivate)
        {
            ResetAgent();
            OnAgentActivation?.Invoke(agentActivate);
        }
        OnAgentException?.Invoke(exceptionNumber);
    }

    /*public void CheckTransition()
    {
        SocketState newSocketState = _currentSocketState;
        switch (_currentSocketState)
        {
            case SocketState.IDLE:
                stopReceiving = false;
                Debug.Log("CHANGE stop Receiving -> " + stopReceiving);
                _dictationActivation.ToggleActivation(); //attivo
                newSocketState = SocketState.LISTENING;
                break;
            case SocketState.LISTENING:
                _dictationActivation.ToggleActivation(); //disattivo
                newSocketState = SocketState.IDLE;
                break;
            case SocketState.SPEAKING:
                if (receiverAudioSrc.isPlaying)
                {
                    receiverAudioSrc.Stop();
                    stopReceiving = false;
                    _dictationActivation.ToggleActivation(); //attivo
                    newSocketState = SocketState.LISTENING;
                    break;
                }
                break;
            case SocketState.RECEIVING:
                //CASO 1 : STO RICEVENDO ANCORA
                if (isReceiving)
                {
                    stopReceiving = true;
                    Debug.Log("CHANGE stop receiving -> " + stopReceiving);
                    newSocketState = SocketState.IDLE;
                    break;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        if (newSocketState != _currentSocketState)
        {
            Debug.Log($"Changing State FROM:{_currentSocketState} --> TO:{newSocketState}");
            _currentSocketState = newSocketState;
        }
    }*/


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

    public IEnumerator SendMessageToServer(string message, string evenName = "")
    {
        //AUDIO OF SENDING MESSAGE
        if(agentBipSrc!=null) agentBipSrc.PlayOneShot(agentBipClips[2], 1f); //clip invio messagio
        //TO SEND MESSAGES FROM CLIENT TO SERVER
        if (isConnected)
        {
            //TRASCRITION THE AUDIO
            if (message.ToLower() == "exit")
            {
                socket.Disconnect(); //gstione interna di SocketIOUnity
            }
            socket.Emit("chat_message", message); //.Wait() (usato nei thread per attendere che il thread finisca) ma si bloccava prima
            Debug.Log("Message sent");
        }
        //yield return new WaitForSeconds(1f);
        //waiting background sound
        if (agentBipSrc != null)
        {
            agentBipSrc.loop = true;
            agentBipSrc.clip = agentBipClips[3];
            agentBipSrc.Play();
        }
        yield return null;
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