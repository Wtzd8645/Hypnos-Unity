using System;
using System.Collections.Generic;

public class SheetFieldInfo : IEquatable<SheetFieldInfo>
{
    public Type type;
    public string name;

    public int columnIndex;
    public List<string> values;

    public bool Equals(SheetFieldInfo other)
    {
        return other.name == name;
    }
}