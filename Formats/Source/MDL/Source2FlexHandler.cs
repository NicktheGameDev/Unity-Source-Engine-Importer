using UnityEngine;
using uSource.Formats.Source.MDL;


public class Source2FlexHandler : IFlexHandler
{
    public void ConvertFlexesToBlendShapes(MDLFile mdlFile)
    {
        Mesh mesh = new Mesh();
        foreach (var flex in mdlFile.MDL_Flexes)
        {
            //string blendShapeName = flex.GetFlexName(mdlFile.mdldata);
            //int index = flex.vertindex;
            //Vector3[] posDeltas = new Vector3[] { mdlFile.FlexVertices[index].PositionDelta };
           // Vector3[] normDeltas = new Vector3[] { mdlFile.FlexVertices[index].NormalDelta };
//Vector3[] tanDeltas = new Vector3[] { mdlFile.FlexVertices[index].TangentDelta };
         //   mesh.AddBlendShapeFrame(blendShapeName, 100f, posDeltas, normDeltas, tanDeltas);
        }
    }
}
