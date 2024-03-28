using Meta.WitAi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxManager: MonoBehaviour
{
    [SerializeField] Material skyboxImg360Mono;
    [SerializeField] Material skyboxImg180Mono;
    [SerializeField] Material skyboxBase;
    [SerializeField] GameObject _environement;
    [SerializeField] GameObject _office;
    public FadeScreen fadeScreen;
    

    private void Start()
    {
        RenderSettings.skybox = skyboxBase;
    }
    public void OnButton1Pressed()
    {   /*  
        if (_environement != null)
        {
            _environement.SetActive(true);
        }
        if (_office != null)
        {
            _office.SetActive(false);
        }
        */
        if(RenderSettings.skybox == skyboxImg360Mono)
        {
            RenderSettings.skybox = skyboxBase;
        }
        else
        {   
            RenderSettings.skybox = skyboxImg360Mono;
        }
    }

    public void OnButton3Pressed()
    {

    }
        
}
