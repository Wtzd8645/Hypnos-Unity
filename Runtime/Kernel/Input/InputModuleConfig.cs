using System;
using UnityEngine;

namespace Morpheus.IO
{
    [Serializable]
    public struct AxisKeyPair
    {
        public int Id;
        public string AxisX;
        public string AxisY;
    }

    [Serializable]
    public struct ButtonKeyPair
    {
        public int Id;
        public KeyCode KeyboardKey;
        public KeyCode JoystickKey;
    }

    public class InputModuleConfig : ScriptableObject
    {
        public AxisKeyPair[] AxisKeyPairs = new AxisKeyPair[0];
        public ButtonKeyPair[] ButtonKeyPairs = new ButtonKeyPair[0];
    }
}