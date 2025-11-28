using System.Collections.Generic;

namespace QL_BANDIENTHOAI.Models
{
    public class SanPham
    {
        public string MaSp { get; set; }
        public string MaLoai { get; set; }
        public string TenSp { get; set; }
        public double? GiaBan { get; set; }
        public string MoTa { get; set; }

        public string Ram { get; set; }
        public string Rom { get; set; }
        public string Os { get; set; }
        public string Chipset { get; set; }
        public string Gpu { get; set; }
        public string Camera { get; set; }
        public string Pin { get; set; }
        public string ManHinh { get; set; }
        public string KichThuoc { get; set; }
        public string TrongLuong { get; set; }
        public string SimCard { get; set; }
        public string AnhSanPham { get; set; }

        public LoaiSp LoaiSp { get; set; }

        // Quan hệ 1–N
        public ICollection<ChiTietHD> ChiTietHds { get; set; }
        public ICollection<ChiTietPN> ChiTietPNs { get; set; }
        public ICollection<ChiTietGioHang> ChiTietGHs { get; set; }
        public ICollection<TonKho> TonKhos { get; set; }

        public SanPham()
        {
            ChiTietHds = new List<ChiTietHD>();
            ChiTietPNs = new List<ChiTietPN>();
            ChiTietGHs = new List<ChiTietGioHang>();
            TonKhos = new List<TonKho>();
        }
    }
}
