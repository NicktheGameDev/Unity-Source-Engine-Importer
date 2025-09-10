using System.Collections.Generic;
using System.IO;
using UnityEngine;
using uSource;

public class CustomFlexControllerParser : BaseFlexControllerParser
{
    
    
        public override List<FlexController> ParseFlexControllers(BinaryReader fileStream, int controllerOffset, int controllerCount)
        {
            // Parse base flex controllers
            List<FlexController> baseControllers = base.ParseFlexControllers(fileStream, controllerOffset, controllerCount);

            // Additional logic to enhance or validate flex controllers
            foreach (var controller in baseControllers)
            {
                Debug.Log($"Parsed Flex Controller: {controller.Name}");

                // Ensure the default value is within the allowed range
                if (controller.Default < controller.Min || controller.Default > controller.Max)
                {
                    Debug.LogWarning($"Controller {controller.Name} has an invalid default value. Clamping to valid range.");
                    controller.Default = Mathf.Clamp(controller.Default, controller.Min, controller.Max);
                }

                // Apply scaling if needed (e.g., normalize values to a specific range)
                float range = controller.Max - controller.Min;
                if (range > 0)
                {
                    controller.Min /= range;
                    controller.Max /= range;
                    controller.Default /= range;
                    Debug.Log($"Controller {controller.Name} values normalized to a [0, 1] range.");
                }

                // Add metadata or tags to certain controllers based on naming conventions
                if (controller.Name.ToLower().Contains("eyebrow"))
                {
                    controller.Current = controller.Default; // Set initial value
                    Debug.Log($"Controller {controller.Name} tagged as 'Eyebrow'. Current value set to default.");
                }
                else if (controller.Name.ToLower().Contains("jaw"))
                {
                    controller.Current = controller.Max; // Maximize for testing jaw movement
                    Debug.Log($"Controller {controller.Name} tagged as 'Jaw'. Current value set to max.");
                }

                // Log processed controller details
                Debug.Log($"Finalized Controller {controller.Name}: Min={controller.Min}, Max={controller.Max}, Default={controller.Default}, Current={controller.Current}");
            }

            // Perform global validations or transformations after individual processing
            if (baseControllers.Count == 0)
            {
                Debug.LogError("No valid flex controllers were found or parsed.");
            }
            else
            {
                Debug.Log($"Successfully processed {baseControllers.Count} flex controllers.");
            }

            return baseControllers;
        }
    }

