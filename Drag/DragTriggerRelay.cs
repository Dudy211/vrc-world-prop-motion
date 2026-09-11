using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class DragTriggerRelay : UdonSharpBehaviour
{
    public Drag drag;

    public override void Interact()
    {
        if (drag != null)
        {
            drag.TryStartDrag();
        }
    }
}
