using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class cSceneInfo : MonoBehaviour{
    public Transform trInitPos;
    public Transform[] trSlots;
    public GameObject[] goTeleportPoints;
    public VideoPlayer vp;
    public GameObject goVideoPlayer;
    public GameObject goLogoCentral;
    public Animator animLogo;

    private bool bShowVideo = false;
    private float timeLastClick = 0;

    //ADDED FOR METRICS (ALE)
    public Action OnButtonVideoPressed;

    private void Update() {
        if (bShowVideo) {
            Vector3 euler = Quaternion.LookRotation(goVideoPlayer.transform.position - cXRManager.GetTrCenterEye().position).eulerAngles;
            goVideoPlayer.transform.eulerAngles = new Vector3(0, euler.y, 0);
        }
        else {
            Vector3 euler = Quaternion.LookRotation(goLogoCentral.transform.position - cXRManager.GetTrCenterEye().position).eulerAngles;
            goLogoCentral.transform.eulerAngles = new Vector3(0, euler.y, 0);
        }
    }

    public Transform GetUserInitTr() {
        return trInitPos;
    }
    public Transform GetSlotTr(int id) {
        trSlots[id].GetChild(0).gameObject.SetActive(true);
        return trSlots[id];
    }

    public int GetNumSlots() {
        return trSlots.Length;
    }

    public void ShowPath(bool show) {
        foreach(GameObject go in goTeleportPoints)
            go.SetActive(show);
    }
    public void ClickLogo() {
        Debug.Log("Tocado logo");
        animLogo.ResetTrigger("HideVideo");
        animLogo.SetTrigger("ShowVideo");
        vp.loopPointReached += EndVideo;
        vp.isLooping = true;
        vp.Play();
        bShowVideo = true;
    }
    public void ClickButtonVideo() {
        //MODIFICA METRICHE
        if (OnButtonVideoPressed != null)
            OnButtonVideoPressed();

        if (Time.realtimeSinceStartup - timeLastClick < 1) {
            return;
        }
        if (bShowVideo) {
            vp.loopPointReached -= EndVideo;
            vp.Stop();
            animLogo.ResetTrigger("ShowVideo");
            animLogo.SetTrigger("HideVideo");
            bShowVideo = false;
            timeLastClick = Time.realtimeSinceStartup;
        }
        else {
            Debug.Log("Tocado logo");
            animLogo.ResetTrigger("HideVideo");
            animLogo.SetTrigger("ShowVideo");
            vp.loopPointReached += EndVideo;
            vp.isLooping = true;
            vp.Play();
            bShowVideo = true;
            timeLastClick = Time.realtimeSinceStartup;
            //cDataManager.AddResponse(eDataSesionAction.VIDEO, "");
        }
    }
    public void SetVideo(VideoClip vc) {
        vp.clip = vc;
    }

    private void EndVideo(VideoPlayer vp) {
        vp.loopPointReached -= EndVideo;
        vp.Stop();
        animLogo.ResetTrigger("ShowVideo");
        animLogo.SetTrigger("HideVideo");
        bShowVideo = false;
    }

}
