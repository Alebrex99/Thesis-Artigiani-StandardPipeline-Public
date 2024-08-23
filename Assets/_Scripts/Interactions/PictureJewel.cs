using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PictureJewel : MonoBehaviour
{
    private bool isPictureTouched = false;
    [SerializeField] private GameObject jewelInformations;
    [SerializeField] private GameObject picture;

    public float rotationDuration = 1f; // Durata della rotazione

    // Start is called before the first frame update
    void Start()
    {
        isPictureTouched = false;
        jewelInformations.SetActive(false);
    }

    public void TouchPicture()
    {
        isPictureTouched = !isPictureTouched;
        Debug.Log("Picture touched " + isPictureTouched);
        if (picture.activeSelf)
        {
            StartCoroutine(RotatePicture(180,picture, jewelInformations));
        }
        else if(jewelInformations.activeSelf)
        {
            Debug.Log("toccato info");
            StartCoroutine(RotatePicture(180, jewelInformations, picture));
            //jewelInformations.SetActive(false);
            //picture.SetActive(true);
        }
    }

    private IEnumerator RotatePicture(float angle, GameObject toHide, GameObject toShow)
    {
        float elapsed = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, angle, 0);

        //if(!isPictureTouched) jewelInformations.SetActive(false);
        while (elapsed < rotationDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
        toHide.SetActive(false);
        toShow.SetActive(true);
        //if(isPictureTouched) jewelInformations.SetActive(true);


    }
}
