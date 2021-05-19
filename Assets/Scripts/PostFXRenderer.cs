using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostFXRenderer : MonoBehaviour
{
    public Material mat;
    public Color color;

    public Color[] LutINColors;
    // This is a flattened array of TimelineSize * LUTINsize
    public Color[] LutOUTColors;

    private Texture2D LutIN;
    private Texture2D LutOUTPost;
    private Texture2D LutOUTPre;

    public float CurrentTime = 0;
    public float MaxTime = 2;

    public Vector2 _position;

    public bool IsAnimating = false;

    // Start is called before the first frame update
    void Start()
    {
        if (LutOUTColors.Length % LutINColors.Length != 0)
            Debug.LogError("[LUT] Missing colors !");

        LutIN = new Texture2D(LutINColors.Length, 1, TextureFormat.RGB24, false, false);
        LutIN.filterMode = FilterMode.Point;
        LutIN.SetPixels(LutINColors);
        LutIN.Apply();

        //Put lut in as first index in out array, I know... This is dumb...
        Color[] Temp = new Color[LutOUTColors.Length + LutINColors.Length];
        LutINColors.CopyTo(Temp, 0);
        LutOUTColors.CopyTo(Temp, LutINColors.Length);
        LutOUTColors = Temp;
    }
    
    public void StartAnimation( Vector2 position, int TimelineIn, int TimelineOut )
    {
        _position = position;

        // TODO(toffa): COMMON ON DUDE PLEASE DO SOMETHING BETTER
        if (position != Vector2.zero)
        {
            //CurrentTime = 0;
        }
        IsAnimating = true;
        int Size = LutINColors.Length;

        LutOUTPost = new Texture2D(Size, 1, TextureFormat.RGB24, false, false);
        LutOUTPost.filterMode = FilterMode.Point;
        Color[] Pixels = new Color[Size]; 
        System.Array.Copy(LutOUTColors, TimelineOut*Size, Pixels, 0, Size);
        LutOUTPost.SetPixels(Pixels);
        LutOUTPost.Apply();

        LutOUTPre = new Texture2D(Size, 1, TextureFormat.RGB24, false, false);
        LutOUTPre.filterMode = FilterMode.Point;
        System.Array.Copy(LutOUTColors, TimelineIn*Size, Pixels, 0, Size);
        LutOUTPre.SetPixels(Pixels);
        LutOUTPre.Apply();
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
            mat.SetTexture("_LUT_OUT_PRERIPPLE", LutOUTPre);
            mat.SetTexture("_LUT_OUT_POSTRIPPLE", LutOUTPost);
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
