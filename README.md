# BÁO CÁO BÀI TẬP THỰC HÀNH LẬP TRÌNH C# WINFORMS

## I. THÔNG TIN SINH VIÊN

* **Họ và tên:** Hoàng Nghĩa Bảo
* **Mã số sinh viên:** 24810320182
* **Lớp học phần:** Lập trình .NET - Mã học phần: 0101004755

* **Link Repository:** [https://github.com/nghiabao2006-commits/LTNET-TEST02_24810320182_HoangNghiaBao](https://github.com/nghiabao2006-commits/LTNET-TEST02_24810320182_HoangNghiaBao)

---

## II. DANH SÁCH BÀI TẬP HOÀN THÀNH & TỰ ĐÁNH GIÁ

| Bài tập | Tiêu đề bài tập | Trạng thái | Mức độ hoàn thành (%) |
| :---: | :--- | :---: | :---: |
| **Bài 1** | Quản lý Dịch vụ Phòng Gym & Hội viên | Hoàn thành | 99% |
| **Bài 2** | Báo cáo & Lưu trữ Tài liệu Nhà sách | Hoàn thành | 99% |
| **Bài 3** | Hệ thống MDI Quản lý Bệnh viện & Async | Hoàn thành | 99% |
| **Bài 4** | BẢNG ĐIỀU KHIỂN THỐNG KÊ DOANH THU  | Hoàn thành | 99% |
| **Bài 5** | MÔ PHỎNG GIÁM SÁT TỶ GIÁ TÀI CHÍNH | Hoàn thành | 99% |


---

## III. MINH HỌA GIAO DIỆN & KỸ THUẬT NỔI BẬT

### 1. Bài 1: Quản lý Dịch vụ Phòng Gym & Hội viên
* **Kỹ thuật áp dụng:** Cấu hình DataGridView bằng code (`AutoGenerateColumns =
false`), Data Binding 2 chiều với `BindingSource`, Validation với `ErrorProvider`.
* **Hình ảnh minh họa Giao diện:**

![Giao diện chính Bài 1](docs/bai1giaodien.PNG)

*Mô tả: Giao diện chính hiển thị DataGridView và đồng bộ dữ liệu 2 chiều lên các
TextBox.*

![Bắt lỗi Validation Bài 1](docs/bai1valid.PNG)

*Mô tả: Kiểm tra hợp lệ dữ liệu nhập vào bằng ErrorProvider khi nhấn Thêm mới.*

---

### 2. Bài 2: Quản lý & Xuất Báo cáo Nhà sách
* **Kỹ thuật áp dụng:** `OpenFileDialog` (Tải ảnh bìa), `SaveFileDialog` /
`OpenFileDialog` (Đọc/Ghi file CSV), Cập nhật `StatusStrip` theo thời gian thực.
* **Hình ảnh minh họa Giao diện:**

![Giao diện Bài 2 và Load Ảnh](docs/bai2thaotacanhbia.PNG)
*Mô tả: Chọn ảnh bìa sách từ đĩa cứng hiển thị lên PictureBox và nạp dữ liệu từ file
CSV.*

![Giao diện Bài 2 và Xuất file CSV](docs/bai2xuatfile.PNG)
*Mô tả: Xuất file CSV.*
---

### 3. Bài 3: HỆ THỐNG MDI QUẢN LÝ BỆNH VIỆN & TRUY XUẤT HỒ SƠ BỆNH ÁN
* **Kỹ thuật áp dụng:** MDI Container, Quản lý chống mở trùng Form con,
`async/await` chống treo UI, Safe Cross-Thread bằng `this.Invoke()`.
* **Hình ảnh minh họa Giao diện:**

![Giao diện danh mục bác sĩ và lịch trực](docs/bai3giaodiendanhmucbacsivalichtruc.PNG)

![Giao diện danh mục khoa\phòng](docs/bai3giaodiendanhmuckhoaphong.PNG)

![Giao diện hồ sơ bênh nhân](docs/bai3giaodienhosobenhnhan.PNG)

![Giao diện Khung MDI](docs/bai3xeptang.PNG)
*Mô tả: Khung Form cha MDI điều khiển mở các Form con xếp tầng.*

### 4. Bài 4: BẢNG ĐIỀU KHIỂN THỐNG KÊ DOANH THU MULTI- THREADING (ASYNC/AWAIT & KHÔNG KHÓA UI)

![Giao diện thống kê doanh thu](docs/bai4giaodienthongkedoanhthu.PNG)

![Giao diện huỷ thao tác](docs/bai4huythaotac.PNG)

---

### 5. Bài 5: MÔ PHỎNG GIÁM SÁT TỶ GIÁ TÀI CHÍNH THỜI GIAN THỰC (CROSS-THREAD SAFETY & PROCESSING)

![Giao diện giám sát BTC](docs/bai5giamsatbtc.PNG)

![Giao diện tạm dừng giám sát](docs/bai5giaodientamdung.PNG)

---
## IV. HƯỚNG DẪN CÀI ĐẶT & CHẠY DỰ ÁN
1. Clone Repository về máy:
```bash

git clone [https://github.com/nghiabao2006-commits/LTNET-TEST02_24810320182_HoangNghiaBao](https://github.com/nghiabao2006-commits/LTNET-TEST02_24810320182_HoangNghiaBao)

2. Mở file solution src/WinFormsAppSolution.sln bằng Visual Studio 2022.
3. Bấm F5 hoặc chọn Start để biên dịch và chạy chương trình.
