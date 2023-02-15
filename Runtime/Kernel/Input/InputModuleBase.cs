using UnityEngine;

namespace Hypnos.IO
{
    public abstract class InputModuleBase
    {
        protected AxisKeyPair[] axisKeyPairs = new AxisKeyPair[0];
        protected ButtonKeyPair[] buttonKeyPairs = new ButtonKeyPair[0];

        public abstract void Reset();

        protected internal virtual void Check()
        {
            CheckAxisInput();
            CheckKeyboardInput();
            CheckJoystickInput();
        }

        protected internal abstract void Process();

        protected virtual void CheckTouchInput()
        {

        }

        protected virtual void CheckAxisInput()
        {
            for (int i = 0; i < axisKeyPairs.Length; ++i)
            {
                OnAxisInputReceive(axisKeyPairs[i].id, Input.GetAxis(axisKeyPairs[i].axisX), Input.GetAxis(axisKeyPairs[i].axisY));
            }
        }

        protected virtual void CheckKeyboardInput()
        {
            for (int i = 0; i < buttonKeyPairs.Length; ++i)
            {
                if (Input.GetKeyDown(buttonKeyPairs[i].keyboardKey))
                {
                    OnButtonInputReceive(buttonKeyPairs[i].id, true);
                }

                if (Input.GetKeyUp(buttonKeyPairs[i].keyboardKey))
                {
                    OnButtonInputReceive(buttonKeyPairs[i].id, false);
                }
            }
        }

        protected virtual void CheckJoystickInput()
        {
            for (int i = 0; i < buttonKeyPairs.Length; ++i)
            {
                if (Input.GetKeyDown(buttonKeyPairs[i].joystickKey))
                {
                    OnButtonInputReceive(buttonKeyPairs[i].id, true);
                }

                if (Input.GetKeyUp(buttonKeyPairs[i].joystickKey))
                {
                    OnButtonInputReceive(buttonKeyPairs[i].id, false);
                }
            }
        }

        protected abstract void OnAxisInputReceive(int id, float x, float y);

        protected abstract void OnButtonInputReceive(int id, bool isDown);
    }
}