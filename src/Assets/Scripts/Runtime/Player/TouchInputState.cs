namespace PiterGame
{
    /// <summary>
    /// Shared state of the on-screen touch buttons.
    /// </summary>
    public static class TouchInputState
    {
        private static bool leftHeld;
        private static bool rightHeld;
        private static bool jumpQueued;
        private static bool attackQueued;
        private static bool potionQueued;

        public static float Horizontal => (rightHeld ? 1f : 0f) - (leftHeld ? 1f : 0f);

        public static bool JumpHeld { get; private set; }

        public static void SetButton(TouchButtonAction action, bool pressed)
        {
            switch (action)
            {
                case TouchButtonAction.Left:
                    leftHeld = pressed;
                    break;
                case TouchButtonAction.Right:
                    rightHeld = pressed;
                    break;
                case TouchButtonAction.Jump:
                    if (pressed && !JumpHeld)
                    {
                        jumpQueued = true;
                    }

                    JumpHeld = pressed;
                    break;
                case TouchButtonAction.Attack:
                    attackQueued |= pressed;
                    break;
                case TouchButtonAction.Potion:
                    potionQueued |= pressed;
                    break;
            }
        }

        public static bool ConsumeJump()
        {
            bool value = jumpQueued;
            jumpQueued = false;

            return value;
        }

        public static bool ConsumeAttack()
        {
            bool value = attackQueued;
            attackQueued = false;

            return value;
        }

        public static bool ConsumePotion()
        {
            bool value = potionQueued;
            potionQueued = false;

            return value;
        }

        public static void Reset()
        {
            leftHeld = false;
            rightHeld = false;
            jumpQueued = false;
            attackQueued = false;
            potionQueued = false;
            JumpHeld = false;
        }
    }
}
