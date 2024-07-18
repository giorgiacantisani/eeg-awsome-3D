using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls : MonoBehaviour
{
    public LSLInletReader lsl;

    public Renderer[] head;
    public GameObject[] setup1;
    public GameObject[] setup2;
    public GameObject[] setup3;
    public GameObject[] setup4;
    public GameObject[] setup5;
    public GameObject[] setup6;
    public GameObject[] setup7;

    public Camera[] camerasWithIndicatorsOffByDefault;

    public DisplayEEG displayEEG;
    public RenderTexture eegDisplayRT;
    public RenderTexture electrodeDisplayRT;
    public RenderTexture fftRT;

    float headCutoff = -7.0f;


    // Start is called before the first frame update
    void Start()
    {
        if (head.Length > 0)
            headCutoff = head[0].sharedMaterial.GetFloat("_Cutoff");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("up"))
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            headCutoff -= .1f;
            foreach (var r in head)
            {
                r.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat("_Cutoff", headCutoff);
                r.SetPropertyBlock(propertyBlock);
            }
        }
        if (Input.GetKey("down"))
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            headCutoff += .1f;
            foreach (var r in head)
            {
                r.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat("_Cutoff", headCutoff);
                r.SetPropertyBlock(propertyBlock);
            }
        }

        if (Input.GetKeyUp("space"))
        {
            foreach (var c in camerasWithIndicatorsOffByDefault)
                c.cullingMask ^= 1 << 1;
        }

        if (Input.GetKeyUp("z"))
            displayEEG.eeg = eegDisplayRT;
        if (Input.GetKeyUp("x"))
            displayEEG.eeg = electrodeDisplayRT;
        if (Input.GetKeyUp("c"))
            displayEEG.eeg = fftRT;

        if (Input.GetKeyUp("1"))
        {
            ActivateSetup(0);
        }
        else if (Input.GetKeyUp("2"))
        {
            ActivateSetup(1);
        }
        else if (Input.GetKeyUp("3"))
        {
            ActivateSetup(2);
        }
        else if (Input.GetKeyUp("4"))
        {
            ActivateSetup(3);
        }
        else if (Input.GetKeyUp("5"))
        {
            ActivateSetup(4);
        }
        else if (Input.GetKeyUp("6"))
        {
            ActivateSetup(5);
        }
        else if (Input.GetKeyUp("7"))
        {
            ActivateSetup(6);
        }

        if (Input.GetKeyUp("5"))
        {
            lsl.computeFourier = false;
            lsl.shortFourier = true;
        }
        else if (Input.GetKeyUp("6"))
        {
            lsl.computeFourier = true;
            lsl.shortFourier = false;
            displayEEG.eeg = electrodeDisplayRT;
        }
    }

    void ActivateSetup(int index)
    {
        var list = new[] {setup1, setup2, setup3, setup4, setup5, setup6, setup7};
        for (var i = 0; i < list.Length; ++i)
            if (i != index)
                foreach (var go in list[i]) go.SetActive(false);
        foreach (var go in list[index]) go.SetActive(true);
   }
}