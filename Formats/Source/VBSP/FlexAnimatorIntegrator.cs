using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using USource.Model.Flex;          // namespace for VtaSeparatorSmooth and FlexFrame definitions
using uSource.Formats.Source.MDL;  // dla MDLFileHolder i innych, jeśli potrzebne

/// <summary>
/// Pomocnik do odtwarzania sekwencji vertex animation (vertanim) z VTA dla phonemów.
/// Zawiera coroutine oraz statyczną metodę event handlera.
/// </summary>
public class PhonemeRunner : MonoBehaviour
{
    /// <summary>
    /// Odtwarza kolejne klatki vertanim, aplikując delty bezpośrednio na wierzchołkach.
    /// </summary>
    /// <param name="root">Root GameObject zawierający SkinnedMeshRenderer/y</param>
    /// <param name="frames">Lista klatek VertAnim dla danego phonemu</param>
    public IEnumerator PlayVisemeSequence(GameObject root, List<VtaSeparatorSmooth.FlexFrame> frames)
    {
        // Załóżmy 30 FPS (możesz to dostosować)
        float frameDelay = 1f / 30f;

        foreach (var frame in frames)
        {
            foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Mesh mesh = smr.sharedMesh;  // instancja mesh
                Vector3[] verts = mesh.vertices;
                int[] indices = frame.vertexIndices;
                Vector3[] deltas = frame.deltas;

                for (int i = 0; i < indices.Length; i++)
                {
                    int vi = indices[i];
                    if (vi >= 0 && vi < verts.Length)
                        verts[vi] += deltas[i];
                }

                mesh.vertices = verts;
                mesh.RecalculateNormals();
                smr.sharedMesh = mesh;
            }
            yield return new WaitForSeconds(frameDelay);
        }
        Destroy(this);
    }

    /// <summary>
    /// Statyczna metoda event handlera, którą można podłączyć,
    /// aby automatycznie uruchomić vertanim po otrzymaniu listy klatek.
    /// </summary>
    /// <param name="root">Root GameObject zawierający PhonemeRunner</param>
    /// <param name="frames">Lista klatek VertAnim</param>
    public static void OnPhonemeEvent(GameObject root, List<VtaSeparatorSmooth.FlexFrame> frames)
    {
        var runner = root.GetComponent<PhonemeRunner>() ?? root.AddComponent<PhonemeRunner>();
        runner.StartCoroutine(runner.PlayVisemeSequence(root, frames));
    }
}
