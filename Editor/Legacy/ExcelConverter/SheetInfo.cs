using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;

namespace Blanketmen.Hypnos.Editor
{
    // IRow.LastCellNum is the index of the last cell contained in this row PLUS ONE.
    // Both ISheet.LastRowNum and IRow.LastCellNum are 0-based.
    [Serializable]
    public class SheetInfo : IEquatable<SheetInfo>
    {
        // File Data
        public string sheetName;
        public string fileName;

        // Sheet Data
        public ISheet sheet;
        public IRow classRow;
        public IRow typeRow;
        public IRow nameRow;

        // Code Data
        public string className;
        public List<SheetFieldInfo> fieldInfos;

        // Value Data
        public int LastRowIndex { get { return sheet.LastRowNum; } }
        public int firstValueRowIndex;
        public int totalValueRowCount;

        // Convert Settings
        public bool isGenCode;
        public bool isGenFile;

        public SheetInfo(ISheet sheet)
        {
            this.sheet = sheet;
            sheetName = sheet.SheetName;
        }

        public bool Equals(SheetInfo other)
        {
            return other.className == className;
        }

        public bool IsFieldVaild()
        {
            if (string.IsNullOrEmpty(className))
            {
                Kernel.LogError($"[SheetInfo] Class name is not valid. Sheet: {sheetName}");
                return false;
            }

            if (fieldInfos.Count == 0)
            {
                Kernel.LogError($"[SheetInfo] Field info is not valid. Sheet: {sheetName}");
                return false;
            }

            return true;
        }

        // TODO: Impl
        public bool IsValueVaild()
        {
            if (firstValueRowIndex > LastRowIndex)
            {
                Kernel.LogError($"[SheetInfo] {sheetName} No data row to parse. LastRowNum: {LastRowIndex}");
                return false;
            }
            return true;
        }
    }
}