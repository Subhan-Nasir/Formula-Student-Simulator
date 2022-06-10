using UnityEngine;
using System.Collections;

// Used to stop Unity from automatically switching to game view when you press play.
// When the empty game object this script is attached to is active, Unity will stay in Scene view when you press play.

public class KeepSceneAlive : MonoBehaviour
{
    public bool KeepSceneViewActive;

    void Start()
    {
        if (this.KeepSceneViewActive && Application.isEditor)
        {
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        }
    }
}