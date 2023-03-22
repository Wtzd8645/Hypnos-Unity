﻿using NPOI.SS.UserModel;

namespace Blanketmen.Hypnos.Editor
{
    public interface IExcelHandler
    {
        bool Parse(IWorkbook iWorkBook);
        void GenerateXML(bool iIsEncrypt);
        void GenerateCSharpCode();
    }
}