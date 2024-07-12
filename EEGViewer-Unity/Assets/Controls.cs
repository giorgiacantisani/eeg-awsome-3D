using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls : MonoBehaviour
{
    public Renderer[] head;
    public GameObject[] setup1;
    public GameObject[] setup2;
    
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

        if (Input.GetKeyUp("1"))
        {
            foreach (var go in setup1)
                go.SetActive(true);
            foreach (var go in setup2)
                go.SetActive(false);
        }
        else if (Input.GetKeyUp("2"))
        {
            foreach (var go in setup1)
                go.SetActive(false);
            foreach (var go in setup2)
                go.SetActive(true);
        }
    }
}