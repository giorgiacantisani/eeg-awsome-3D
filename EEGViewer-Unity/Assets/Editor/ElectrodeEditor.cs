using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
[CustomEditor(typeof(Electrode))]
[CanEditMultipleObjects]
public class ElectrodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        foreach (Electrode electrode in targets)
        {
            electrode.distanceFromSkull = EditorGUILayout.Slider("Distance from Skull", electrode.distanceFromSkull, 0.0f, 1.0f);
            // electrode.brainCenter = (GameObject)EditorGUILayout.ObjectField("Brain Center", electrode.brainCenter, typeof(GameObject), true);
            electrode.fx = (GameObject)EditorGUILayout.ObjectField("FX object", electrode.fx, typeof(GameObject), true);
        }
    }
}

