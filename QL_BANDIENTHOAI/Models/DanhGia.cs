using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class DanhGia
    {
        public string MaHd { get; set; }               // PK & FK -> HoaDon
        public int? SoSao { get; set; }
        public string GhiChu { get; set; }

        public HoaDon HoaDon { get; set; }
    }
}