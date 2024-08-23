using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandDetectionManager : MonoBehaviour
{
    public static HandDetectionManager instance;
    private HandDetectionActivator[] allButtons;
    public HandDetectionActivator buttons;
    public HandDetectionActivator buttonHome1;
    public HandDetectionActivator buttonHome2;
    public float hideDelay = 3f;
    private float timer;
    private bool handsDetected = true;
    private bool isActive = false;
    public float handHeightThreshold = 0.35f; // Altezza rispetto alla testa sotto la quale nascondere i bottoni

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("HandDetectionManager duplicated. Deleting the new one.");
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
        if(buttonHome1 != null)
            buttonHome1.gameObject.SetActive(isActive);
        if (buttonHome2 != null)
            buttonHome2.gameObject.SetActive(isActive);
    }

    public void Activate()
    {
        isActive = true;
        timer = 0f;
        Debug.Log("HandDetectionManager activated");
        if (buttons == null || buttonHome1==null || buttonHome2==null)
        {
            //buttons = FindObjectOfType<HandDetectionActivator>(true);
            allButtons = FindObjectsOfType<HandDetectionActivator>(true);
            Debug.Log("Found " + allButtons.Length + " HandDetectionActivator objects");
            buttons = allButtons[0];
            buttonHome1 = allButtons[1];
            buttonHome2 = allButtons[2];

        }
    }

    public void Deactivate()
    {
        isActive = false;
        SetButtonsActive(true);
        Debug.Log("HandDetectionManager deactivated");
    }
}
