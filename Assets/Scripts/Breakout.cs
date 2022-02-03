using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakout : MonoBehaviour
{
    private Texture2D mainTex;
    private MeshRenderer meshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        try {
            meshRenderer = GetComponent<MeshRenderer>();
            mainTex = new Texture2D(128, 128, TextureFormat.RGB24);
            meshRenderer.material.mainTexture = mainTex;
            Debug.Log(mainTex.GetType());           
        }
        catch(Exception e) {
            Debug.Log($"Breakout: unable to get all or part of MeshRenderer's main texture: {e.Message} ({e.GetType()})");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Color redColor = new Color(1f, 0.0f, 0.0f);
        Color greenColor = new Color(0.0f, 1f, 0.0f);
        Color blueColor = new Color(0.0f, 0.0f, 1f);
        var fillColorArray =  mainTex.GetPixels();
        
        for(var i = 0; i < fillColorArray.Length; ++i)
        {
            if (i % 3 == 0) fillColorArray[i] = redColor;
            if (i % 3 == 1) fillColorArray[i] = greenColor;
            if (i % 3 == 2) fillColorArray[i] = blueColor;
        }
        
        mainTex.SetPixels( fillColorArray );
        mainTex.Apply();
    }
}
