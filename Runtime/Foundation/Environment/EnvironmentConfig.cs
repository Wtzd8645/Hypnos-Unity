using System;
using UnityEngine;

namespace Blanketmen.Hypnos
{
    public class EnvironmentConfig : ScriptableObject
    {
        public bool runInBackground = true;
        public bool neverSleepScreen = true;
        [Range(30f, 144f)] public int targetFrameRate = 60;
        public Vector2 maxScreenResolution = new Vector2(1334f, 750f);
    }
}