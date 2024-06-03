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


public class cSocketManager : MonoBehaviour
{
    //SOCKET IO UNITY
    public static cSocketManager instance;
    public SocketIOUnity socket;
    private bool isConnected = false;
    private bool isPlaying = false;

    //RICEZIONE SERVER -> CLIENT
    public static List<byte[]> conversation = new List<byte[]>();
    Queue<byte[]> audioQueue = new Queue<byte[]>();
    private byte[] audioBuffer = new byte[0];
    private int chunkCounter = 0;
    private int channels = 2;

    [SerializeField] private GameObject objectToSpin;
    
    //INVIO CLIENT -> SERVER
    private string message = "Hello from Unity!";

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
        /*Un "ping" è un messaggio inviato dal client al server per verificare se 
         * il server è ancora raggiungibile e per misurare il tempo di latenza 
         * tra il client e il server. Il server risponderà quindi con un "pong" 
         * per confermare la ricezione del messaggio e includerà informazioni 
         * sulla latenza, come ad esempio il tempo impiegato per ricevere 
         * il messaggio di ping.*/
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
            Debug.Log("Audio response chunk: " + response.ToString());
            chunkCounter++;
            
            //MODO1 : UNITY SERIALIZATION
            string json = response.ToString().Trim('[', ']');
            //json = json.Substring(1, json.Length - 2); //toglie le parentesi quadre
            DataResponse stringChunk = JsonUtility.FromJson<DataResponse>(json);
            Debug.Log("UNITY Serialization: "+ stringChunk.audio_chunk);
            var base64String = stringChunk.audio_chunk;
            var chunk = Convert.FromBase64String(base64String); //prende una string e converte in bytes
            
            //MODO2 : JOBJECT .NET
            /*var audioChunkObject = response.GetValue<JObject>(); //prende un oggetto JSON
            JToken jtoken = audioChunkObject["audio_chunk"];
            var base64String = jtoken.ToString();
            //var base64String = response.GetValue<string>(); //prima: GetValue<string>("audio_chunk"); possible: data[index]
            var chunk = Convert.FromBase64String(base64String); //prende una string e converte in bytes */
            conversation.Add(chunk); //lista con tutti i chunk da usare ovunque nel codice
            Debug.Log($"Received chunk of length {chunk.Length}");

            //DA MANDARE AD UN AUDIO SOURCE
            //audioQueue.Enqueue(chunk); //Uso di una coda
            //Uso Buffer: sarà uno stream unico = unico byte[]
            Buffer.BlockCopy(chunk, 0, audioBuffer, audioBuffer.Length, chunk.Length); 
            //pak si riempie -> delay sicuramente -> in audio response end 
            //Se è tanto delay -> PROVA 2 COROUTINES
           
        });
        socket.On("audio_response_end", response =>
        {
            Debug.Log("Audio response end: " + response.ToString());
            if (!isPlaying) //serve per evitare che partano N coroutine separatamente...
            {
                StartCoroutine(PlayAudioBuffer(audioBuffer));
            }
        });


        //------------------- RECEIVING = CLIENT REACTIONS TO SERVER (FOR GAMEOBJECTS) ------------------------
        /*ON UNITY THREAD(Unity wrapper) = SOLO CON PLAYER PREFS SYSTEM, PER EFFETTO SU OGGETTI
         * N.B. NON PUOI USARE OnUnityThread e On contemporaneamente sullo stesso evento
         * Set (unityThreadScope) the thread scope function where the code should run. 
         * Options are: .Update, .LateUpdate or .FixedUpdate, default: UnityThreadScope.Update */
        socket.unityThreadScope = UnityThreadScope.Update; //dove tale thread sta andando
        socket.OnUnityThread("spin", (response) =>
        {
            objectToSpin.transform.Rotate(0, 45, 0);
            objectToSpin.transform.position = new Vector3(2, 2, 2);
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
            objectToSpin.transform.Rotate(0, 45, 0);
            objectToSpin.transform.position = new Vector3(2, 2, 2);
        });
    }

    void Update()
    {
        //-------------------------EMITTING FROM CLIENT TO SERVER------------------------
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            socket.Emit("chat_message", 123);
        }
        //Quando l'utente parla -> voice to text -> send text
        if(Input.GetKeyDown(KeyCode.Space))
        {
            socket.Emit("chat_message", 123); //Possibilità 1
            //StartCoroutine(SendMessages(message)); //send message epresso da USER
        }
    }

    private float[] ConvertByteToFloat(byte[] array)
    {
        float[] floatArr = new float[array.Length / 4];
        for (int i = 0; i < floatArr.Length; i++)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(array, i * 4, 4);
            floatArr[i] = BitConverter.ToSingle(array, i * 4) / 0x80000000;
        }
        return floatArr;
    }

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
      
            yield return StartCoroutine(PlayAudioBuffer(audioBuffer)); //es) buffer fino a metà
        }
        isPlaying = false;

    }
    private IEnumerator PlayAudioBuffer(byte[] audioBuffer)
    {
        isPlaying = true;
        while (Buffer.ByteLength(audioBuffer) > 0) //ottimizzato
        {
            //Riproduzione audio

            float[] audioBufferFloat = ConvertByteToFloat(audioBuffer);
            AudioClip clip = AudioClip.Create("ClipName", audioBufferFloat.Length, channels,44100 , false); 
            clip.SetData(audioBufferFloat, 0);
            /*
            audioSource.clip = clip;
            audioSource.Play();
            PlayOnce = false;*/
            yield return null;
        }
        isPlaying = false;
        this.audioBuffer = null; //svuota a fine di ogni conversazione
    }

    void OnApplicationQuit()
    {
        //StartCoroutine(Disconnect());
        socket.Disconnect(); //gstione interna di SocketIOUnity
        //await socket.DisconnectAsync(); //cambio ad asincrono di SocketIO
        Debug.Log("Application ending after " + Time.realtimeSinceStartup + " seconds");
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

    //Per UNITY JSON serialization:
    [System.Serializable]
    public class DataResponse
    {
        public string audio_chunk;
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
}