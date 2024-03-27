using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiSceneLauncher : MonoBehaviour
{
    [SerializeField] private string _firstSceneToLoad;
    [SerializeField] private OVRCameraRig _ovrCameraRig;
    // Start is called before the first frame update
    void Start()
    {       
        Scene firstScene = SceneManager.GetSceneByName(_firstSceneToLoad);
        if (firstScene.IsValid() && firstScene.isLoaded)
            return;

        if (_ovrCameraRig != null)
            _ovrCameraRig.gameObject.SetActive(false);
        StartCoroutine(LoadFirstScene(_firstSceneToLoad));
        
    }

    private IEnumerator LoadFirstScene(string sceneName)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOperation.isDone)
            yield return null;

        if (_ovrCameraRig != null)
            _ovrCameraRig.gameObject.SetActive(true);
    }
}
