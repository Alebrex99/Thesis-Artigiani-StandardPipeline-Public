using SocketIOClient;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class SocketIO_Init : MonoBehaviour
{
    private SocketIO client;
    private bool isConnected = false;

    //ALE
    private SocketIOUnity client_unity; //SOCKETIO UNITY estende SOCKETIO: puoi accedere a tutte le funzioni di SOCKETIO

    //MESSAGGIO DA INVIARE DA VOICE -> TO TEXT
    private string message = "Hello from Unity!";

    async void Start()
    {
        client = new SocketIO("http://localhost:5000");
        client_unity = new SocketIOUnity("http://localhost:5000");
        client.OnConnected += async (sender, e) =>
        {
            Debug.Log("Connected to server");
            await client.EmitAsync("chat_message", "hola"); //message to INIT 
            Debug.Log("Initial message sent from connect");
        };

        //REACTION FROM SERVER : receves CHUNKS FROM SERVER
        client.On("audio_response_chunk", response =>
        {
            var base64String = response.GetValue<string>(1); //prima: "audio_chunk"; possible: data[index]
            var chunk = Convert.FromBase64String(base64String);
            Debug.Log($"Received chunk of length {chunk.Length}");
        });

        client.On("audio_response_end", response =>
        {
            Debug.Log("Audio response end: " + response.ToString());
        });

        client.OnDisconnected += (sender, e) =>
        {
            Debug.Log("Disconnected from server");
            isConnected = false;
        };

        await Connect();
        StartCoroutine(SendMessages(message)); //send messga epreso da USER
    }

    async Task Connect()
    {
        try
        {
            await client.ConnectAsync();
            isConnected = true;
        }
        catch (Exception e)
        {
            Debug.Log($"Exception: {e}");
        }
    }

    IEnumerator SendMessages(string message) //params -> ENTERO MESSAGE TO SEND
    {
        //TESTING
        while (true)
        {
            if (isConnected)
            {
                //TRASCRITION THE AUDIO
                Debug.Log("Enter message to send: ");
                //string message = Console.ReadLine(); //TO READ FROM terminal: passare message (TEXT) como parametro
                if (message.ToLower() == "exit")
                {
                    StartCoroutine(Disconnect());
                    yield break;
                }
                client.EmitAsync("chat_message", message).Wait();
                Debug.Log("Message sent");
            }
            yield return null;
        }

        //TO SEND MESSAGES FROM USER REALMEMENT, NON CON CONSOLE
        /*if (isConnected)
        {
            //TRASCRITION THE AUDIO
            Debug.Log("Enter message to send: ");
            //string message = Console.ReadLine(); //TO READ FROM temrminal: passare message (TEXT) como parametro
            if (message.ToLower() == "exit")
            {
                StartCoroutine(Disconnect());
                yield break;
            }
            client.EmitAsync("chat_message", message).Wait();
            Debug.Log("Message sent");
        }
        yield return null;*/
        
    }

    IEnumerator Disconnect()
    {
        if (client != null)
        {
            yield return client.DisconnectAsync();
            isConnected = false;
        }
    }

    void OnApplicationQuit()
    {
        StartCoroutine(Disconnect());
    }
}