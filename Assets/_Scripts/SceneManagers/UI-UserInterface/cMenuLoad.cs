using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class cMenuLoad : MonoBehaviour
{
    public GameObject menuLoadPanel;
    public GameObject goBtLoad;
    public Image imProgressBar;
    // Start is called before the first frame update
    private void Awake()
    {
        menuLoadPanel.SetActive(false);

    }

    public void ShowMenu()
    {
        menuLoadPanel.SetActive(true);
        goBtLoad.SetActive(true);
        //possibili animazioni ... progress bar
    }

    public void HideMenu()
    {
        //stop animazioni ... progress bar
        menuLoadPanel.SetActive(false);
        goBtLoad.SetActive(false);
    }

    public void ClickLoad()
    {
        menuLoadPanel.SetActive(false);
        goBtLoad.SetActive(false);
        
        IntroManager.instance.InitApplication();
        //cMainUIManager.HideLoading();   
    }

}
