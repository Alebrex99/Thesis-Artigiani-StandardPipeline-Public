using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class cMenuLoad : MonoBehaviour
{
    public GameObject menuLoadPanel;
    public GameObject goBtLoadPhysic;
    public Image imProgressBar;
    // Start is called before the first frame update
    private void Awake()
    {
        menuLoadPanel.SetActive(false);

    }

    public void ShowMenu()
    {
        menuLoadPanel.SetActive(true);
        if (goBtLoadPhysic!=null) 
        { 
            //goBtLoad.SetActive(true);
            goBtLoadPhysic.SetActive(true); 
        }
        //possibili animazioni ... progress bar
    }

    public void HideMenu()
    {
        //stop animazioni ... progress bar
        menuLoadPanel.SetActive(false);
        if(goBtLoadPhysic != null)
        {
            //goBtLoad.SetActive(false);
            goBtLoadPhysic.SetActive(false);
        }
       
    }

    public void ClickLoad()
    {
        menuLoadPanel.SetActive(false);
        if(goBtLoadPhysic != null)
        {
            //goBtLoad.SetActive(false);
            goBtLoadPhysic.SetActive(false);
        }
        IntroManager.instance.InitApplication();
        //cMainUIManager.HideLoading();   
    }

    public bool IsShowing()
    {
        return menuLoadPanel.activeSelf;
    }

}
