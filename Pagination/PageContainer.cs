using System.Collections;
using DisCatSharp.Interactivity;

namespace SQR.Pagination;

public class PageContainer<T>
{
    public IList<Page<T>> Pages => _pages;
    
    private IList<Page<T>> _pages = new List<Page<T>>();
    private int _currentIndex;

    /// <summary>
    /// PageContainer constructor 
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when items less or equals to 0</exception>
    public PageContainer(IEnumerable<T> items, int itemsPerPage = 10)
    {
        var index = 1;
        var enumerable = items.ToList();

        if (enumerable.Count <= 0)
        {
            throw new ArgumentException($"{nameof(items)}'s count must be greater than 0");
        }
        
        for (var i = 0; i < enumerable.Count; i += itemsPerPage)
        {
            var iCopy = i;
            var content = enumerable.Take(iCopy..(iCopy+itemsPerPage));

            var page = new Page<T>(index, content);
            _pages.Add(page);
            index++;
        }
    }

    public Page<T> Current()
    {
        return _pages.ElementAt(_currentIndex);
    }

    public Page<T>? PreviousOrDefault()
    {
        var e = _pages.ElementAtOrDefault(_currentIndex - 1);

        if (e != null)
            _currentIndex--;
        
        return e;
    }
    
    public Page<T>? NextOrDefault()
    {
        var e = _pages.ElementAtOrDefault(_currentIndex + 1);

        if (e != null)
            _currentIndex++;
        
        return e;
    }
}