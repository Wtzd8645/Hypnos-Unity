using UnityEngine;

namespace Morpheus.Environment
{
    public class EnvironmentConfig : ScriptableObject
    {
        public bool RunInBackground;
        [Range(30f, 144f)] public int TargetFrameRate = 60;
        public Vector2 MaxScreenResolution = new Vector2(1334f, 750f);
        public bool NeverSleepScreen;
    }
}