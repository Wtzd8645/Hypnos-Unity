using System;

namespace Hypnos.Editor.GameData
{
    public class MetaGameDataField : IEquatable<MetaGameDataField>
    {
        public int ColumnIndex;

        public Type type;
        public string name;

        public bool Equals(MetaGameDataField other)
        {
            return other.name == name;
        }
    }
}