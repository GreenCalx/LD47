﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostFXRenderer : MonoBehaviour
{
    public Material mat;
    public Color color;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        mat.SetColor("_Color", color);
        Graphics.Blit(source, destination, mat);
    }
}
