// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Services;

public class DichVuModule4 : IDichVuModule4
{
    private const string ThongBaoChuaTichHop = "Chưa tích hợp Module 3.";

    public Task<KetQuaThaoTacViewModel> DuyetDangKyAsync(string maDangKy)
    {
        return Task.FromResult(KetQuaChuaTichHop());
    }

    public Task<KetQuaThaoTacViewModel> TuChoiDangKyAsync(string maDangKy, string lyDoTuChoi)
    {
        return Task.FromResult(KetQuaChuaTichHop());
    }

    public Task<KetQuaThaoTacViewModel> CheckInAsync(string maThamDu, string? ghiChu)
    {
        return Task.FromResult(KetQuaChuaTichHop());
    }

    private static KetQuaThaoTacViewModel KetQuaChuaTichHop()
    {
        return new KetQuaThaoTacViewModel
        {
            ThanhCong = false,
            ThongBao = ThongBaoChuaTichHop
        };
    }
}