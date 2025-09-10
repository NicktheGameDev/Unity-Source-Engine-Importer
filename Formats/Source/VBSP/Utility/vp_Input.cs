using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_Input : MonoBehaviour
{
    [System.Serializable]
    public class vp_InputAxis
    {
        public KeyCode Positive;
        public KeyCode Negative;
    }

    public int ControlType = 0;

    // Primary buttons
    public Dictionary<string, KeyCode> Buttons = new Dictionary<string, KeyCode>();
    public List<string> ButtonKeys = new List<string>();
    public List<KeyCode> ButtonValues = new List<KeyCode>();

    // Secondary buttons
    public Dictionary<string, KeyCode> Buttons2 = new Dictionary<string, KeyCode>();
    public List<KeyCode> ButtonValues2 = new List<KeyCode>();

    // Axis
    public Dictionary<string, vp_InputAxis> Axis = new Dictionary<string, vp_InputAxis>();
    public List<string> AxisKeys = new List<string>();
    public List<vp_InputAxis> AxisValues = new List<vp_InputAxis>();

    // Unity Input Axis
    public List<string> UnityAxis = new List<string>();

    public static string FolderPath = "UFPS/Base/Content/Resources/Input";
    public static string PrefabPath = "Assets/UFPS/Base/Content/Resources/Input/vp_Input.prefab";

    public static bool mIsDirty = true;

    protected static vp_Input m_Instance;
    public static vp_Input Instance
    {
        get
        {
            if (mIsDirty)
            {
                mIsDirty = false;

                if (m_Instance == null)
                {
                    if (Application.isPlaying)
                    {
                        GameObject go = Resources.Load("Input/vp_Input") as GameObject;
                        if (go == null)
                        {
                            m_Instance = new GameObject("vp_Input").AddComponent<vp_Input>();
                        }
                        else
                        {
                            m_Instance = go.GetComponent<vp_Input>();
                            if (m_Instance == null)
                                m_Instance = go.AddComponent<vp_Input>();
                        }
                    }
                    m_Instance.SetupDefaults();
                }
            }
            return m_Instance;
        }
    }

    public static void CreateMissingInputPrefab(string prefabPath, string folderPath)
    {
#if UNITY_EDITOR
        GameObject go = UnityEditor.AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
        if (go == null)
        {
            if (!Application.isPlaying)
            {
                bool needsRefresh = false;
                string path = "";
                string[] folders = folderPath.Split(new string[1] { "/" }, System.StringSplitOptions.None);
                foreach (string folder in folders)
                {
                    path += "/";
                    if (!System.IO.Directory.Exists(Application.dataPath + path + folder))
                    {
                        needsRefresh = true;
                        System.IO.Directory.CreateDirectory(Application.dataPath + path + folder);
                    }
                    path += folder;
                }
                if (needsRefresh)
                    UnityEditor.AssetDatabase.Refresh();
            }

            go = new GameObject("vp_Input") as GameObject;
            go.AddComponent<vp_Input>();
#if UNITY_2018_3_OR_NEWER
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
#else
            UnityEditor.PrefabUtility.CreatePrefab(prefabPath, go);
#endif
            UnityEngine.Object.DestroyImmediate(go);
        }
        else
        {
            if (go.GetComponent<vp_Input>() == null)
                go.AddComponent<vp_Input>();
        }
#endif
    }

    protected virtual void Awake()
    {
        if (m_Instance == null)
            m_Instance = Instance;

        SetupDefaults();
    }

    public virtual void SetDirty(bool dirty)
    {
        mIsDirty = dirty;
    }

    public virtual void SetupDefaults(string type = "")
    {
        if (type == "" || type == "Buttons")
        {
            if (ButtonKeys.Count == 0)
            {
                AddButton("Attack", KeyCode.Mouse0);
                AddButton("SetNextWeapon", KeyCode.E);
                AddButton("SetPrevWeapon", KeyCode.Q);
                AddButton("ClearWeapon", KeyCode.Backspace);
                AddButton("Zoom", KeyCode.Mouse1);
                AddButton("Reload", KeyCode.R);
                AddButton("Jump", KeyCode.Space);
                AddButton("Crouch", KeyCode.C);
                AddButton("Run", KeyCode.LeftShift);
                AddButton("Interact", KeyCode.F);
                AddButton("Accept1", KeyCode.Return);
                AddButton("Accept2", KeyCode.KeypadEnter);
                AddButton("Pause", KeyCode.P);
                AddButton("Menu", KeyCode.Escape);
                AddButton("Toggle3rdPerson", KeyCode.V);
                AddButton("ScoreBoard", KeyCode.Tab);
                AddButton("SetWeapon1", KeyCode.Alpha1);
                AddButton("SetWeapon2", KeyCode.Alpha2);
                AddButton("SetWeapon3", KeyCode.Alpha3);
                AddButton("SetWeapon4", KeyCode.Alpha4);
                AddButton("SetWeapon5", KeyCode.Alpha5);
                AddButton("SetWeapon6", KeyCode.Alpha6);
                AddButton("SetWeapon7", KeyCode.Alpha7);
                AddButton("SetWeapon8", KeyCode.Alpha8);
                AddButton("SetWeapon9", KeyCode.Alpha9);
                AddButton("SetWeapon10", KeyCode.Alpha0);
                AddButton("Teleport", KeyCode.None);

                CreateMissingSecondaryButtons();
                AddSecondaryButton("Attack", KeyCode.JoystickButton5);
                AddSecondaryButton("SetNextWeapon", KeyCode.JoystickButton3);
                AddSecondaryButton("SetPrevWeapon", KeyCode.None);
                AddSecondaryButton("ClearWeapon", KeyCode.None);
                AddSecondaryButton("Zoom", KeyCode.JoystickButton4);
                AddSecondaryButton("Reload", KeyCode.JoystickButton2);
                AddSecondaryButton("Jump", KeyCode.JoystickButton0);
                AddSecondaryButton("Crouch", KeyCode.JoystickButton1);
                AddSecondaryButton("Run", KeyCode.JoystickButton8);
                AddSecondaryButton("Interact", KeyCode.JoystickButton2);
                AddSecondaryButton("Pause", KeyCode.P);
                AddSecondaryButton("Menu", KeyCode.JoystickButton6);
            }
        }

        if (type == "" || type == "Axis")
        {
            if (AxisKeys.Count == 0)
            {
                AddAxis("Vertical", KeyCode.W, KeyCode.S);
                AddAxis("Horizontal", KeyCode.D, KeyCode.A);
            }
        }

        if (type == "" || type == "UnityAxis")
        {
            if (UnityAxis.Count == 0)
            {
                AddUnityAxis("Mouse X");
                AddUnityAxis("Mouse Y");
                AddUnityAxis("Horizontal");
                AddUnityAxis("Vertical");
                AddUnityAxis("LeftTrigger");
                AddUnityAxis("RightTrigger");
            }
        }

        UpdateDictionaries();
    }

    public virtual void CreateMissingSecondaryButtons()
    {
        foreach (KeyValuePair<string, KeyCode> k in Buttons)
        {
            if (!Buttons2.ContainsKey(k.Key))
                AddSecondaryButton(k.Key, default(KeyCode));
        }
    }

    bool HaveBinding(string button)
    {
        if (Buttons.ContainsKey(button) || Buttons2.ContainsKey(button))
            return true;

        Debug.LogError("Error (" + this + ") \"" + button + "\" is not declared in the UFPS Input Manager.");
        return false;
    }

    public virtual void AddButton(string n, KeyCode k = KeyCode.None)
    {
        if (ButtonKeys.Contains(n))
            ButtonValues[ButtonKeys.IndexOf(n)] = k;
        else
        {
            ButtonKeys.Add(n);
            ButtonValues.Add(k);
        }
    }

    public virtual void AddSecondaryButton(string n, KeyCode k = KeyCode.None)
    {
        if (ButtonKeys.Contains(n))
        {
            try { ButtonValues2[ButtonKeys.IndexOf(n)] = k; }
            catch { ButtonValues2.Add(k); }
        }
        else
            ButtonValues2.Add(k);
    }

    public virtual void AddAxis(string n, KeyCode pk = KeyCode.None, KeyCode nk = KeyCode.None)
    {
        if (AxisKeys.Contains(n))
            AxisValues[AxisKeys.IndexOf(n)] = new vp_InputAxis { Positive = pk, Negative = nk };
        else
        {
            AxisKeys.Add(n);
            AxisValues.Add(new vp_InputAxis { Positive = pk, Negative = nk });
        }
    }

    public virtual void AddUnityAxis(string n)
    {
        if (!UnityAxis.Contains(n))
            UnityAxis.Add(n);
    }

    public virtual void UpdateDictionaries()
    {
        if (!Application.isPlaying)
            return;

        Buttons.Clear();
        for (int i = 0; i < ButtonKeys.Count; i++)
        {
            if (!Buttons.ContainsKey(ButtonKeys[i]))
                Buttons.Add(ButtonKeys[i], ButtonValues[i]);
        }

        Buttons2.Clear();
        for (int i = 0; i < ButtonKeys.Count; i++)
        {
            if (!Buttons2.ContainsKey(ButtonKeys[i]))
                Buttons2.Add(ButtonKeys[i], ButtonValues2[i]);
        }

        Axis.Clear();
        for (int i = 0; i < AxisKeys.Count; i++)
        {
            Axis.Add(AxisKeys[i], new vp_InputAxis { Positive = AxisValues[i].Positive, Negative = AxisValues[i].Negative });
        }

        CreateMissingSecondaryButtons();
    }

    public static bool GetButtonAny(string button) => Instance.DoGetButtonAny(button);
    public virtual bool DoGetButtonAny(string button)
    {
        if (!HaveBinding(button)) return false;
        return Input.GetKey(Buttons[button]) || Input.GetKeyDown(Buttons[button]) || Input.GetKeyUp(Buttons[button]) ||
               Input.GetKey(Buttons2[button]) || Input.GetKeyDown(Buttons2[button]) || Input.GetKeyUp(Buttons2[button]);
    }

    public static bool GetButton(string button) => Instance.DoGetButton(button);
    public virtual bool DoGetButton(string button)
    {
        if (!HaveBinding(button)) return false;
        return Input.GetKey(Buttons[button]) || Input.GetKey(Buttons2[button]);
    }

    public static bool GetButtonDown(string button) => Instance.DoGetButtonDown(button);
    public virtual bool DoGetButtonDown(string button)
    {
        if (!HaveBinding(button)) return false;
        return Input.GetKeyDown(Buttons[button]) || Input.GetKeyDown(Buttons2[button]);
    }

    public static bool GetButtonUp(string button) => Instance.DoGetButtonUp(button);
    public virtual bool DoGetButtonUp(string button)
    {
        if (!HaveBinding(button)) return false;
        return Input.GetKeyUp(Buttons[button]) || Input.GetKeyUp(Buttons2[button]);
    }

    public static float GetAxisRaw(string axis) => Instance.DoGetAxisRaw(axis);
    public virtual float DoGetAxisRaw(string axis)
    {
        if (Axis.ContainsKey(axis) && ControlType == 0)
        {
            float val = 0;
            if (Input.GetKey(Axis[axis].Positive)) val = 1;
            if (Input.GetKey(Axis[axis].Negative)) val = -1;
            return val;
        }
        else if (UnityAxis.Contains(axis))
        {
            return Input.GetAxisRaw(axis);
        }
        else
        {
            Debug.LogError("Error (" + this + ") \"" + axis + "\" is not declared in the UFPS Input Manager.");
            return 0;
        }
    }

    public static void ChangeButtonKey(string button, KeyCode keyCode, bool save = false)
    {
        if (Instance.Buttons.ContainsKey(button))
        {
            if (save) Instance.ButtonValues[vp_Input.Instance.ButtonKeys.IndexOf(button)] = keyCode;
            Instance.Buttons[button] = keyCode;
            return;
        }

        if (Instance.Buttons2.ContainsKey(button))
        {
            if (save) Instance.ButtonValues2[vp_Input.Instance.ButtonKeys.IndexOf(button)] = keyCode;
            Instance.Buttons2[button] = keyCode;
            return;
        }

        Instance.HaveBinding(button);
    }

    public static void ChangeAxis(string n, KeyCode pk = KeyCode.None, KeyCode nk = KeyCode.None, bool save = false)
    {
        if (!Instance.AxisKeys.Contains(n))
        {
            Debug.LogWarning("The Axis \"" + n + "\" Doesn't Exist");
            return;
        }

        if (save)
            Instance.AxisValues[vp_Input.Instance.AxisKeys.IndexOf(n)] = new vp_InputAxis { Positive = pk, Negative = nk };

        Instance.Axis[n] = new vp_InputAxis { Positive = pk, Negative = nk };
    }

    void DebugDumpCollections()
    {
        string buttonlist = "\n\n---- BUTTON KEYS: ---- \n";
        string keyCodelist = "\n\n---- BUTTON VALUES: ---- \n";
        string keyCodelist2 = "\n\n---- BUTTON VALUES 2: ---- \n";

        foreach (string s in ButtonKeys)
            buttonlist += ("\t\t" + s + "\n");

        foreach (KeyCode k in ButtonValues)
            keyCodelist += ("\t\t" + k + "\n");

        foreach (KeyCode k in ButtonValues2)
            keyCodelist2 += ("\t\t" + k + "\n");

        Debug.Log("-------- DUMP --------\n" + buttonlist + keyCodelist + keyCodelist2);
    }
}
