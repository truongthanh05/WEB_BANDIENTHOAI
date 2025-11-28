using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class PhieuNhap
    {
        public string MaPhieuNhap { get; set; }
        public string Id { get; set; }                 // FK -> NhanVien.Id
        public DateTime? NgayNhap { get; set; }
        public string NhaCungCap { get; set; }

        public NhanVien NhanVien { get; set; }
        public ICollection<ChiTietPN> ChiTietPns { get; set; }

        public PhieuNhap()
        {
            ChiTietPns = new List<ChiTietPN>();
        }
    }
}