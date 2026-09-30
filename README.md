# GarageAI — Đồ án quản lý garage ô tô

## Chạy nhanh
Yêu cầu Windows 10/11 và .NET SDK 10. Mở thư mục này bằng Visual Studio hỗ trợ .NET 10 hoặc VS Code.

```powershell
dotnet restore --configfile NuGet.Config
dotnet run --no-launch-profile --urls http://localhost:5080
```
Mở http://localhost:5080. Hoặc nhấp đúp CHAY-DU-AN.cmd.

Tài khoản: admin / letan / kythuat / thungan. Mật khẩu chung: Garage@123.
Vai trò tương ứng: quản lý / lễ tân / kỹ thuật viên / thu ngân.

## Chức năng
- Đăng nhập cookie, mật khẩu băm, phân quyền API.
- Thêm khách hàng, xe; tìm tên, điện thoại, biển số.
- Đặt/hủy lịch, tiếp nhận xe, phân công kỹ thuật viên.
- Phiếu sửa chữa: tiếp nhận → đang sửa → hoàn tất; hạng mục, số lượng, ghi chú.
- Bảng giá dịch vụ, phụ tùng; nhập tồn kho; tự trừ khi hoàn tất, chặn âm kho.
- Hóa đơn từ phiếu hoàn tất, chặn xuất trùng, ghi nhận thanh toán, in.
- Thống kê thực thu, công nợ, dịch vụ/phụ tùng sử dụng nhiều.
- Tóm tắt lịch sử, giải thích dịch vụ, báo giá nháp.

## AI
Mặc định `AI:Provider=Demo`: dựng nội dung theo mẫu, KHÔNG gọi mô hình AI.
Để dùng AI thật: cài Ollama, tải mô hình bằng `ollama pull qwen2.5:3b`, khởi động Ollama. Sửa `AI:Provider` thành `Ollama` trong appsettings.json rồi chạy lại. API mặc định http://localhost:11434/api/chat. Không cần API key. Kết nối AI thật chưa được thử trên máy này.
Prompt yêu cầu chỉ dùng dữ liệu cung cấp, không chẩn đoán lỗi, không sửa giá. Nhân viên phải duyệt nội dung trước khi dùng.

## Kiến trúc và dữ liệu
ASP.NET Core 10 Minimal API + HTML/CSS/JavaScript; SQLite tích hợp Windows qua winsqlite3.
Program.cs: API, xác thực, nghiệp vụ, AI. Models.cs: mô hình và lưu trữ. wwwroot: giao diện.
App_Data/garage.db được tạo tự động với dữ liệu mẫu. Đây là bản cơ bản: SQLite lưu toàn bộ trạng thái dưới dạng JSON trong một hàng GarageState, chưa chuẩn hóa bảng quan hệ; các liên kết và nghiệp vụ được kiểm tra ở ứng dụng. Khóa đồng bộ và một lần ghi SQLite giúp cập nhật phiếu/kho nguyên tử trong một tiến trình. Không triển khai nhiều tiến trình chung DB.
Quan hệ logic: Khách hàng 1—n Xe; Xe 1—n Lịch hẹn/Phiếu; Nhân viên 1—n Phiếu; Phiếu 1—n Chi tiết; Phiếu 1—0..1 Hóa đơn; Chi tiết tham chiếu danh mục và chụp đơn giá.
Bản này chạy Windows; chưa hỗ trợ Linux. Chưa có sửa/xóa khách hàng, quản trị tài khoản, nhật ký kiểm toán, phân trang hoặc hóa đơn thuế.

## Kịch bản trình bày
1. Đăng nhập admin; tạo khách và xe.
2. Đặt lịch → tiếp nhận; chọn kỹ thuật viên.
3. Vào phiếu, thêm dịch vụ và phụ tùng; lưu Đang sửa chữa.
4. Tạo giải thích dịch vụ/báo giá; lưu Hoàn tất để trừ kho.
5. Lập hóa đơn, thu tiền, in; xem báo cáo và lịch sử xe.
6. Đăng nhập letan để minh họa phân quyền.

## Nộp mã nguồn
Nộp ZIP đi kèm. Muốn có link công khai: tạo repository GitHub rồi tải các file nguồn lên; không tải App_Data, bin, obj hay khóa bảo vệ dữ liệu. Mã nguồn: https://github.com/dtc245200338-ship-it/H-thong-sua-chua-o-to
localhost chỉ mở trên máy đang chạy, không phải link công khai cho giáo viên. Muốn triển khai cần máy chủ Windows/.NET phù hợp và cấu hình HTTPS, tài khoản riêng, sao lưu và AllowedHosts. Không đưa tài khoản demo lên Internet.

## Tài liệu tham khảo
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0
- https://github.com/ollama/ollama/blob/main/docs/api/introduction.mdx
