using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PictureJewel : MonoBehaviour
{
    public Action<bool> OnPictureTouched;
    private bool isPictureTouched = false;
    [SerializeField] private GameObject jewelInformations;
    [SerializeField] private GameObject picture;

    public float rotationDuration = 1f; // Durata della rotazione
    private Quaternion initialPictureRotation;
    private Quaternion initialInformationsRotation;

    // Start is called before the first frame update
    void Start()
    {
        isPictureTouched = false;
        jewelInformations.SetActive(false);
        initialPictureRotation = picture.transform.rotation;
        initialInformationsRotation = jewelInformations.transform.rotation;
    }

    public void TouchPicture()
    {
        isPictureTouched = !isPictureTouched;
        Debug.Log("Picture touched " + isPictureTouched);
        //Scatena l'azione in modo da fare cose nel rispettivo manager di scena
        OnPictureTouched?.Invoke(isPictureTouched);

        if (isPictureTouched)
        {
            StartCoroutine(RotatePicture(180, picture, jewelInformations));
        }
        else
        {
            //StartCoroutine(RotatePicture(-180, picture, jewelInformations));
            jewelInformations.SetActive(false);
            picture.SetActive(true);
        }
    }

    private IEnumerator RotatePicture(float angle, GameObject toHide, GameObject toShow)
    {
        float elapsed = 0f;
        Quaternion startRotation = toHide.transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, angle, 0);

        //if(!isPictureTouched) jewelInformations.SetActive(false);
        while (elapsed < rotationDuration)
        {
            toHide.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        toHide.transform.rotation = endRotation;
        toShow.SetActive(true);
        //if(isPictureTouched) jewelInformations.SetActive(true);


    }
}
