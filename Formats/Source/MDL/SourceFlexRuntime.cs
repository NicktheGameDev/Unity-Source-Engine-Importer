// SourceFlexRuntime.cs — runtime driver
// - Jeśli flex rules/controllers obecne → używa pełnego interpretera (SourceFlexInterpreter)
// - W innym razie: mapowanie 1:1 (kontroler/parametr == blendshape)
// - Zero crash, multi-renderer support, Animator floats opcjonalnie
using System;
using System.Collections.Generic;
using UnityEngine;

namespace uSource.Formats.Source.MDL
{
    [DisallowMultipleComponent]
    public sealed class SourceFlexRuntime : MonoBehaviour
    {
        [Tooltip("Auto-bind Animator float params to controller inputs (same names).")]
        public bool BindAnimatorParameters = true;

        [Tooltip("Clamp controller inputs to [0,1].")]
        public bool Clamp01 = true;

        private readonly Dictionary<string, float> _ctrl = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        private SkinnedMeshRenderer[] _renderers;
        private Dictionary<SkinnedMeshRenderer, Dictionary<string, int>> _shapeIndex
            = new Dictionary<SkinnedMeshRenderer, Dictionary<string, int>>();
        private Animator _anim;

        // Flex interpreter (if rules present)
        private SourceFlexInterpreter _interp;
        private string[] _flexNames;

        public void SafeInitFromFlexNames(string[] names)
        {
            try
            {
                _renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
                _anim = GetComponent<Animator>();

                _shapeIndex.Clear();
                foreach (var r in _renderers)
                {
                    var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    int c = r.sharedMesh != null ? r.sharedMesh.blendShapeCount : 0;
                    for (int i = 0; i < c; i++)
                    {
                        string nm = r.sharedMesh.GetBlendShapeName(i);
                        if (!dict.ContainsKey(nm))
                            dict[nm] = i;
                        if (!_ctrl.ContainsKey(nm))
                            _ctrl[nm] = 0f;
                    }
                    _shapeIndex[r] = dict;
                }

                _flexNames = names ?? Array.Empty<string>();
                if (names != null)
                    foreach (var nm in names)
                        if (!string.IsNullOrEmpty(nm) && !_ctrl.ContainsKey(nm))
                            _ctrl[nm] = 0f;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SourceFlexRuntime] Init warning: {e.Message}");
            }
        }

        public void InjectFlexRuleData(object[] controllers, object[] rules, object[] opsFlat, string[] flexNames)
        {
            try
            {
                if (controllers != null && controllers.Length > 0 && rules != null && rules.Length > 0 && opsFlat != null && opsFlat.Length > 0)
                {
                    _interp = new SourceFlexInterpreter();
                    _interp.Build(controllers, rules, opsFlat, flexNames);
                    // Ensure we have ctrl entries for every controller name
                    foreach (var cname in _interp.ControllerNames)
                        if (!_ctrl.ContainsKey(cname)) _ctrl[cname] = 0f;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SourceFlexRuntime] Flex interpreter init failed: " + e.Message);
                _interp = null;
            }
        }

        public void SetController(string name, float value)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (!_ctrl.ContainsKey(name)) _ctrl[name] = 0f;
            _ctrl[name] = Sanitize(value);
        }

        public float GetController(string name)
        {
            if (string.IsNullOrEmpty(name)) return 0f;
            return _ctrl.TryGetValue(name, out var v) ? v : 0f;
        }

        void Update()
        {
            // Pull Animator floats
            if (BindAnimatorParameters && _anim != null && _anim.runtimeAnimatorController != null)
            {
                foreach (var p in _anim.parameters)
                {
                    if (p.type != AnimatorControllerParameterType.Float) continue;
                    _ctrl[p.name] = Sanitize(_anim.GetFloat(p.name));
                }
            }

            if (_renderers == null || _renderers.Length == 0) return;

            if (_interp != null)
            {
                // Evaluate rules → flex weights dictionary (name -> 0..1)
                var weights01 = _interp.Evaluate(_ctrl, Clamp01);
                ApplyWeights(weights01);
            }
            else
            {
                // Direct mapping: controller name == blendshape name
                ApplyWeights(_ctrl);
            }
        }

        private void ApplyWeights(Dictionary<string, float> weights01)
        {
            foreach (var r in _renderers)
            {
                if (r == null || r.sharedMesh == null) continue;
                if (!_shapeIndex.TryGetValue(r, out var map)) continue;

                foreach (var kv in map)
                {
                    string shape = kv.Key;
                    int idx = kv.Value;
                    float w01 = weights01.TryGetValue(shape, out var v) ? v : 0f;
                    float w = Mathf.Clamp01(w01) * 100f;
                    try { r.SetBlendShapeWeight(idx, w); } catch {}
                }
            }
        }

        private float Sanitize(float x)
        {
            if (float.IsNaN(x) || float.IsInfinity(x)) return 0f;
            return Clamp01 ? Mathf.Clamp01(x) : x;
        }
    }
}
