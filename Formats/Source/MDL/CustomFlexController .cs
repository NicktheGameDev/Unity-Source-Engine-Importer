using uSource.Formats.Source.MDL;
using static uSource.Formats.Source.MDL.MDLFile;

public class CustomFlexController_ : BaseFlexController
{
    public override void InitializeFlexControllers()
    {
        // Call base class to initialize default controllers
        base.InitializeFlexControllers();

        // Add additional custom flex controllers
        FlexControllers.Add(new FlexController { Name = "custom_flex_1", Min = -1f, Max = 2f, Default = 0.5f, Current = 0.5f });
        FlexControllers.Add(new FlexController { Name = "custom_flex_2", Min = 0f, Max = 3f, Default = 1f, Current = 1f });

        // Modify existing flex controllers
        UpdateFlexController("smile", 0.3f, 1.5f, 0.8f); // Set min, max, and default for "smile"
        UpdateFlexController("jaw_drop", 0f, 2f, 1f);    // Adjust the range for "jaw_drop"

        // Remove a specific flex controller (if needed)
        FlexControllers.RemoveAll(fc => fc.Name == "gesture_rightleft");

        // Example: Add a new controller if another one is missing
        if (!FlexControllers.Exists(fc => fc.Name == "gesture_updown"))
        {
            FlexControllers.Add(new FlexController { Name = "gesture_updown", Min = 0f, Max = 1f, Default = 0.5f, Current = 0.5f });
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
}
