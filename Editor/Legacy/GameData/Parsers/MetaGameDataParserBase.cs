using System.Collections.Generic;

namespace Blanketmen.Hypnos.Editor.GameData
{
    public abstract class MetaGameDataParserBase
    {
        public bool IsParsed { get; protected set; }

        public List<MetaGameData> MetadataList { get; protected set; }

        protected MetaGameDataParserBase()
        {
            MetadataList = new List<MetaGameData>();
        }

        public abstract void PreParse(string filePath);
        public abstract void Parse();

        public virtual void Clear()
        {
            MetadataList.Clear();
        }
    }
}