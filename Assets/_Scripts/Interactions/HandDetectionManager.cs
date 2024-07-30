using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandDetectionManager : MonoBehaviour
{
    public static HandDetectionManager instance;
    public HandDetectionActivator buttons;
    public float hideDelay = 3f;
    private float timer;
    private bool handsDetected = true;
    private bool isActive = false;
    public float handHeightThreshold = 0.4f; // Altezza rispetto alla testa sotto la quale nascondere i bottoni

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
            return;
        
        handsDetected = AreHandsDetected();
       
        if (handsDetected)
        {
            timer = 0f;
            SetButtonsActive(true);
        }
        else
        {
            timer += Time.deltaTime;
            if (timer >= hideDelay)
            {
                SetButtonsActive(false);
            }
        }
    }

    private bool AreHandsDetected()
    {
        bool handsTracked = cXRManager.GetLeftHand().IsTrackedDataValid || cXRManager.GetRightHand().IsTrackedDataValid;
        if (!handsTracked) return false;

        bool handsAboveThreshold =
            (cXRManager.GetLeftHand().IsTrackedDataValid && cXRManager.GetTrLeftHand().position.y > cXRManager.GetTrCenterEye().position.y - handHeightThreshold) ||
            (cXRManager.GetRightHand().IsTrackedDataValid && cXRManager.GetTrRightHand().position.y > cXRManager.GetTrCenterEye().position.y - handHeightThreshold);

        return handsAboveThreshold;
    }


    private void SetButtonsActive(bool isActive)
    {
        if(buttons != null)
            buttons.gameObject.SetActive(isActive);
    }

    public void Activate()
    {
        isActive = true;
        timer = 0f;
        Debug.Log("HandDetectionManager activated");
        if (buttons == null)
        {
            buttons = FindObjectOfType<HandDetectionActivator>(true);
        }
    }

    public void Deactivate()
    {
        isActive = false;
        SetButtonsActive(true);
        Debug.Log("HandDetectionManager deactivated");
    }
}
