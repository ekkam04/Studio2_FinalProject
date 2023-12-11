using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Dualsense
{
    // public static Gamepad controller = null;

    public static bool TryGettingDualsense(Gamepad gamepad, string layoutFile = null)
    {
        Debug.Log("Dualsense.getController()");

        Gamepad controller = null;
        string layout = File.ReadAllText(layoutFile == null ? "Assets/EkkamStuff/dualsenseHIDLayoutCustom.json" : layoutFile);

        InputSystem.RegisterLayoutOverride(layout, "DualSenseCustom");

        if (gamepad is Gamepad && gamepad.description.interfaceName == "HID" && gamepad.description.product.Contains("DualSense"))
        {
            controller = gamepad;
            Debug.Log("Player has joined with DualSense controller");
            return true;
        }
        else
        {
            Debug.Log("Player has joined without DualSense controller");
            return false;
        }
    }

    public static Quaternion GetRotation(float scale = 1.0f, Gamepad controller = null)
    {
        ButtonControl gyroX = controller.GetChildControl<ButtonControl>("gyro X 17");
        ButtonControl gyroY = controller.GetChildControl<ButtonControl>("gyro Y 19");
        ButtonControl gyroZ = controller.GetChildControl<ButtonControl>("gyro Z 21");

        float x = ProcessRawData(gyroX.ReadValue()) * scale;
        float y = ProcessRawData(gyroY.ReadValue()) * scale;
        float z = ProcessRawData(gyroZ.ReadValue()) * -scale;
        return Quaternion.Euler(x, 0, z);
    }

    public static Vector3 GetDirection(float scale = 1.0f, Gamepad controller = null)
    {
        if (controller == null) return Vector3.zero;
        ButtonControl gyroX = controller.GetChildControl<ButtonControl>("gyro X 17");
        ButtonControl gyroY = controller.GetChildControl<ButtonControl>("gyro Y 19");
        ButtonControl gyroZ = controller.GetChildControl<ButtonControl>("gyro Z 21");

        float x = ProcessRawData(gyroX.ReadValue()) * scale;
        float y = ProcessRawData(gyroY.ReadValue()) * scale;
        float z = ProcessRawData(gyroZ.ReadValue()) * scale;
        return new Vector3(z, x, 0);
    }

    private static float ProcessRawData(float data)
    {
        return data > 0.5 ? 1 - data : -data;
    }
}
