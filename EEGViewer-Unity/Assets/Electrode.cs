using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Electrode : MonoBehaviour
{
    public float distanceFromSkull = 0.1f;
    public GameObject fx;

    private GameObject brainCenter;

    // Update is called once per frame
    void Update()
    {
        if (!fx)
            return;

        brainCenter = GameObject.FindGameObjectWithTag("BrainCenter");

        if (!brainCenter)
            return;

        RaycastHit hit;
        Vector3 rayDir = brainCenter.transform.position - transform.position;
        if (Physics.Raycast(transform.position, rayDir.normalized, out hit))
        {
            fx.transform.position = hit.point + hit.normal * distanceFromSkull;
        }
   
    }
}
