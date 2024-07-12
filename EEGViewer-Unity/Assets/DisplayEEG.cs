using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DisplayEEG : MonoBehaviour
{

    public RenderTexture eeg;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(eeg, dest);
    }
}
