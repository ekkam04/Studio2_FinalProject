using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Dualsense
{
    public static bool TryGettingDualsense(Gamepad gamepad, string layoutFile = null)
    {
        Debug.Log("Dualsense.cs: TryGettingDualsense()");

        string layout = File.ReadAllText(layoutFile == null ? "C:/UnityProjects/Studio2_FinalProject/Assets/EkkamStuff/dualsenseHIDLayoutCustom.json" : layoutFile);

        InputSystem.RegisterLayoutOverride(layout, "DualSenseCustom");

        if (gamepad is Gamepad && (gamepad.description.interfaceName == "HID" || gamepad.description.product.Contains("DualSense") || gamepad.displayName.Contains("DualSense")))
        {
            Debug.Log("Player has joined with DualSense controller");
            Debug.Log("display name: " + gamepad.displayName + " product: " + gamepad.description.product + " interface name: " + gamepad.description.interfaceName);
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
        return Quaternion.Euler(x, y, z);
    }

    public static Vector3 GetDirection2D(float scale = 1.0f, Gamepad controller = null)
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
