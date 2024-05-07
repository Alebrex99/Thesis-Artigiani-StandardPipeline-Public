using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{

    public bool fadeOnStart = true;
    public float fadeDuration = 2; //duarata in secondi
    public Color fadeColor;
   
    
    private Renderer rend;
    
    
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        //usato per effetto fade all'avvio
        if(fadeOnStart)
            FadeIn();
    }
    //FadeIn() + FadeOut(): funzioni usate tramite pulsante : quando clicchi il pulsante, il fade inizia per cambio scena
    public void FadeIn()
    {
        Fade(1, 0);
    }
    public void FadeOut()
    {
        Fade(0,1);
    }

    public void Fade(float alphaIn, float alphaOut)
    {
        
        StartCoroutine(FadeRoutine(alphaIn, alphaOut));
    }

    public IEnumerator FadeRoutine(float alphaIn, float alphaOut)
    {
        float timer = 0;
        while (timer <= fadeDuration)
        {
            //Meodologia iniziale per modificare l'alpha del materiale:
            Color newColor = fadeColor;
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);
            rend.material.SetColor("_Color", newColor);
            
            //FadeThis(rend, Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration));
            
            timer += Time.deltaTime;
            //wait until next frame
            yield return null;
        }
        Color newColor2 = fadeColor;
        newColor2.a = alphaOut;
        rend.material.SetColor("_Color", newColor2);
    }

    private void FadeThis(Renderer r, float inputAlpha)
    {
        Material mat = r.material;
        Material newMat = new Material(Shader.Find("Unlit/Unlit Transparent Color"));
        
 
        // Set surface type to Transparent
        mat.SetFloat("_Surface", 1.0f);
 
        // Set Blending Mode to Alpha
        mat.SetFloat("_Blend", 0.0f);    
 
        // Set alpha
        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, inputAlpha);  
               
        newMat = mat;
        r.material = newMat;
    }
    

    public IEnumerator FadeOutInRoutine()
    {
        float timer = 0;
        while (timer <= fadeDuration)
        {
            // Color newColor = fadeColor;
            // newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);
            // rend.material.SetColor("_Color", newColor);
            
            FadeThis(rend, Mathf.Lerp(0, 1, timer / fadeDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        while (timer <= fadeDuration)
        {
            // Color newColor = fadeColor;
            // newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);
            // rend.material.SetColor("_Color", newColor);
            
            FadeThis(rend, Mathf.Lerp(1, 0, timer / fadeDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        Color newColor2 = fadeColor;
        newColor2.a = 0;
        rend.material.SetColor("_Color", newColor2);
        
    }
}
