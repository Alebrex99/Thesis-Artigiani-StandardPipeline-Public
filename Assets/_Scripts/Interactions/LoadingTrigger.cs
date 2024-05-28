using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("ENTRATO: " + other.gameObject.name);
        if (cMenuLoad.IsShowing() && goToSwitchOffMenu.Contains(other.gameObject))
        {
            other.gameObject.SetActive(false); //solo se in lista di spegnimento
            Debug.Log("MENU: spengo " + other.gameObject.name);

        }
        if (cLoading.IsShowing())
        {
            //triggeredObjs.Add(other.gameObject);
            other.gameObject.SetActive(false);
            Debug.Log("LOADING: spengo " + other.gameObject.name);
        }
    }
}
