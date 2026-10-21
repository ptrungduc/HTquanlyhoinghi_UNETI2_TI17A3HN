<!-- Họ và tên: [TÊN CỦA TÔI] -->
<!-- Mã sinh viên: [MSSV CỦA TÔI] -->
<!-- Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh. -->

# Ghi chú tích hợp Module 4

Module 4 đã có service dùng các DbSet/entity hiện có để xử lý duyệt, từ chối, cấp mã tham dự, check-in và điểm danh phiên. Đây chỉ là trạng thái của source code; chưa xác minh trên SQL Server thực tế. Không tạo entity thay thế, DbSet, quan hệ hay dữ liệu mẫu cho Module 3.

Các phần sau vẫn cần xác nhận hoặc tích hợp trước khi kết luận nghiệp vụ hoạt động đầy đủ:

1. Xác minh trên SQL Server quy tắc duyệt `DangKyThamDu`, trạng thái sự kiện, đăng ký trùng và thời điểm duyệt.
2. Xác nhận quy tắc sức chứa và các trạng thái được tính vào số đăng ký hiệu lực.
3. Bổ sung ràng buộc duy nhất cho `MaThamDu` nếu được chủ sở hữu schema phê duyệt; code hiện kiểm tra trùng trước khi lưu nhưng database chưa có unique index này.
4. Entity `CheckIn` và quan hệ một-một với `DangKyThamDu` đã có trong source/migration; cần xác minh migration đã được áp dụng đúng trên database mục tiêu.
5. Xác nhận ràng buộc chống check-in trùng trên database mục tiêu; source có kiểm tra nghiệp vụ và migration định nghĩa unique index trên `CheckIns.MaDangKy`.
6. Xác minh quy tắc khung thời gian check-in với nghiệp vụ sự kiện.
7. Phối hợp Module 3 xác nhận entity, trạng thái và schema của `DangKyPhien`. Các migration hiện có chưa tạo bảng `DangKyPhiens`, nên điểm danh phiên phụ thuộc database/schema và chưa thể xác nhận hoạt động.
8. Xác nhận entity `PhienSuKien`, trường thời gian và trạng thái hủy với schema được triển khai.
9. Unit test, kiểm thử SQL Server và kiểm thử HTTP/giao diện chưa được xác nhận.

Trước khi áp dụng thay đổi schema, cần xác nhận kiểu khóa, giá trị trạng thái, navigation, DbSet trong `AppDbContext`, phạm vi phân quyền Ban tổ chức và ảnh hưởng migration với người phụ trách. Không tự tạo hoặc áp dụng migration trong quá trình hoàn thiện Module 4.