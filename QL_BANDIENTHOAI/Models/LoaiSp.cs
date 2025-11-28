using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class LoaiSp
    {
        public string MaLoai { get; set; }
        public string TenLoai { get; set; }

        public ICollection<SanPham> SanPhams { get; set; }

        public LoaiSp()
        {
            SanPhams = new List<SanPham>();
        }
    }
}