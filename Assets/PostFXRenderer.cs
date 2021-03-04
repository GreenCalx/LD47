using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostFXRenderer : MonoBehaviour
{
    public Material mat;
    public Color color;

    public float CurrentTime = 0;

    public Vector2 _position;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public void StartAnimation( Vector2 position )
    {
        _position = position;
        CurrentTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        CurrentTime += Time.deltaTime;
        if(Input.GetKeyDown(KeyCode.N)) CurrentTime = 0;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        mat.SetColor("_Color", color);
        mat.SetVector("_PlayerPosition", GetComponent<Camera>().WorldToViewportPoint(_position));
        mat.SetFloat("_AnimationTime", CurrentTime);

        source.filterMode = FilterMode.Point;
    
        Graphics.Blit(source, destination, mat);
    }
}
