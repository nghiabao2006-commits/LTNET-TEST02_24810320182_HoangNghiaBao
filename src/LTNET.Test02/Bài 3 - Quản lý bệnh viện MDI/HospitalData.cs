using System.ComponentModel;

namespace LTNET.Test02.Hospital;

public sealed class PatientModel
{
    public string PatientId { get; set; } = "";
    public string FullName { get; set; } = "";
    public DateTime BirthDate { get; set; } = DateTime.Today;
    public string Gender { get; set; } = "Nam";
    public string Phone { get; set; } = "";
    public string Department { get; set; } = "";
    public string Diagnosis { get; set; } = "";
}

public sealed class DoctorModel
{
    public string DoctorId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Department { get; set; } = "";
    public DateTime DutyDate { get; set; } = DateTime.Today;
    public string Shift { get; set; } = "Sáng";
}

public sealed class DepartmentModel
{
    public string DepartmentId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Room { get; set; } = "";
}

public sealed class HospitalData
{
    public BindingList<PatientModel> Patients { get; } = new();
    public BindingList<DoctorModel> Doctors { get; } = new();
    public BindingList<DepartmentModel> Departments { get; } = new();
    public HospitalData() => Reset();
    public void Reset()
    {
        Patients.Clear(); Doctors.Clear(); Departments.Clear();
        Departments.Add(new() { DepartmentId = "K001", Name = "Nội tổng hợp", Room = "A101" });
        Departments.Add(new() { DepartmentId = "K002", Name = "Ngoại", Room = "B201" });
        Doctors.Add(new() { DoctorId = "BS001", FullName = "Nguyễn Minh Anh", Department = "Nội tổng hợp", DutyDate = DateTime.Today, Shift = "Sáng" });
        Doctors.Add(new() { DoctorId = "BS002", FullName = "Trần Hoàng Nam", Department = "Ngoại", DutyDate = DateTime.Today, Shift = "Đêm" });
        Patients.Add(new() { PatientId = "BN001", FullName = "Lê Văn Bình", BirthDate = new DateTime(1998, 5, 12), Gender = "Nam", Phone = "0912345678", Department = "Nội tổng hợp", Diagnosis = "Theo dõi huyết áp; tái khám sau 7 ngày." });
        Patients.Add(new() { PatientId = "BN002", FullName = "Nguyễn Ngọc Lan", BirthDate = new DateTime(2000, 8, 20), Gender = "Nữ", Phone = "0987654321", Department = "Ngoại", Diagnosis = "Theo dõi phục hồi sau điều trị." });
    }
}
