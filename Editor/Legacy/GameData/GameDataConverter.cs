using System.Collections.Generic;
using System.IO;

namespace Morpheus.Editor.GameData
{
    public enum CodeGenerator
    {
        CSharp
    }

    public enum FileGenerator
    {
        Xml
    }

    public class GameDataConverter
    {
        #region Constant
        public const string TemporaryFileTag = "~$";
        #endregion

        private List<MetaGameDataParserBase> parsers;

        public GameDataConverter()
        {
            parsers = new List<MetaGameDataParserBase>(); // TODO: Costum defalut number.
        }

        public void GetMetadataList(List<MetaGameData> result)
        {
            result.Clear();
            for (int i = 0; i < parsers.Count; i++)
            {
                result.AddRange(parsers[i].MetadataList);
            }
        }

        public void PreParse(string[] filePaths)
        {
            for (int i = 0; i < filePaths.Length; i++)
            {
                PreParse(filePaths[i]);
            }
        }

        public void PreParse(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            if (fileName.StartsWith(TemporaryFileTag))
            {
                return;
            }

            MetaGameDataParserBase parser = null;
            switch (Path.GetExtension(filePath))
            {
                case ExcelToMetaGameData.XlsExtension:
                    parser = new ExcelToMetaGameData(fileName, ExcelToMetaGameData.XlsExtension);
                    break;
                case ExcelToMetaGameData.XlsxExtension:
                    parser = new ExcelToMetaGameData(fileName, ExcelToMetaGameData.XlsxExtension);
                    break;
                default:
                    Kernel.LogWarning($"[GameDataConverter] The file format can't be parse. File: {fileName}");
                    return;
            }

            parser.PreParse(filePath);
            parsers.Add(parser);
        }

        public void Parse()
        {
            for (int i = 0; i < parsers.Count; i++)
            {
                parsers[i].Parse();
            }
        }

        public void Convert(GameDataConverterSetting setting)
        {
            List<MetaGameData> metadataList = new List<MetaGameData>();
            GetMetadataList(metadataList);
            for (int i = 0; i < metadataList.Count; i++)
            {
                if (metadataList[i].isGenCode)
                {
                    switch (metadataList[i].codeGenerator)
                    {
                        default:
                            MetaGameDataToCSharp.Generate(metadataList[i], setting);
                            break;
                    }
                }

                if (metadataList[i].isGenFile)
                {
                    switch (metadataList[i].fileGenerator)
                    {
                        default:
                            MetaGameDataToXml.Generate(metadataList[i], setting);
                            break;
                    }
                }
            }
        }

        private void GenerateCode(MetaGameData metadata, CodeGenerator generator)
        {
            switch (generator)
            {
                default:
                    MetaGameDataToCSharp.Generate(metadata, null);
                    break;
            }
        }

        private void GenerateFile(MetaGameData metadata, FileGenerator generator)
        {
            switch (generator)
            {
                default:
                    break;
            }
        }

        public void Clear()
        {
            parsers.Clear();
        }
    }
}