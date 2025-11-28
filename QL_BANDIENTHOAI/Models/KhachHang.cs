using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class KhachHang
    {
        public string Id { get; set; }
        public int? Diem { get; set; }

        public NguoiDung NguoiDung { get; set; }
        public ICollection<HoaDon> HoaDons { get; set; }
        public ICollection<LichSuMua> LichSuMuas { get; set; }

        public KhachHang()
        {
            HoaDons = new List<HoaDon>();
            LichSuMuaS = new List<LichSuMua>();
        }

        // Fix typo property name for older code that may expect this:
        public ICollection<LichSuMua> LichSuMuaS
        {
            get { return LichSuMuas; }
            set { LichSuMuas = value; }
        }

        public string DienThoai { get; internal set; }
        public string HoTen { get; internal set; }
        public string DiaChi { get; internal set; }
    }
}