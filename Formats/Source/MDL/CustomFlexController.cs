using uSource.Formats.Source.MDL;
#if UNITY_EDITOR 

using static uSource.Formats.Source.MDL.MDLFile;
#endif

public class CustomFlexController : BaseFlexController
{
    public override void InitializeFlexControllers()
    {
        // Call base class to initialize default controllers
        base.InitializeFlexControllers();

        // Add additional custom flex controllers
        FlexControllers.Add(new FlexController());
        FlexControllers.Add(new FlexController());

        // Modify existing flex controllers
        UpdateFlexController("smile", 0.3f, 1.5f, 0.8f); // Set min, max, and default for "smile"
        UpdateFlexController("jaw_drop", 0f, 2f, 1f);    // Adjust the range for "jaw_drop"

        // Remove a specific flex controller (if needed)
        FlexControllers.RemoveAll(fc => fc.Name == "gesture_rightleft");

        // Example: Add a new controller if another one is missing
        if (!FlexControllers.Exists(fc => fc.Name == "gesture_updown"))
        {
            FlexControllers.Add(new FlexController());
        }
    }

    /// <summary>
    /// Updates the values of an existing flex controller by name.
    /// </summary>
    private void UpdateFlexController(string name, float min, float max, float defaultValue)
    {
        var flexController = FlexControllers.Find(fc => fc.Name == name);
        if (flexController != null)
        {
            flexController.Min = min;
            flexController.Max = max;
            flexController.Default = defaultValue;
            flexController.Current = defaultValue; // Initialize current to default
        }
    }

    public void AddCustomFlexController(string name, float min, float max, float defaultValue)
    {
        FlexControllers.Add(new FlexController());
    }

    public void RemoveCustomFlexController(string name)
    {
        FlexControllers.RemoveAll(fc => fc.Name == name);
    }

    public void ClearCustomFlexControllers()
    {
        FlexControllers.Clear();
    }
}
