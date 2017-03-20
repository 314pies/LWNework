using UnityEngine;
using System.Collections;

public delegate void OnViewerUpdate();

[ExecuteInEditMode]
public class EditorTTEst : MonoBehaviour
{
    /// <summary>
    /// The viewID of this LWView
    /// </summary>
    public int ViewID;

#if UNITY_EDITOR
    static OnViewerUpdate onViewerUpdate = new OnViewerUpdate(DelegateInvoked);
    static void DelegateInvoked()
    {
        Debug.Log("onViewerUpdate is invoked. ");
    }
#endif
    // Use this for initialization
    void Start()
    {
#if UNITY_EDITOR
        SignViewerID();
        onViewerUpdate += SignViewerID;
#endif
    }

    void OnDestroy()
    {
#if UNITY_EDITOR
        onViewerUpdate -= SignViewerID;
#endif
    }

#if UNITY_EDITOR
    static int LastestViewerID = 0;
#endif

    void SignViewerID()
    {
#if UNITY_EDITOR
        ViewID = LastestViewerID;
        LastestViewerID++;
        Debug.Log("ViewID signed: " + ViewID);
#endif
    }

    /// <summary>
    /// Reset is called when the user hits the Reset button in the Inspector's context menu or when adding the component the first time.
    /// </summary>
    void Reset()
    {
#if UNITY_EDITOR
        Debug.Log("Reset - A___A");
        LastestViewerID = 0;
        onViewerUpdate();
#endif
    }
}
