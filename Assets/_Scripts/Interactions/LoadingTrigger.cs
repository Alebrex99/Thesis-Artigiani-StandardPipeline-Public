using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("SOLO PLAYER DENTRO TRIGGER");
        if (other.gameObject != GetComponent<OVRCameraRig>())
        {
            Debug.Log("RILEVATI OGGETTI NEL TRIGGER");
            other.gameObject.SetActive(false);
        }
    }
}
