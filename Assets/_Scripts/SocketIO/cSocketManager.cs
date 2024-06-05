using System;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using UnityEditor.PackageManager;
using System.Collections;
using System.Buffers.Text;
using static SocketIOUnity;
using TMPro.Examples;
using OVRSimpleJSON;
using UnityEngine.XR;
using System.IO;
using System.Linq;
using UnityEditor;


public class cSocketManager : MonoBehaviour
{
    //SOCKET IO UNITY
    public static cSocketManager instance;
    public SocketIOUnity socket;
    private bool isConnected = false;

    //RICEZIONE SERVER -> CLIENT
    public static List<byte> conversation = new List<byte>();
    private byte[] audioBuffer;
    private float[] audioBufferFloat; //N bytes / 4byte (1 float = 4 byte)questi sono i SAMPLES dell' AudioClip
    private bool bufferReady = false;
    private int bufferIndex = 0;
    private int bufferSize = 0;
    private int chunkCounter = 0;
    private int responseCounter = 0;
    private bool isPlaying = false;
    public AudioSource receiverAudioSrc;
    public AudioClip clip;
    //Audio clip creata
    private int channels = 2;
    private int frequency = 44100;

    [SerializeField] private GameObject objectToSpin;
    
    //INVIO CLIENT -> SERVER
    private string message = "Hello from Unity!";
    private int replyCounter = 0;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    private void Start() //mettere async void Start() per asincrono
    {   
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

        //-------------- reserved socketio events-----------------------
        /*Un "ping" è un messaggio inviato dal client al server per verificare se il server è ancora raggiungibile e per misurare il tempo di latenza 
         * tra il client e il server. Il server risponderà quindi con un "pong" per confermare la ricezione del messaggio e includerà informazioni 
         * sulla latenza, come ad esempio il tempo impiegato per ricevere il messaggio di ping.*/
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("socket.OnConnected");
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
            //Debug.Log("Pong: " + e.TotalMilliseconds);
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
       

        //----------------CONNECTION ASYNC -------------------------------
        Debug.Log("Connecting...");
        socket.Connect(); //gestione interna di SocketIOUnity
        //await socket.ConnectAsync(); //cambio ad asincrono


        //------------------- RECEIVING = CLIENT REACTIONS TO SERVER (FOR AUDIO CHUNKS)-------------------------
        //receives CHUNKS FROM SERVER : Equivalente delle funzioni sopra, ma prese dirette da SOcketIO (tanto SocketIO Unity estende socketIO)
        socket.On("audio_response_chunk", response =>
        {
            //MODO1 : UNITY SERIALIZATION
            string json = response.ToString().Trim('[', ']');
            DataResponse stringChunk = JsonUtility.FromJson<DataResponse>(json);
            Debug.Log("audio_response_chunk (UNITY Serialization): "+ stringChunk.audio_chunk);
            var base64String = stringChunk.audio_chunk;
            var chunk = Convert.FromBase64String(base64String); //prende una string e converte in bytes
            
            //MODO2 : JOBJECT .NET
            /*var audioChunkObject = response.GetValue<JObject>(); //prende un oggetto JSON
            JToken jtoken = audioChunkObject["audio_chunk"];
            var base64String = jtoken.ToString();
            //var base64String = response.GetValue<string>(); //prima: GetValue<string>("audio_chunk"); possible: data[index]
            var chunk = Convert.FromBase64String(base64String); //prende una string e converte in bytes */
            Debug.Log($"Received chunk of length {chunk.Length}");

            //GESTIONE AUDIO BUFFER REAL TIME
            conversation.AddRange(chunk); //lista perchè il buffer non è dinamico
            //chunk.CopyTo(audioBuffer, bufferIndex); //copia il chunk in audioBuffer
            //Buffer.BlockCopy(chunk, 0, audioBuffer, bufferIndex, chunk.Length); 
            //bufferIndex += chunk.Length;
            //bufferSize += chunk.Length;
            chunkCounter++;
            //Se è tanto delay -> PROVA 2 COROUTINES
        });
        socket.On("audio_response_end", response =>
        {
            Debug.Log("Audio response end: " + response.ToString());
            responseCounter++;

            //GESTIONE AUDIO BUFFER AT END: converto lista run time in array
            audioBuffer = new byte[conversation.Count];
            conversation.CopyTo(audioBuffer);
            Debug.Log("Audio Buffer of BYTE created: " + audioBuffer.Length);
            audioBufferFloat = ConvertByteToFloat(audioBuffer); //per avere i samples dell'AudioClip
            Debug.Log("Audio Buffer of FLOAT converted: " + audioBufferFloat.Length);
            
            bufferReady = true;
            /*if (!isPlaying) //serve per evitare che partano N coroutine separatamente...
            {
                bufferIndex = 0;
                //StartCoroutine(PlayAudioBuffer(audioBufferFloat));
                PlayAudioBuffer(clip);  //pak si riempie -> delay sicuramente -> in audio response end
            }*/
        });

        //------------------- RECEIVING = CLIENT REACTIONS TO SERVER (FOR GAMEOBJECTS) ------------------------
        /*ON UNITY THREAD(Unity wrapper) = SOLO CON PLAYER PREFS SYSTEM, PER EFFETTO SU OGGETTI
         * N.B. NON PUOI USARE OnUnityThread e On contemporaneamente sullo stesso evento
         * Set (unityThreadScope) the thread scope function where the code should run. 
         * Options are: .Update, .LateUpdate or .FixedUpdate, default: UnityThreadScope.Update */
        socket.unityThreadScope = UnityThreadScope.Update; //dove tale thread sta andando
        socket.OnUnityThread("spin", (response) =>
        {
            //objectToSpin.transform.Rotate(0, 45, 0);
            //objectToSpin.transform.position = new Vector3(2, 2, 2);
        });
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
        socket.OnAnyInUnityThread((name, response) =>
        {
            //ReceivedText.text += "Received On " + name + " : " + response.GetValue<string>() + "\n"; //response.GetValue().GetRawText()
            //objectToSpin.transform.Rotate(0, 45, 0);
            //objectToSpin.transform.position = new Vector3(2, 2, 2);
        });
    }

    void Update()
    {
        //POLLING AUDIO BUFFER
        if (bufferReady)
        {
            PlayAudioBuffer(audioBufferFloat);
            bufferReady = false;
        }
        //-------------------------EMITTING FROM CLIENT TO SERVER------------------------
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            socket.Emit("chat_message", 123);
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            socket.Emit("chat_message", 123); //Possibilità 1
            //StartCoroutine(SendMessages(message)); //send message epresso da USER
        }
    }

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

    private void PlayAudioBuffer(float[] audioBufferFloat)
    {
        Debug.Log($"[PLAY] AudioResponse: {responseCounter}");
        Debug.Log("Length of audioBufferFloat: " + audioBufferFloat.Length);

        isPlaying = true;
        //OPZIONE 2: Usare una clip creata esistente e raccogliere i byte per poi riassociarli con i miei
        /*int channels = this.clip.channels;
        int frequency = this.clip.frequency;
        float[] samples = new float[this.clip.samples * this.clip.channels];
        this.clip.GetData(samples, 0);*/
        try
        {
            clip = AudioClip.Create("AudioResponse", audioBufferFloat.Length, channels, frequency, false);
            if (clip == null)
            {
                Debug.LogError("Failed to create AudioClip");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to create AudioClip: " + e.Message);
        }
        try
        {
            bool setDataSuccess = clip.SetData(audioBufferFloat, 0); //DEVE ESSERE FATTO NEL MAIN THREAD
            Debug.Log("SetData success: " + setDataSuccess);   
            if(clip != null)
            {
                receiverAudioSrc.clip = clip;
                receiverAudioSrc.PlayOneShot(clip, 1);
            }
            else
            {
                Debug.LogError("Audio source is null");
            }
        }
        catch(Exception e)
        {
            Debug.LogError("Failed to set data to AudioClip: " + e.Message);
        }
        //yield return new WaitWhile(() => receiverAudioSrc.isPlaying);
        
        //FINE OPERAZIONI CONVERSAZIONE i-esima
        conversation.Clear();
        this.audioBuffer = new byte[0];
        this.audioBufferFloat = new float[0];
        Debug.Log("Audio Buffer cleared: " + audioBuffer.Length);
        isPlaying = false;
    }

    void OnApplicationQuit()
    {
        //StartCoroutine(Disconnect());
        socket.Disconnect(); //gstione interna di SocketIOUnity
        //await socket.DisconnectAsync(); //cambio ad asincrono di SocketIO
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
        isPlaying = true;
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
        isPlaying = false;
    }
}