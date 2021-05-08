using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostFXRenderer : MonoBehaviour
{
    public Material mat;
    public Color color;

    public Color[] LutINColors;
    public Color[] LutOUTColors;

    private Texture2D LutIN;
    private Texture2D LutOUT;

    public float CurrentTime = 0;
    public float MaxTime = 2;

    public Vector2 _position;

    public bool IsAnimating = false;

    // Start is called before the first frame update
    void Start()
    {
        if (LutINColors.Length != LutOUTColors.Length)
            Debug.LogError("[LUT] Missing colors !");

        LutIN = new Texture2D(LutINColors.Length, 1, TextureFormat.RGB24, false, false);
        LutIN.filterMode = FilterMode.Point;
        LutIN.SetPixels(LutINColors);
        LutIN.Apply();
        LutOUT = new Texture2D(LutOUTColors.Length, 1, TextureFormat.RGB24, false, false);
        LutOUT.filterMode = FilterMode.Point;
        LutOUT.SetPixels(LutOUTColors);
        LutOUT.Apply();
    }
    
    public void StartAnimation( Vector2 position )
    {
        _position = position;
        CurrentTime = 0;
        IsAnimating = true;
    }

    // Update is called once per frame
    void Update()
    {
        CurrentTime += Time.deltaTime;
        if (CurrentTime > MaxTime) IsAnimating = false;
        if(Input.GetKeyDown(KeyCode.N)) CurrentTime = 0;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (IsAnimating)
        {
            mat.SetColor("_Color", color);
            mat.SetVector("_PlayerPosition", GetComponent<Camera>().WorldToViewportPoint(_position));
            mat.SetFloat("_AnimationTime", CurrentTime);
            mat.SetTexture("_LUT_IN", LutIN);
            mat.SetTexture("_LUT_OUT", LutOUT);
            source.filterMode = FilterMode.Point;
        }
        else
        {
            // NOTe (Toffa): quick hack to go end of the animation therefor everything is colored
            // TODO(Toffa): fix shader to be able to set an animation percentage, maybe even to set animation values here
            mat.SetFloat("_AnimationTime", 10000);

        }

            Graphics.Blit(source, destination, mat);
    }
}
