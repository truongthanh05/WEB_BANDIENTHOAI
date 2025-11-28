using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class HoaDon
    {
        public string MaHd { get; set; }
        public string Id { get; set; }                 // FK -> KhachHang.Id
        public double? ThanhTien { get; set; }
        public DateTime? NgayLap { get; set; }

        public KhachHang KhachHang { get; set; }
        public ICollection<ChiTietHD> ChiTietHds { get; set; }
        public GiaoHang GiaoHang { get; set; }         // 1-1
        public DanhGia DanhGia { get; set; }           // 1-1

        public HoaDon()
        {
            ChiTietHds = new List<ChiTietHD>();
        }
    }
}