using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class PhieuKhuyenMai
    {
        public string MaPKM { get; set; }
        public string LoaiPhieu { get; set; }
        public int GiaTri { get; set; }
        public DateTime NgayHetHan { get; set; }
        public string DieuKien { get; set; }
        public int SoLuong { get; set; }
        public string TrangThai { get; set; }
    }
}