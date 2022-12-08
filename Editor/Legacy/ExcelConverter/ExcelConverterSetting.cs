using System.Collections.Generic;
using UnityEngine;

namespace Morpheus.Editor
{
    [CreateAssetMenu(fileName = "ExcelConverterSetting", menuName = "Editor/ExcelConverterSetting")]
    public class ExcelConverterSetting : ScriptableObject
    {
        public string excelSourcePath;

        public string bytesOutputPath;

        public string cSharpCodeNamespace;
        public string dataManagerTypeName;
        public string cSharpCodeOutputPath;

        public Dictionary<string, SheetConvertSetting> sheetConvertSettings;

        private void Awake()
        {
            excelSourcePath = Application.dataPath + "/Editor/ExcelConverter/SampleFiles";
            bytesOutputPath = Application.dataPath + "/AutoGen/Binary";
            cSharpCodeOutputPath = Application.dataPath + "/AutoGen/Scripts";

            sheetConvertSettings = new Dictionary<string, SheetConvertSetting>();
        }
    }
}