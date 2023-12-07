using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ekkam {
    public class RumbleManager : MonoBehaviour
    {
        public static RumbleManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }
        public void RumblePulse(Gamepad pad, float lowFrequency, float highFrequency, float duration)
        {
            if (pad != null)
            {
                // Start rumble
                pad.SetMotorSpeeds(lowFrequency, highFrequency);

                // Stop rumble after duration
                StartCoroutine(StopRumble(pad, duration));
            }
        }

        IEnumerator StopRumble(Gamepad pad, float duration)
        {
            yield return new WaitForSeconds(duration);
            pad.SetMotorSpeeds(0, 0);
        }

        public void StopContinuousRumble(Gamepad pad)
        {
            if (pad != null)
            {
                // Stop rumble
                pad.SetMotorSpeeds(0, 0);
            }
        }

        public void StartRumbleContinuous(Gamepad pad, float lowFrequency, float highFrequency)
        {
            if (pad != null)
            {
                // Start rumble
                pad.SetMotorSpeeds(lowFrequency, highFrequency);
            }
        }

        private void OnApplicationQuit() {
            StopAllRumble();
        }

        void OnDestroy()
        {
            StopAllRumble();
        }

        public void StopAllRumble()
        {
            foreach (Gamepad pad in Gamepad.all)
            {
                pad.SetMotorSpeeds(0, 0);
            }
        }
    }
}