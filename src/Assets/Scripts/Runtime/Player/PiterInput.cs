using UnityEngine;
using UnityEngine.InputSystem;

namespace PiterGame
{
    /// <summary>
    /// Merges keyboard, gamepad and touch input into gameplay commands.
    /// </summary>
    public sealed class PiterInput : MonoBehaviour
    {
        private const float StickDeadZone = 0.3f;

        private bool inputEnabled = true;
        private int enabledFrame = -1;
        private bool jumpQueued;
        private bool attackQueued;
        private bool potionQueued;
        private bool simulated;
        private float simulatedHorizontal;

        public float Horizontal { get; private set; }

        public bool JumpHeld { get; private set; }

        public bool InputEnabled
        {
            get => inputEnabled;
            set
            {
                if (value && !inputEnabled)
                {
                    enabledFrame = Time.frameCount;
                }

                inputEnabled = value;

                if (!value)
                {
                    Clear();
                }
            }
        }

        public bool ConsumeJump()
        {
            bool value = jumpQueued;
            jumpQueued = false;

            return value;
        }

        public bool ConsumeAttack()
        {
            bool value = attackQueued;
            attackQueued = false;

            return value;
        }

        public bool ConsumePotion()
        {
            bool value = potionQueued;
            potionQueued = false;

            return value;
        }

        public void Clear()
        {
            Horizontal = 0f;
            JumpHeld = false;
            jumpQueued = false;
            attackQueued = false;
            potionQueued = false;
        }

        public void SimulateHorizontal(float value)
        {
            simulated = true;
            simulatedHorizontal = Mathf.Clamp(value, -1f, 1f);
        }

        public void SimulateJump()
        {
            simulated = true;
            jumpQueued = true;
        }

        public void SimulateAttack()
        {
            simulated = true;
            attackQueued = true;
        }

        public void StopSimulation()
        {
            simulated = false;
            simulatedHorizontal = 0f;
        }

        private static void ReadKeyboard(ref float horizontal, ref bool jumpHeld, ref bool jumpPressed, ref bool attackPressed, ref bool potionPressed)
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                horizontal -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                horizontal += 1f;
            }

            jumpHeld |= keyboard.spaceKey.isPressed || keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed;
            jumpPressed |= keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame;
            attackPressed |= keyboard.jKey.wasPressedThisFrame || keyboard.kKey.wasPressedThisFrame || keyboard.leftCtrlKey.wasPressedThisFrame;
            potionPressed |= keyboard.eKey.wasPressedThisFrame || keyboard.qKey.wasPressedThisFrame;
        }

        private static void ReadGamepad(ref float horizontal, ref bool jumpHeld, ref bool jumpPressed, ref bool attackPressed, ref bool potionPressed)
        {
            Gamepad gamepad = Gamepad.current;

            if (gamepad == null)
            {
                return;
            }

            float stick = gamepad.leftStick.ReadValue().x;

            if (Mathf.Abs(stick) > StickDeadZone)
            {
                horizontal += stick;
            }

            if (gamepad.dpad.left.isPressed)
            {
                horizontal -= 1f;
            }

            if (gamepad.dpad.right.isPressed)
            {
                horizontal += 1f;
            }

            jumpHeld |= gamepad.buttonSouth.isPressed;
            jumpPressed |= gamepad.buttonSouth.wasPressedThisFrame;
            attackPressed |= gamepad.buttonWest.wasPressedThisFrame || gamepad.rightTrigger.wasPressedThisFrame;
            potionPressed |= gamepad.buttonNorth.wasPressedThisFrame;
        }

        private void Update()
        {
            if (!inputEnabled)
            {
                Horizontal = 0f;
                JumpHeld = false;
                TouchInputState.ConsumeJump();
                TouchInputState.ConsumeAttack();
                TouchInputState.ConsumePotion();

                return;
            }

            float horizontal = TouchInputState.Horizontal;
            bool jumpHeld = TouchInputState.JumpHeld;
            bool jumpPressed = TouchInputState.ConsumeJump();
            bool attackPressed = TouchInputState.ConsumeAttack();
            bool potionPressed = TouchInputState.ConsumePotion();

            ReadKeyboard(ref horizontal, ref jumpHeld, ref jumpPressed, ref attackPressed, ref potionPressed);
            ReadGamepad(ref horizontal, ref jumpHeld, ref jumpPressed, ref attackPressed, ref potionPressed);

            if (simulated)
            {
                horizontal = simulatedHorizontal;
                jumpHeld = true;
            }

            Horizontal = Mathf.Clamp(horizontal, -1f, 1f);
            JumpHeld = jumpHeld;

            if (Time.frameCount == enabledFrame)
            {
                return;
            }

            jumpQueued |= jumpPressed;
            attackQueued |= attackPressed;
            potionQueued |= potionPressed;
        }

    }
}
