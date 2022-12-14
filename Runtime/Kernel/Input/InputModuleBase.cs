using UnityEngine;

namespace Morpheus.IO
{
    public abstract class InputModuleBase
    {
        protected AxisKeyPair[] AxisKeyPairs = new AxisKeyPair[0];
        protected ButtonKeyPair[] ButtonKeyPairs = new ButtonKeyPair[0];

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
            for (int i = 0; i < AxisKeyPairs.Length; ++i)
            {
                OnAxisInputReceive(AxisKeyPairs[i].Id, Input.GetAxis(AxisKeyPairs[i].AxisX), Input.GetAxis(AxisKeyPairs[i].AxisY));
            }
        }

        protected virtual void CheckKeyboardInput()
        {
            for (int i = 0; i < ButtonKeyPairs.Length; ++i)
            {
                if (Input.GetKeyDown(ButtonKeyPairs[i].KeyboardKey))
                {
                    OnButtonInputReceive(ButtonKeyPairs[i].Id, true);
                }

                if (Input.GetKeyUp(ButtonKeyPairs[i].KeyboardKey))
                {
                    OnButtonInputReceive(ButtonKeyPairs[i].Id, false);
                }
            }
        }

        protected virtual void CheckJoystickInput()
        {
            for (int i = 0; i < ButtonKeyPairs.Length; ++i)
            {
                if (Input.GetKeyDown(ButtonKeyPairs[i].JoystickKey))
                {
                    OnButtonInputReceive(ButtonKeyPairs[i].Id, true);
                }

                if (Input.GetKeyUp(ButtonKeyPairs[i].JoystickKey))
                {
                    OnButtonInputReceive(ButtonKeyPairs[i].Id, false);
                }
            }
        }

        protected abstract void OnAxisInputReceive(int id, float x, float y);

        protected abstract void OnButtonInputReceive(int id, bool isDown);
    }
}