using NPOI.SS.UserModel;

namespace Morpheus.Editor
{
    public interface IExcelHandler
    {
        bool Parse(IWorkbook iWorkBook);
        void GenerateXML(bool iIsEncrypt);
        void GenerateCSharpCode();
    }
}