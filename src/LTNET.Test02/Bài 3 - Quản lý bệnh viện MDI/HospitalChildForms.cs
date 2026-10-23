namespace LTNET.Test02.Hospital;

public sealed class frmPatientProfiles : RecordEditorForm<PatientModel>
{
    public frmPatientProfiles(HospitalData data) : base("Hồ sơ bệnh nhân", data.Patients, nameof(PatientModel.PatientId), "BN",
        new(nameof(PatientModel.FullName), "Họ tên"),
        new(nameof(PatientModel.BirthDate), "Ngày sinh", IsBirthDate: true),
        new(nameof(PatientModel.Gender), "Giới tính", ["Nam", "Nữ", "Khác"]),
        new(nameof(PatientModel.Phone), "Điện thoại", IsPhone: true),
        new(nameof(PatientModel.Department), "Khoa/Phòng"),
        new(nameof(PatientModel.Diagnosis), "Bệnh án")) { Name = "frmPatientProfiles"; }
}

public sealed class frmDoctorSchedule : RecordEditorForm<DoctorModel>
{
    public frmDoctorSchedule(HospitalData data) : base("Danh mục bác sĩ & lịch trực", data.Doctors, nameof(DoctorModel.DoctorId), "BS",
        new(nameof(DoctorModel.FullName), "Tên bác sĩ"),
        new(nameof(DoctorModel.Department), "Khoa/Phòng"),
        new(nameof(DoctorModel.DutyDate), "Ngày trực"),
        new(nameof(DoctorModel.Shift), "Ca trực", ["Sáng", "Chiều", "Đêm"])) { Name = "frmDoctorSchedule"; }
}

public sealed class frmDepartmentCatalog : RecordEditorForm<DepartmentModel>
{
    public frmDepartmentCatalog(HospitalData data) : base("Danh mục Khoa/Phòng", data.Departments, nameof(DepartmentModel.DepartmentId), "K",
        new(nameof(DepartmentModel.Name), "Tên khoa"),
        new(nameof(DepartmentModel.Room), "Phòng")) { Name = "frmDepartmentCatalog"; }
}
