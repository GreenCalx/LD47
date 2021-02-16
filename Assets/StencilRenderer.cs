using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StencilRenderer : MonoBehaviour
{
    public Material StencilMat;

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
        StencilMat.SetTexture("_MainTex", source);
        RenderTexture Temp = RenderTexture.GetTemporary(source.descriptor);
        Graphics.SetRenderTarget(Temp.colorBuffer, source.depthBuffer);
        GL.PushMatrix();
        GL.LoadOrtho();

        StencilMat.SetPass(0);

        GL.Begin(GL.QUADS);
        GL.TexCoord2(0f, 0f); GL.Vertex3(0f, 0f, 0f);    /* note the order! */
        GL.TexCoord2(0f, 1f); GL.Vertex3(0f, 1f, 0f);    /* also, we need TexCoord2 */
        GL.TexCoord2(1f, 1f); GL.Vertex3(1f, 1f, 0f);
        GL.TexCoord2(1f, 0f); GL.Vertex3(1f, 0f, 0f);
        GL.End();
      
        GL.PopMatrix();

        Graphics.Blit(Temp, (RenderTexture)null);

/*
        RenderTexture Temp = RenderTexture.GetTemporary(source.descriptor);

        Graphics.Blit(source, Temp, StencilMat);  
        Graphics.Blit(Temp, destination);
        */

        RenderTexture.ReleaseTemporary(Temp);

    }

}