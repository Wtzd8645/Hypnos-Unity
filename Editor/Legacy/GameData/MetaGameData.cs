using System;
using System.Collections.Generic;

namespace Morpheus.Editor.GameData
{
    // TODO: Add generator type;
    [Serializable]
    public class MetaGameData
    {
        public string fileName;
        public string sheetName;
        public CodeGenerator codeGenerator;
        public FileGenerator fileGenerator;
        public bool isGenCode;
        public bool isGenFile;

        public string dataClass;
        public List<MetaGameDataField> dataFields;
        public List<string[]> rowValues;

        #region Excel Parameter
        public int firstValueRowIndex;
        #endregion

        public bool IsFormatVaild
        {
            get
            {
                return !(string.IsNullOrEmpty(dataClass) || dataFields.Count == 0);
            }
        }

        public MetaGameData()
        {
            dataFields = new List<MetaGameDataField>();
            rowValues = new List<string[]>();
        }

        public void SetClass(string name)
        {
            dataClass = name;
        }

        public void AddField(int colIndex, Type fieldType, string fieldName)
        {
            bool isUnique = true;
            for (int i = 0; i < dataFields.Count; i++)
            {
                if (dataFields[i].name == fieldName)
                {
                    isUnique = false;
                    break;
                }
            }

            if (isUnique)
            {
                MetaGameDataField field = new MetaGameDataField()
                {
                    ColumnIndex = colIndex,
                    type = fieldType,
                    name = fieldName
                };

                dataFields.Add(field);
            }
        }

        public void AddRowValue(string[] rowValue)
        {
            if (rowValue.Length == dataFields.Count)
            {
                rowValues.Add(rowValue);
            }
        }
    }
}