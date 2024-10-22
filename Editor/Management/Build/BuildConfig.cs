using System;
using System.Collections.Generic;
using UnityEditor;

namespace Blanketmen.Hypnos.Editor.Build
{
    [Serializable]
    public class EditionConfig
    {
        public int id;
        public string name = "New Edition";
        public string version = "0.0.0";
        public int buildVersion;
        public BuildTarget platform = BuildTarget.Android;
    }

    [Serializable]
    public class BuildConfig
    {
        public List<EditionConfig> editionConfigs = new List<EditionConfig>(2);
    }
}