using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingTrigger : MonoBehaviour
{
    //OGGETTI DA IGNORARE:
    [SerializeField] private GameObject[] goToSwitchOffMenu;

    [SerializeField] private cMenuLoad cMenuLoad;
    [SerializeField] private cLoading cLoading;
    private List<GameObject> triggeredObjs = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        cMainUIManager.instance.OnLoadingEnd += OnLoadingEndEffect;
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnLoadingEndEffect()
    {
        triggeredObjs.ForEach(obj =>
        {
            if (obj != null)
            {
                obj.SetActive(true);
                Debug.Log("RIACCENDO: " + obj.name);
            }

        });
        triggeredObjs.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("ENTRATO: " + other.gameObject.name);
        if (cMenuLoad.IsShowing() && goToSwitchOffMenu.Contains(other.gameObject))
        {
            if(other.GetComponentInParent<OVRCameraRig>() == null)
            {
                other.gameObject.SetActive(false);
                Debug.Log("MENU: spengo " + other.gameObject.name);
            }
        }
        if (cLoading.IsShowing())
        {
            if (other.GetComponentInParent<OVRCameraRig>() == null)
            {
                other.gameObject.SetActive(false);
                triggeredObjs.Add(other.gameObject);
                Debug.Log("LOADING: spengo " + other.gameObject.name);
            }
            
           
        }
    }

    private void OnDestroy()
    {
        cMainUIManager.instance.OnLoadingEnd -= OnLoadingEndEffect;
    }
    
}
