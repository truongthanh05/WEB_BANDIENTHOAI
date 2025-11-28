using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class GioHang
    {
        public string Id { get; set; }
        public string Matk { get; set; }
        public int? TongSanPham { get; set; }
        public double? TongTien { get; set; }

        public TaiKhoan TaiKhoan { get; set; }
        public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }

        public GioHang()
        {
            ChiTietGioHangs = new List<ChiTietGioHang>();
        }
    }
}