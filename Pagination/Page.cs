namespace SQR.Pagination;

public class Page<T>
{
    public readonly int Index;
    public IEnumerable<T> Content { get; }

    public Page(int index, IEnumerable<T> content)
    {
        Index = index;
        Content = content;
    }
}