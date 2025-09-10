
using System;
using UnityEngine;
namespace uSource.Formats.Source.MDL
{
    [DisallowMultipleComponent]
    public sealed class MdlAnimationEventReceiver : MonoBehaviour
    {
        /// <summary>Raised when an MDL sequence event fires from an AnimationClip.</summary>
        public event Action<int,string> OnMdlEventFired;

        /// <summary>Called via AnimationEvent with functionName = "OnMdlEvent".</summary>
        public void OnMdlEvent(string payload)
        {
            // Unity AnimationEvent only passes string/float/int; type is attached via intParameter by our importer.
            // Fetch current clip info not available here; we only propagate payload and type.
            OnMdlEventFired?.Invoke(_lastType, payload);
        }

        // This will be set by the importer just before playback – but since AnimationEvent can't pass two values at once,
        // we keep it updated through the Animator so receivers can read it when the event fires.
        internal int _lastType;
    }
}
