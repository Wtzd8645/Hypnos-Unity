using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;

namespace Morpheus.Editor
{
    public class SheetInfoParser
    {
        private const string CommentTag = "#";
        private const string ClassNameTag = "Class";
        private const string FieldTypeTag = "Type";
        private const string FieldNameTag = "Name";

        public static string GetCellString(ICell iCell)
        {
            if (iCell == null)
            {
                return string.Empty;
            }

            if (iCell.CellType != CellType.String)
            {
                iCell.SetCellType(CellType.String);
            }

            return iCell.StringCellValue;
        }

        /// <summary>
        /// Parse all sheets in workbook to SheetInfo.
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="fileName"></param>
        /// <returns>The returned list won't be null.</returns>
        public virtual List<SheetInfo> ParseSheetInfo(IWorkbook workbook, string fileName)
        {
            if (workbook == null)
            {
                Logger.Log("[ExcelParser] Workbook is null.");
                return new List<SheetInfo>();
            }

            int sheetNum = workbook.NumberOfSheets;
            List<SheetInfo> sheets = new List<SheetInfo>(sheetNum);
            for (int i = 0; i < sheetNum; i++)
            {
                string sheetName = workbook.GetSheetName(i);
                if (sheetName.StartsWith(CommentTag))
                {
                    continue;
                }

                SheetInfo sheetInfo = new SheetInfo(workbook.GetSheetAt(i))
                {
                    fileName = fileName
                };
                ParseRowInfo(sheetInfo);
                FetchClassName(sheetInfo);

                sheets.Add(sheetInfo);
            }

            return sheets;
        }

        private void ParseRowInfo(SheetInfo sheetInfo)
        {
            int lastRowCount = sheetInfo.LastRowIndex + 1;
            for (int i = 0; i < lastRowCount; i++)
            {
                IRow row = sheetInfo.sheet.GetRow(i);
                if (row == null)
                {
                    continue;
                }

                // NOTE: Can modify start column.
                string str = GetCellString(row.GetCell(0));
                if (string.IsNullOrEmpty(str))
                {
                    continue;
                }

                if (string.Equals(str, ClassNameTag, StringComparison.OrdinalIgnoreCase))
                {
                    sheetInfo.classRow = row;
                }
                else if (string.Equals(str, FieldTypeTag, StringComparison.OrdinalIgnoreCase))
                {
                    sheetInfo.typeRow = row;
                }
                else if (string.Equals(str, FieldNameTag, StringComparison.OrdinalIgnoreCase))
                {
                    sheetInfo.nameRow = row;
                }
                else
                {
                    continue;
                }

                if (sheetInfo.classRow != null && sheetInfo.typeRow != null && sheetInfo.nameRow != null)
                {
                    sheetInfo.firstValueRowIndex = i + 1;
                    sheetInfo.totalValueRowCount = sheetInfo.LastRowIndex - sheetInfo.firstValueRowIndex + 1;
                    break;
                }
            }
        }

        private void FetchClassName(SheetInfo sheetInfo)
        {
            if (sheetInfo.classRow == null)
            {
                Logger.LogWarning($"[ExcelParser] classRow is null. Sheet: {sheetInfo.sheetName}");
                return;
            }

            sheetInfo.className = GetCellString(sheetInfo.classRow.GetCell(1));
        }

        public virtual void FetchFieldInfo(SheetInfo sheetInfo)
        {
            if (sheetInfo.typeRow == null || sheetInfo.nameRow == null)
            {
                Logger.LogWarning($"[ExcelParser] typeRow or nameRow is null. Sheet: {sheetInfo.sheetName}");
                return;
            }

            int colNum = sheetInfo.typeRow.LastCellNum;
            List<SheetFieldInfo> fieldInfos = new List<SheetFieldInfo>(colNum);
            string typeStr = string.Empty;
            string nameStr = string.Empty;
            for (int i = 1; i < colNum; i++)
            {
                typeStr = GetCellString(sheetInfo.typeRow.GetCell(i));
                if (string.IsNullOrEmpty(typeStr) || typeStr.StartsWith(CommentTag))
                {
                    continue;
                }

                Type fieldType = Type.GetType("System." + typeStr);
                if (fieldType == null)
                {
                    Logger.LogWarning($"[ExcelParser] Can't get type at column {i - 1}. Sheet: {sheetInfo.sheetName}");
                    continue;
                }

                nameStr = GetCellString(sheetInfo.nameRow.GetCell(i));
                if (string.IsNullOrEmpty(nameStr))
                {
                    continue;
                }

                SheetFieldInfo fieldInfo = new SheetFieldInfo()
                {
                    type = fieldType,
                    name = nameStr,
                    columnIndex = i
                };

                if (fieldInfos.Contains(fieldInfo))
                {
                    Logger.LogError($"[ExcelParser] FieldName is duplicate at column {i - 1}. Sheet: {sheetInfo.sheetName}");
                    continue;
                }

                fieldInfos.Add(fieldInfo);
            }

            sheetInfo.fieldInfos = fieldInfos;
        }

        public virtual void FetchValue(SheetInfo sheetInfo)
        {
            ISheet sheet = sheetInfo.sheet;
            List<SheetFieldInfo> fieldInfos = sheetInfo.fieldInfos;

            int lastRowIdx = sheet.LastRowNum;
            int infosCount = fieldInfos.Count;
            for (int i = 0; i < infosCount; i++)
            {
                fieldInfos[i].values = new List<string>(sheetInfo.totalValueRowCount);
            }

            for (int i = sheetInfo.firstValueRowIndex; i <= lastRowIdx; i++)
            {
                IRow row = sheet.GetRow(i);
                if (GetCellString(row.GetCell(0)).StartsWith(CommentTag))
                {
                    sheetInfo.totalValueRowCount--;
                    continue;
                }

                for (int j = 0; j < infosCount; j++)
                {
                    string value = GetCellString(row.GetCell(fieldInfos[j].columnIndex));
                    fieldInfos[j].values.Add(value);
                }
            }
        }
    }
}