using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class NguoiDung
    {
        public string Id { get; set; }
        public string HoTen { get; set; }
        public string Sdt { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string GioiTinh { get; set; }
        public string DanToc { get; set; }

        // Navs 1-1 / 1-n
        public KhachHang KhachHang { get; set; }
        public NhanVien NhanVien { get; set; }
        public ICollection<TaiKhoan> TaiKhoans { get; set; }

        public NguoiDung()
        {
            TaiKhoans = new List<TaiKhoan>();
        }
    }
}