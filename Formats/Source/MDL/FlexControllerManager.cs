using System.Collections.Generic;
using UnityEngine;
using uSource.Formats.Source.MDL;

public class FlexControllerManager : MonoBehaviour
{
    public SkinnedMeshRenderer MeshRenderer;
    public List<VertexAnimation> VertexAnimations;
    public void ApplyFlexesToMesh(MDLFile mdlFile)
    {
        Mesh mesh = MeshRenderer.sharedMesh;

  
    }
}