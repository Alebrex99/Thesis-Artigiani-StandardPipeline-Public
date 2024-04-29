using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cFaceTracking : MonoBehaviour{
    public Transform trOrigen;

    private cFaceTarget scrLastObject = null;
    private int layerMask;
    //private LayerMask mask;

    public void Start() {
        layerMask = 1 << 14;// LayerMask.NameToLayer("FaceTracking");
        //mask = LayerMask.GetMask("FaceTracking");
        scrLastObject = null;
    }

    void LateUpdate(){
        RaycastHit hit;
        if (Physics.Raycast(trOrigen.position, trOrigen.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask)) { 
            cFaceTarget scrNew = hit.collider.GetComponent<cFaceTarget>();
            if (null != scrNew) {
                if (null != scrLastObject) {
                    if (scrLastObject.GetInstanceID() != scrNew.GetInstanceID()) {
                        scrLastObject.OnEyeExit();
                        scrLastObject = scrNew;
                        scrLastObject.OnEyeEnter();
                    }
                }
                else {
                    scrLastObject = scrNew;
                    scrLastObject.OnEyeEnter();
                }
            }
            else if (null!=scrLastObject) {
                scrLastObject.OnEyeExit();
                scrLastObject = null;
            }
        }
        else if (null != scrLastObject) {
            scrLastObject.OnEyeExit();
            scrLastObject = null;
        }
    }
}
