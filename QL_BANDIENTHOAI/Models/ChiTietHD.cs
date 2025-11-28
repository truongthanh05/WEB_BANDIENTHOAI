using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class ChiTietHD
    {
        public string MaHd { get; set; }
        public string MaLoai { get; set; }
        public string MaSp { get; set; }
        public int? SoLuong { get; set; }
        public double? DonGia { get; set; }

        public HoaDon HoaDon { get; set; }
        public SanPham SanPham { get; set; }
    }
}