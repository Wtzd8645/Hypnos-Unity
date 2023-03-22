using UnityEngine;

namespace Blanketmen.Hypnos
{
    public class EnvironmentConfig : ScriptableObject
    {
        public bool runInBackground;
        [Range(30f, 144f)] public int targetFrameRate = 60;
        public Vector2 maxScreenResolution = new Vector2(1334f, 750f);
        public bool neverSleepScreen;
    }
}