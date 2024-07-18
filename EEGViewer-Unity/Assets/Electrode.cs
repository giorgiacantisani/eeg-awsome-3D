using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Electrode : MonoBehaviour
{
    public float distanceFromSkull = 0.1f;
    public GameObject fx;
    public GameObject indicator;

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

            if (indicator)
            {
                indicator.transform.localRotation = Quaternion.identity;
                var scale = transform.lossyScale;
                indicator.transform.localScale = new Vector3(1.0f/scale.x, 1.0f/scale.y, 1.0f/scale.z);
                foreach (Transform child in indicator.transform)
                {
                    child.LookAt(hit.point + hit.normal * distanceFromSkull * 100.0f);
                    child.transform.localRotation *= Quaternion.Euler(90, 0, 0);
                }
            }
        }
    }
}
