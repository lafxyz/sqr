using System.Drawing;

namespace SQR.Extenstions;

public static class CustomIntEnumeratorExtenstion
{
    public static CustomIntEnumerator GetEnumerator(this Range range)
    {
        return new CustomIntEnumerator(range);
    }
}

public ref struct CustomIntEnumerator
{
    public int Current => _current;
    private int _current;
    private int _max;

    public CustomIntEnumerator(Range range)
    {
        _current = range.Start.Value - 1;
        _max = range.End.Value;
    }

    public bool MoveNext()
    {
        _current++;
        return _current <= _max;
    }
}

