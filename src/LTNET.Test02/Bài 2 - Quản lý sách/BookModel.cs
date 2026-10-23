using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LTNET.Test02.Books;

public sealed class BookModel : INotifyPropertyChanged
{
    private string bookId = "", title = "", author = "", coverPath = "";
    private decimal price;
    private int quantity;
    public string BookId { get => bookId; set => Set(ref bookId, value); }
    public string Title { get => title; set => Set(ref title, value); }
    public string Author { get => author; set => Set(ref author, value); }
    public decimal Price { get => price; set => Set(ref price, value); }
    public int Quantity { get => quantity; set => Set(ref quantity, value); }
    public string CoverPath
    {
        get => coverPath;
        set { if (Set(ref coverPath, value)) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoverFileName))); }
    }
    public string CoverFileName => Path.GetFileName(CoverPath);
    public decimal InventoryValue => Price * Quantity;
    public event PropertyChangedEventHandler? PropertyChanged;
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        if (name is nameof(Price) or nameof(Quantity))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InventoryValue)));
        return true;
    }
}
