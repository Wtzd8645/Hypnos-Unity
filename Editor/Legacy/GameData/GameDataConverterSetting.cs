using System.Collections.Generic;
using UnityEngine;

namespace Blanketmen.Hypnos.Editor.GameData
{
    [CreateAssetMenu(fileName = "GameDataConverterSetting", menuName = "Editor/GameDataConverterSetting")]
    public class GameDataConverterSetting : ScriptableObject
    {
        public string sourceFilePath;

        public string bytesOutputPath;

        public string cSharpCodeNamespace;
        public string dataManagerTypeName;
        public string cSharpCodeOutputPath;

        public Dictionary<string, SheetConvertSetting> sheetConvertSettings;

        private void Awake()
        {
            sourceFilePath = Application.dataPath + "/Editor/GameDataConverter/SampleFiles";
            bytesOutputPath = Application.dataPath + "/AutoGen/Binary";
            cSharpCodeOutputPath = Application.dataPath + "/Scripts/AutoGen";

            sheetConvertSettings = new Dictionary<string, SheetConvertSetting>();
        }
    }
}