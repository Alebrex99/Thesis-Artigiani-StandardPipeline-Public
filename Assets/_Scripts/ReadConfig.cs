using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;

public class ReadConfig : MonoBehaviour
{
    public static ReadConfig instance;
    //[SerializeField] private TextMeshProUGUI myText;
    public static string filePathCsv;
    //public static string filePathTxt;
    private int currentLineIndex = 0;
   
    //CSV
    //public static List<string> configData = new List<string>();
    public static Dictionary<string, Dictionary<string, float>> configDataMap = new Dictionary<string, Dictionary<string, float>>();

    void Awake()
    {
        instance = this;
        //CSV
        filePathCsv = Application.persistentDataPath + "/config.csv";
        //StartCoroutine(ReadCSVFile(filePathCsv)); //commentato se chiamata da cMenuLoad
    }

    // Start is called before the first frame update
    void Start()
    {
        //filePathTxt = Application.persistentDataPath + "/append.txt";
        //myText.text = GetLineAtIndex(currentLineIndex);

       
    }

    public static IEnumerator ReadCSVFile()
    {
        filePathCsv = filePathCsv.Replace("\\", "/");
        yield return new WaitForEndOfFrame();

        if(!File.Exists(filePathCsv))
        {
            Debug.LogError("File not found: " + filePathCsv + "Create a config file to load main project variables");
            yield break;
        }
        else
        {
            using (StreamReader sr = new StreamReader(filePathCsv))
            {
                bool endOfFile = false;
                string currentScene = null;
                while (!endOfFile)
                {
                    string line = sr.ReadLine();
                    if (line == null)
                    {
                        endOfFile = true;
                        break;
                    }
                    //usa Array
                    /*var data_values = line.Split(';');
                    Debug.Log("Data: " + data_values[0].ToString() + " " + data_values[1].ToString());*/

                    //Usa Lista
                    line.Trim(); //rimuove spazi bianchi
                    line = line.TrimEnd(';');
                    Debug.Log("[READ CONFIG] Line: " + line);

                    var line_values = line.Split(';'); //accesso ai valori della riga: nomeValore / valore
                    if (line_values.Length >= 2)
                    {
                        line_values[0].Trim();
                        line_values[1].Trim();
                        Debug.Log("[READ CONFIG] line_values: first_value=" + line_values[0].ToString() + " second_value=" + line_values[1].ToString() + " line_values_length: " + line_values.Length);
                    }
                    else
                    {
                        Debug.LogWarning("[READ CONFIG] correct line format = SCENE;nomeScene / dato1;valore");
                    }
                    /*SCENE;INTRO
                      dato1;3.5
                      dato2;2.0
                      SCENE;HOME
                      dato1;4.1
                      dato2;1.2
                    mappa: scene1 : dato1, dato2 ; scena2 : dato1, dato2
                    */
                    if (line_values.Length >=2 && line_values[0]== "SCENE") //nome scena 
                    {
                        currentScene = line_values[1].Trim();
                        if(!configDataMap.ContainsKey(currentScene))
                        {
                            configDataMap[currentScene] = new Dictionary<string, float>(); //crei nuovo elemento con chiave SCENA
                            Debug.Log($"[READ CONFIG] New Scene Found: {currentScene}");
                        }
                    }
                    else if(currentScene!=null && line_values.Length>=2)
                    {
                        string parameterName = line_values[0].Trim();
                        if(float.TryParse(line_values[1], out float parameterValue))
                        {
                            configDataMap[currentScene][parameterName] = parameterValue;
                            Debug.Log($"[READ CONFIG] Added: {parameterName} = {parameterValue} to scene {currentScene}");
                        }
                        else
                        {
                            Debug.LogWarning($"[READ CONFIG] Unable to parse value for {parameterName} in scene {currentScene} on line: {line}");
                        }
                      
                        
                    }
                    //configData.Add(line);
                }

            }
        }

        /*StreamReader streamReader = new StreamReader(filePathCsv);
        bool endOfFile = false;
        while (!endOfFile)
        {
           
            string data_String = streamReader.ReadLine();
            if (data_String == null)
            {
                endOfFile = true;
                break;
            }
            var data_values = data_String.Split(',');
            Debug.Log("Data: " + data_values[0].ToString() + " " + data_values[1].ToString() + " " + data_values[2].ToString());
        }*/
    }

    public static void ReadFile()
    {
        instance.StartCoroutine(ReadCSVFile());
    }



    //TXT
    /*private string GetLineAtIndex(int index)
    {
        string[] lines = File.ReadAllLines(filePathTxt);
        if(index < lines.Length)
        {
            return lines[index];
        }
        else
        {
            return "NO MORE LINES.";
        }
    }

    public void NextLine()
    {
        currentLineIndex++;
        //myText.text = GetLineAtIndex(currentLineIndex);
    }*/


}
