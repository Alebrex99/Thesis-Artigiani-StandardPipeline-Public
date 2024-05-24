using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadingTrigger : MonoBehaviour
{
    //OGGETTI DA IGNORARE:
    [SerializeField] private GameObject[] goToIgnore;
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
        if (goToIgnore.Contains<GameObject>(other.gameObject))
        {
            other.gameObject.SetActive(true);
        }
        //se in questo trigger si trovano i goToIgnore, non devono essere disattivati

    }
}
