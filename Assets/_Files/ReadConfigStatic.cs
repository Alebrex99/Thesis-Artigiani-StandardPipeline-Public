using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ReadConfigStatic
{
    public static ReadConfig instance;
    //[SerializeField] private TextMeshProUGUI myText;
    private static string filePath;
    private static string filePathTxt;

    public static List<string> configData = new List<string>();

    public static void ReadCSVFile()
    {
        filePath = Application.persistentDataPath + "/config.csv";
        filePath = filePath.Replace("\\", "/");

        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
        }
        else
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                bool endOfFile = false;
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
                    var line_values = line.Split(';'); //accesso ai valori della riga
                    configData.Add(line);
                    Debug.Log("Line: " + line);

                }
            }
        }
    }
}
