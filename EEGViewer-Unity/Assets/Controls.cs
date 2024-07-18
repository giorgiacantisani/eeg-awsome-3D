using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls : MonoBehaviour
{
    public Renderer[] head;
    public GameObject[] setup1;
    public GameObject[] setup2;
    public GameObject[] setup3;
    public GameObject[] setup4;
    public GameObject[] setup5;

    public Camera[] camerasWithIndicatorsOffByDefault;
    
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
    }

    void ActivateSetup(int index)
    {
        var list = new[] {setup1, setup2, setup3, setup4, setup5};
        for (var i = 0; i < list.Length; ++i)
            if (i != index)
                foreach (var go in list[i]) go.SetActive(false);
        foreach (var go in list[index]) go.SetActive(true);
   }
}