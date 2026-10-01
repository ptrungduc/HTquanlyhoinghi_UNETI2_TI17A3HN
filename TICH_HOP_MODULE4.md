<!-- Họ và tên: [TÊN CỦA TÔI] -->
<!-- Mã sinh viên: [MSSV CỦA TÔI] -->
<!-- Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh. -->

# Ghi chú tích hợp Module 4

Module 4 hiện chưa kết nối database. Không tạo entity thay thế, DbSet, quan hệ hay dữ liệu mẫu cho Module 3. Các thao tác duyệt, từ chối và check-in trả về thông báo chưa tích hợp, không báo thành công.

Các phần sau đang chờ entity thật và quy tắc nghiệp vụ của Module 3:

1. Duyệt `DangKyThamDu` thật, gồm trạng thái, trạng thái sự kiện, quy tắc đăng ký trùng và thời điểm duyệt.
2. Kiểm tra sức chứa thật và quy tắc đếm số đăng ký được tính vào sức chứa.
3. Sinh và lưu `MaThamDu` duy nhất trên đăng ký thật nếu entity chưa có thuộc tính này.
4. Tạo entity `CheckIn` của Module 4 sau khi xác nhận kiểu khóa đăng ký và quan hệ.
5. Cấu hình khóa ngoại từ `CheckIn` tới `DangKyThamDu` dựa trên khóa và navigation thật.
6. Ngăn check-in trùng trong nghiệp vụ và bằng ràng buộc duy nhất phù hợp trong database.
7. Kiểm tra khung thời gian check-in nếu hệ thống hiện có quy định.
8. Đọc entity `DangKyPhien` thật và quan hệ đăng ký/điểm danh phiên.
9. Đọc entity `PhienSuKien` thật và các trường thời gian trước khi làm điểm danh phiên.
10. Chỉ triển khai điểm danh phiên nếu contract phiên và đăng ký hỗ trợ, với tối đa một bản ghi hợp lệ cho mỗi đăng ký/phiên.

Trước khi kết nối, cần xác nhận kiểu khóa, giá trị trạng thái, navigation, DbSet trong `AppDbContext`, phạm vi phân quyền Ban tổ chức và ảnh hưởng migration. Không tạo migration trước khi đọc các contract này.