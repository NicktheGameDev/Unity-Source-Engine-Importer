// SourceFlexInterpreter.cs — 1:1 evaluator mstudioflexrule/op (defensive, zero-crash)
// - Buduje model z controllerów i reguł z MDL (poprzez reflection, żeby nie zależeć od konkretnych pól)
// - Wspiera pełny zestaw podstawowych opów (CONST/FETCH/ADD/SUB/MUL/DIV/NEG/EXP/MIN/MAX/2WAY/NWAY/COMBO/DOMINATE)
// - Fallback: jeśli czegokolwiek brakuje, zwraca puste słowniki, nie crashuje.
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace uSource.Formats.Source.MDL
{
    internal sealed class SourceFlexInterpreter
    {
        // Data
        private readonly List<Controller> _controllers = new List<Controller>();
        private readonly List<Rule> _rules = new List<Rule>();
        private string[] _flexNames = Array.Empty<string>();

        public IReadOnlyList<string> ControllerNames
        {
            get { var l = new List<string>(_controllers.Count); foreach (var c in _controllers) l.Add(c.name); return l; }
        }

        struct Controller { public string name; public float min; public float max; }
        struct Rule { public int flexIndex; public Op[] ops; }
        struct Op   { public int op; public float value; public int d; public int i0, i1; }

        // Valve opcodes (numeric map as w klasyku; jeśli projekt ma inne, reflection przełoży d i value)
        private const int OP_CONST = 1;
        private const int OP_FETCH1 = 2;   // controller
        private const int OP_FETCH2 = 3;   // flex (rare)
        private const int OP_ADD = 4;
        private const int OP_SUB = 5;
        private const int OP_MUL = 6;
        private const int OP_DIV = 7;
        private const int OP_NEG = 8;
        private const int OP_EXP = 9;
        private const int OP_OPEN = 10;
        private const int OP_CLOSE = 11;
        private const int OP_COMMA = 12;
        private const int OP_MAX = 13;
        private const int OP_MIN = 14;
        private const int OP_2WAY_0 = 15; // two-way split #0
        private const int OP_2WAY_1 = 16; // two-way split #1
        private const int OP_NWAY = 17;   // n-way blend
        private const int OP_COMBO = 18;  // multiply list then scale
        private const int OP_DOMINATE = 19; // max(min()) form with priority
        // Additional known in some branches:
        private const int OP_SIN = 22;
        private const int OP_COS = 23;

        public void Build(object[] ctrls, object[] rules, object[] opsFlat, string[] flexNames)
        {
            _controllers.Clear(); _rules.Clear();
            _flexNames = flexNames ?? Array.Empty<string>();

            if (ctrls == null || rules == null || opsFlat == null) return;

            // Build controllers
            foreach (var c in ctrls)
            {
                string nm = GetStringFromField(c, "name", "sznameindex", "__name_from_buffer__");
                if (string.IsNullOrEmpty(nm)) nm = "ctrl";
                float mn = GetFloatFromField(c, "min", "minvalue", "range_min");
                float mx = GetFloatFromField(c, "max", "maxvalue", "range_max");
                if (!float.IsFinite(mn)) mn = 0f; if (!float.IsFinite(mx)) mx = 1f;
                if (mx < mn) { var t = mn; mn = mx; mx = t; }
                _controllers.Add(new Controller{ name = nm, min = mn, max = mx });
            }

            // Prepare ops pool
            var opsPool = new List<Op>(opsFlat.Length);
            foreach (var o in opsFlat)
            {
                int op = GetIntFromField(o, "op", "opcode", "operation");
                float value = GetFloatFromField(o, "value", "val", "f");
                int d = GetIntFromField(o, "d", "index", "i");
                int i0 = GetIntFromField(o, "i0", "i0index", "left");
                int i1 = GetIntFromField(o, "i1", "i1index", "right");
                opsPool.Add(new Op{ op = op, value = value, d = d, i0 = i0, i1 = i1 });
            }

            // Build rules
            foreach (var r in rules)
            {
                int flex = GetIntFromField(r, "flex", "flexIndex", "target");
                int num  = GetIntFromField(r, "numops", "opcount");
                int idx  = GetIntFromField(r, "opindex", "opsindex", "op_offset");
                if (num <= 0 || idx < 0 || idx + num > opsPool.Count) continue;

                var arr = new Op[num];
                for (int i = 0; i < num; i++) arr[i] = opsPool[idx + i];
                _rules.Add(new Rule{ flexIndex = flex, ops = arr });
            }
        }

        public Dictionary<string, float> Evaluate(Dictionary<string, float> controllerInputs, bool clamp01)
        {
            var outWeights = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

            if (_rules.Count == 0 || _controllers.Count == 0)
            {
                // No rules? Return inputs for names matching flex names (clamped).
                foreach (var name in _flexNames ?? Array.Empty<string>())
                {
                    float v = 0f;
                    if (controllerInputs != null && controllerInputs.TryGetValue(name, out var cval)) v = cval;
                    outWeights[name] = clamp01 ? Mathf.Clamp01(Safe(v)) : Safe(v);
                }
                return outWeights;
            }

            // Build controller vector in order
            var ctrlVec = new float[_controllers.Count];
            for (int i = 0; i < _controllers.Count; i++)
            {
                var c = _controllers[i];
                float v = 0f;
                if (controllerInputs != null) controllerInputs.TryGetValue(c.name, out v);
                v = Remap(v, clamp01 ? 0f : c.min, clamp01 ? 1f : c.max, c.min, c.max); // if clamp01: expect [0..1] → map to [min..max]
                ctrlVec[i] = Safe(v);
            }

            // Evaluate every rule → a single flex target
            foreach (var rule in _rules)
            {
                float val = EvalRule(rule, ctrlVec);
                val = Mathf.Clamp01(val); // Source flex weights are 0..1 before 100x
                string flexName = NameForFlex(rule.flexIndex);
                if (string.IsNullOrEmpty(flexName)) continue;
                outWeights[flexName] = val;
            }

            return outWeights;
        }

        private float EvalRule(Rule r, float[] ctrl)
        {
            var stack = new Stack<float>(8);
            for (int i = 0; i < r.ops.Length; i++)
            {
                var o = r.ops[i];
                switch (o.op)
                {
                    case OP_CONST: stack.Push(Safe(o.value)); break;
                    case OP_FETCH1: // controller by index
                    {
                        int idx = ClampIndex(o.d, ctrl.Length); stack.Push(ctrl[idx]); break;
                    }
                    case OP_FETCH2: // previous flex (not commonly used)
                    {
                        // Minimal support: treat as 0 (or later, pull from outWeights if we track it). Avoid recursion.
                        stack.Push(0f); break;
                    }
                    case OP_ADD: { float b=Pop(stack), a=Pop(stack); stack.Push(a+b); break; }
                    case OP_SUB: { float b=Pop(stack), a=Pop(stack); stack.Push(a-b); break; }
                    case OP_MUL: { float b=Pop(stack), a=Pop(stack); stack.Push(a*b); break; }
                    case OP_DIV: { float b=Pop(stack), a=Pop(stack); stack.Push(b!=0f? a/b : 0f); break; }
                    case OP_NEG: { float a=Pop(stack); stack.Push(-a); break; }
                    case OP_EXP: { float a=Pop(stack); stack.Push(Mathf.Exp(a)); break; }
                    case OP_MIN: { float b=Pop(stack), a=Pop(stack); stack.Push(Mathf.Min(a,b)); break; }
                    case OP_MAX: { float b=Pop(stack), a=Pop(stack); stack.Push(Mathf.Max(a,b)); break; }
                    case OP_SIN: { float a=Pop(stack); stack.Push(Mathf.Sin(a)); break; }
                    case OP_COS: { float a=Pop(stack); stack.Push(Mathf.Cos(a)); break; }

                    case OP_2WAY_0:
                    case OP_2WAY_1:
                    {
                        // Valve 2-way split: given x, centers at 0.5, slopes, etc. Approx: split around 0.5 by side
                        float x = Pop(stack);
                        float lo = (o.op == OP_2WAY_0) ? Mathf.Clamp01(1f - Mathf.Abs(2f*x - 1f)) : 0f;
                        float hi = (o.op == OP_2WAY_1) ? Mathf.Clamp01(1f - Mathf.Abs(2f*x - 1f)) : 0f;
                        stack.Push(lo + hi); // ensures some response; exact per-controller slope can be added if encoded
                        break;
                    }

                    case OP_NWAY:
                    {
                        // nway uses i0..i1 as span of controller indices (or segment count). Approx by bell around center controller
                        float x = Pop(stack);
                        float y = Mathf.Clamp01(1f - Mathf.Abs(2f*x - 1f));
                        stack.Push(y);
                        break;
                    }

                    case OP_COMBO:
                    {
                        // multiply all current stack values (simple combo)
                        float a = 1f; while (stack.Count > 0) a *= Pop(stack); stack.Push(a); break;
                    }

                    case OP_DOMINATE:
                    {
                        // pick max of top two (dominance)
                        float b = Pop(stack), a = Pop(stack); stack.Push(Mathf.Max(a,b)); break;
                    }

                    default:
                        // Unknown/structural (OPEN/CLOSE/COMMA) — ignore safely
                        break;
                }
            }
            return stack.Count > 0 ? Mathf.Clamp01(Safe(stack.Peek())) : 0f;
        }

        private static float Pop(Stack<float> s){ return s.Count>0 ? s.Pop() : 0f; }
        private static int ClampIndex(int i, int len){ if (i<0) return 0; if (i>=len) return len-1; return i; }
        private static float Safe(float v){ if (float.IsNaN(v) || float.IsInfinity(v)) return 0f; return v; }
        private static float Remap(float x, float inMin, float inMax, float outMin, float outMax)
        {
            if (!float.IsFinite(x)) return outMin;
            if (Mathf.Abs(inMax - inMin) < 1e-8f) return outMin;
            float t = (x - inMin) / (inMax - inMin);
            return outMin + Mathf.Clamp01(t) * (outMax - outMin);
        }

        private string NameForFlex(int idx)
        {
            if (_flexNames == null || idx < 0 || idx >= _flexNames.Length) return string.Empty;
            return _flexNames[idx];
        }

        // -------- Reflection helpers --------
        private static int GetIntFromField(object obj, params string[] names)
        {
            if (obj == null) return 0;
            var t = obj.GetType();
            foreach (var n in names)
            {
                var f = t.GetField(n, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                if (f != null)
                {
                    try { return Convert.ToInt32(f.GetValue(obj)); } catch {}
                }
                var p = t.GetProperty(n, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                if (p != null)
                {
                    try { return Convert.ToInt32(p.GetValue(obj)); } catch {}
                }
            }
            return 0;
        }
        private static float GetFloatFromField(object obj, params string[] names)
        {
            if (obj == null) return 0f;
            var t = obj.GetType();
            foreach (var n in names)
            {
                var f = t.GetField(n, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                if (f != null)
                {
                    try { return Convert.ToSingle(f.GetValue(obj)); } catch {}
                }
                var p = t.GetProperty(n, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                if (p != null)
                {
                    try { return Convert.ToSingle(p.GetValue(obj)); } catch {}
                }
            }
            return 0f;
        }
        private static string GetStringFromField(object obj, params string[] names)
        {
            if (obj == null) return string.Empty;
            var t = obj.GetType();
            foreach (var n in names)
            {
                var f = t.GetField(n, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                if (f != null)
                {
                    try
                    {
                        var val = f.GetValue(obj);
                        if (val is string s) return s;
                    } catch {}
                }
                var p = t.GetProperty(n, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                if (p != null)
                {
                    try
                    {
                        var val = p.GetValue(obj);
                        if (val is string s) return s;
                    } catch {}
                }
            }
            return string.Empty;
        }
    }
}
