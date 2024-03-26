using Meta.WitAi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skybox: MonoBehaviour
{
    [SerializeField] Material skybox1;
    [SerializeField] Material skybox2;
    //[SerializeField] Material skybox3;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DebugSomething()
    {       
        if(RenderSettings.skybox == skybox1)
        {
            RenderSettings.skybox = skybox2;
        }
        else
        {
            RenderSettings.skybox = skybox1;
        }
    }
        
}
