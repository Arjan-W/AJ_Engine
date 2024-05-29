using System;
namespace AJ.Engine.Interfaces.Util.Strings;

public readonly struct NewLine
{
    public static int Length => _length;

    private static readonly int _length = Environment.NewLine.Length;
    private static readonly string _text = Environment.NewLine;
    private static readonly char[] characters = _text.ToCharArray();
    private static readonly ReadOnlyMemory<char> _memory;

    static NewLine()
    {
        _memory = characters;
    }

    public static implicit operator string(NewLine newLine)
    {
        return _text;
    }

    public static void CopyTo(Span<char> destination)
    {
        _memory.Span.CopyTo(destination);
    }

    public override string ToString()
    {
        return _text;
    }
}