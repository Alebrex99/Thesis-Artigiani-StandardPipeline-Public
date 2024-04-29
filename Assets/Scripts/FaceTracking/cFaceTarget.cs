using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cFaceTarget : MonoBehaviour{
    public string nombre;

    private void Awake() {
        //SET LAYER
        int layerEye = LayerMask.NameToLayer("FaceTracking");
        gameObject.layer = layerEye;
    }
    public void OnEyeEnter() {
        //ALE cDataManager.AddResponse(eDataSesionAction.MIRA_OBJ_IN, nombre);
        //Debug.Log("[EYE] Enter: " + nombre);
    }
    public void OnEyeExit() {
        //ALE cDataManager.AddResponse(eDataSesionAction.MIRA_OBJ_OUT, nombre);
        //Debug.Log("[EYE] Exit: " + nombre);
    }
}
