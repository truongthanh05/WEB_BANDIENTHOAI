using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class ChiTietGioHang
    {
        public string Id { get; set; }
        public string Matk { get; set; }
        public string MaLoai { get; set; }
        public string MaSp { get; set; }
        public int? SoLuong { get; set; }
        public double? DonGia { get; set; }

        public GioHang GioHang { get; set; }
        public SanPham SanPham { get; set; }
    }
}