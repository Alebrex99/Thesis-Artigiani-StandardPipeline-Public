using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class cMenuLoad : MonoBehaviour
{
    public GameObject menuLoadPanel;
    // Start is called before the first frame update
    private void Awake()
    {
        menuLoadPanel.SetActive(false);

    }

    public void ShowMenu()
    {
        menuLoadPanel.SetActive(true);
    }

    public void HideMenu()
    {
        menuLoadPanel.SetActive(false);
    }

    public void ClickLoad()
    {
        menuLoadPanel.SetActive(false);
        //cMainUIManager.ShowLoading();
        //carica file config
        ReadConfig.ReadFile(); //salvataggio del File nella Lista statica condivisa
        //cMainUIManager.HideLoading();
        IntroManager.instance.HideMenuCanvas();
    }
}
