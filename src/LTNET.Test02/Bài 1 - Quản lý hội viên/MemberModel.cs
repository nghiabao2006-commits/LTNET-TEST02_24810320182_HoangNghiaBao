using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LTNET.Test02.Models;

public sealed class MemberModel : INotifyPropertyChanged
{
    private string memberId = "";
    private string fullName = "";
    private string phone = "";
    private string package = "Theo tháng";
    private DateTime startDate = DateTime.Today;
    private long fee;

    public string MemberId { get => memberId; set => Set(ref memberId, value); }
    public string FullName { get => fullName; set => Set(ref fullName, value); }
    public string Phone { get => phone; set => Set(ref phone, value); }
    public string Package { get => package; set => Set(ref package, value); }
    public DateTime StartDate { get => startDate; set => Set(ref startDate, value); }
    public long Fee { get => fee; set => Set(ref fee, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
