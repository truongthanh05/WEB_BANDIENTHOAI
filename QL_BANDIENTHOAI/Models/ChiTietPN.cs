using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class ChiTietPN
    {
        public string MaPhieuNhap { get; set; }
        public string MaLoai { get; set; }
        public string MaSp { get; set; }
        public int? SoLuong { get; set; }
        public double? GiaNhap { get; set; }

        public PhieuNhap PhieuNhap { get; set; }
        public SanPham SanPham { get; set; }
    }
}