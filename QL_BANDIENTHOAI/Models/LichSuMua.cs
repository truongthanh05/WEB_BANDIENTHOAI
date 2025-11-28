using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class LichSuMua
    {
        public string Id { get; set; }                 // FK -> KhachHang.Id (PK)
        public DateTime? NgayMuaHang { get; set; }

        public KhachHang KhachHang { get; set; }
    }
}