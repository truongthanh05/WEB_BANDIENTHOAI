using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class NhanVien
    {
        public string Id { get; set; }
        public string ChucVu { get; set; }
        public DateTime? NgayVaoLam { get; set; }

        public NguoiDung NguoiDung { get; set; }
        public ICollection<PhieuNhap> PhieuNhaps { get; set; }

        public NhanVien()
        {
            PhieuNhaps = new List<PhieuNhap>();
        }
    }
}