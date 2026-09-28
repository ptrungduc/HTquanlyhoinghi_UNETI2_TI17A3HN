using System.Security.Cryptography;
using System.Text;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Helper
{
    public class MatKhauHelper
    {
        public static string MaHoa(string matkhau)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(matkhau));
            return Convert.ToHexString(bytes);
        }
    }
}