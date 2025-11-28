using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class GiaoHang
    {
        public string MaHd { get; set; }               // PK & FK -> HoaDon
        public string TrangThai { get; set; }
        public DateTime? NgayGiao { get; set; }

        public HoaDon HoaDon { get; set; }
    }
}