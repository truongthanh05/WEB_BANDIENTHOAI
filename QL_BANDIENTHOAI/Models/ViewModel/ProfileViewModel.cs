using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models.ViewModel
{
    public class ProfileViewModel
    {
        public string HoTen { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public DateTime NgayTao { get; set; }
        public int SoDonHang { get; set; }
        public int SoVoucher { get; set; }
        public double TongTienDaMua { get; set; }
    }
}