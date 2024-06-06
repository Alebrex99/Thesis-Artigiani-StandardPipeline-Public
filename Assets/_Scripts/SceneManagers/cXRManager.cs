using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.Events;

public class cXRManager : MonoBehaviour{
    public Hand leftHand;
    public Hand rightHand;
    public Transform trCenterEye;
    public Transform trUserPosition;
    //ALE public Transform trMunyecaLeft;
    //ALE  public Transform trMunyecaRight;
    public Transform trLeftHand;
    public Transform trRightHand;

    private static cXRManager instance;
    private static UnityEvent poseSelectLeftPaper;
    private static UnityEvent poseSelectRightPaper;
    private static UnityEvent poseSelectRightPistol;
    private static UnityEvent poseSelectLeftPistol;
    private static UnityEvent poseUnselectLeftPaper;
    private static UnityEvent poseUnselectRightPaper;
    private static UnityEvent poseUnselectRightPistol;
    private static UnityEvent poseUnselectLeftPistol;

    private void Awake() {
        instance = this;
        poseSelectLeftPaper = new UnityEvent();
        poseSelectRightPaper = new UnityEvent();
        poseSelectRightPistol = new UnityEvent();
        poseSelectLeftPistol = new UnityEvent();
        poseUnselectLeftPaper = new UnityEvent();
        poseUnselectRightPaper = new UnityEvent();
        poseUnselectRightPistol = new UnityEvent();
        poseUnselectLeftPistol = new UnityEvent();
    }
    public static Hand GetLeftHand() { return instance.leftHand; }
    public static Hand GetRighttHand() { return instance.rightHand; }
    public static Transform GetTrCenterEye() { return instance.trCenterEye; }
    //ALE public static Transform GetTrMunyecaLeft() { return instance.trMunyecaLeft; }
    //ALE public static Transform GetTrMunyecaRight() { return instance.trMunyecaRight; }
    public static Transform GetTrLeftHand() { return instance.trLeftHand; }
    public static Transform GetTrRightHand() { return instance.trRightHand; }
    public static Transform GetTrUserPosition() { return instance.trUserPosition; }


    public static void SetUserPosition(Vector3 position, Quaternion rotation) {
        instance.trUserPosition.position = position;
        instance.trUserPosition.rotation = rotation;
    }

    public void OnPoseSelectLeftPaper() { poseSelectLeftPaper?.Invoke(); }
    public void OnPoseSelectRightPaper() { poseSelectRightPaper?.Invoke(); }
    public void OnPoseSelectRightPistol() { poseSelectRightPistol?.Invoke(); }
    public void OnPoseSelectLeftPistol() { poseSelectLeftPistol?.Invoke(); }
    public void OnPoseUnselectLeftPaper() { poseUnselectLeftPaper?.Invoke(); }
    public void OnPoseUnselectRightPaper() { poseUnselectRightPaper?.Invoke(); }
    public void OnPoseUnselectRightPistol() { poseUnselectRightPistol?.Invoke(); }
    public void OnPoseUnselectLeftPistol() { poseUnselectLeftPistol?.Invoke(); }
    public static void AddListenerPoseSelectLeftPaper(UnityAction call) { poseSelectLeftPaper.AddListener(call);}
    public static void AddListenerPoseSelectRightPaper(UnityAction call) { poseSelectRightPaper.AddListener(call); }
    public static void AddListenerPoseSelectRightPistol(UnityAction call) { poseSelectRightPistol.AddListener(call); }
    public static void AddListenerPoseSelectLeftPistol(UnityAction call) { poseSelectLeftPistol.AddListener(call); }
    public static void AddListenerPoseUnselectLeftPaper(UnityAction call) { poseUnselectLeftPaper.AddListener(call); }
    public static void AddListenerPoseUnselectRightPaper(UnityAction call) { poseUnselectRightPaper.AddListener(call); }
    public static void AddListenerPoseUnselectRightPistol(UnityAction call) { poseUnselectRightPistol.AddListener(call); }
    public static void AddListenerPoseUnselectLeftPistol(UnityAction call) { poseUnselectLeftPistol.AddListener(call); }
    public static void RemoveListenerPoseSelectLeftPaper(UnityAction call) { poseSelectLeftPaper.RemoveListener(call);}
    public static void RemoveListenerPoseSelectRightPaper(UnityAction call) { poseSelectRightPaper.RemoveListener(call); }
    public static void RemoveListenerPoseSelectRightPistol(UnityAction call) { poseSelectRightPistol.RemoveListener(call); }
    public static void RemoveListenerPoseSelectLeftPistol(UnityAction call) { poseSelectLeftPistol.RemoveListener(call); }
    public static void RemoveListenerPoseUnselectLeftPaper(UnityAction call) { poseUnselectLeftPaper.RemoveListener(call); }
    public static void RemoveListenerPoseUnselectRightPaper(UnityAction call) { poseUnselectRightPaper.RemoveListener(call); }
    public static void RemoveListenerPoseUnselectRightPistol(UnityAction call) { poseUnselectRightPistol.RemoveListener(call); }
    public static void RemoveListenerPoseUnselectLeftPistol(UnityAction call) { poseUnselectLeftPistol.RemoveListener(call); }

}
