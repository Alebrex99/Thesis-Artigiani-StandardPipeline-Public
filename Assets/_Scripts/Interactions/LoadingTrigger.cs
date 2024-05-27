using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadingTrigger : MonoBehaviour
{
    //OGGETTI DA IGNORARE:
    [SerializeField] private GameObject[] goToIgnore;
    [SerializeField] private cMenuLoad cMenuLoad;
    private bool isMenu = false;

    // Start is called before the first frame update
    void Start()
    {
        if(cMenuLoad != null && cMenuLoad.IsShowing())
        {
            isMenu = true;
        }
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (isMenu)
        {

            if (!goToIgnore.Contains<GameObject>(other.gameObject))
            {
                other.gameObject.SetActive(false);
                Debug.Log("DEVE ESSERCI SOLO IL MENU");
            }
            //se in questo trigger si trovano i goToIgnore, non devono essere disattivati
        }
        else //alora è solo loading cambio scena
        {
            //se il pannello attivo è quello di loading allora disattiva tutti gli oggetti col trigger eccetto la OVR camera rig
            if (!other.gameObject.Equals(goToIgnore[0]))
            {
                other.gameObject.SetActive(false);
                Debug.Log("DEVE ESSERCI SOLO IL LOADING");
            }
        }
         
    }
}
