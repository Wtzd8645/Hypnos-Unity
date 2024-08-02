using System;
using UnityEngine;

namespace Blanketmen.Hypnos
{
    [Serializable]
    public struct AxisKeyPair
    {
        public int id;
        public string axisX;
        public string axisY;
    }

    [Serializable]
    public struct ButtonKeyPair
    {
        public int id;
        public KeyCode keyboardKey;
        public KeyCode joystickKey;
    }

    public class InputModuleConfig : ScriptableObject
    {
        public AxisKeyPair[] axisKeyPairs = new AxisKeyPair[0];
        public ButtonKeyPair[] buttonKeyPairs = new ButtonKeyPair[0];
    }
}