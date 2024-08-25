using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandDetectionActivator : MonoBehaviour
{
    [SerializeField] private string activatorName;

    // Start is called before the first frame update
    void Start()
    {
        if (HandDetectionManager.instance != null)
        {
            HandDetectionManager.instance.SubscribeActivator(this); // Subscribe to the HandDetectionManager
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public string GetActivatorName()
    {
        return activatorName;
    }
}
