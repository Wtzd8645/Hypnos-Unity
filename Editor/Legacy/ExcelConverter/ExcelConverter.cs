using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace Blanketmen.Hypnos.Editor
{
    // XSSWorkbook is for .xlsx and it can't write to stream or new an instance with no parameter in Unity.
    // If you need write to stream, use HSSWorkbook instead.
    public class ExcelConverter
    {
        #region Constant
        public const string XlsExtension = ".xls";
        public const string XlsxExtension = ".xlsx";
        #endregion

        private SheetInfoParser excelParser;

        // TODO: Make handler alterable.
        public ExcelConverter()
        {
            excelParser = new SheetInfoParser();
        }

        public List<SheetInfo> GetSheetInfo(string path)
        {
            string fileName = Path.GetFileName(path);
            if (fileName.StartsWith("~$"))
            {
                return null;
            }

            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                IWorkbook workbook;
                switch (Path.GetExtension(path))
                {
                    case XlsExtension:
                        workbook = new HSSFWorkbook(fs);
                        break;
                    case XlsxExtension:
                        workbook = new XSSFWorkbook(fs);
                        break;
                    default:
                        Logging.Error($"[ExcelConverter] The file is not xls or xlsx format. File: {fileName}");
                        return null;
                }

                return excelParser.ParseSheetInfo(workbook, fileName);
            }
            catch (Exception e)
            {
                Logging.Error($"[ExcelConverter] {e.Message}");
                return null;
            }
            finally
            {
                fs.Close();
            }
        }

        // TODO: Make generator alterable.
        public void Convert(List<SheetInfo> sheetInfos, string codeOutputPath, string fileOutputPath)
        {
            CSharpGenerator codeGenerator = new CSharpGenerator();
            XmlGenerator fileGenerator = new XmlGenerator();
            for (int i = 0; i < sheetInfos.Count; i++)
            {
                excelParser.FetchFieldInfo(sheetInfos[i]);
                if (sheetInfos[i].isGenCode)
                {
                    codeGenerator.Generate(sheetInfos[i], codeOutputPath);
                }

                if (sheetInfos[i].isGenFile)
                {
                    excelParser.FetchValue(sheetInfos[i]);
                    fileGenerator.GenerateFile(sheetInfos[i], fileOutputPath);
                }
            }
        }
    }
}