using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SourceIO/Qc Flex Data")]
public class QcFlexData : ScriptableObject
{
    [Header("Global FPS (from $fps)")]
    public float fps = 30f;

    [Header("All $flexcontroller names (min/max)")]
    public List<FlexController> controllers = new List<FlexController>();

    [Header("All $flex names in this .qc")]
    public List<string> flexNames = new List<string>();

    [Serializable]
    public struct FlexController
    {
        public string name;
        public float min;
        public float max;
    }
    public struct FlexInfo
    {
        public string name;
        public float min;
        public float max;
        public float fps;
    }

    
}