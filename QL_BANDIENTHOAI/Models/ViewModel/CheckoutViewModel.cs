using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models.ViewModel
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public double TongTien { get; set; }
        public int SoLuongSP { get; set; }
        public string HoTen { get; set; } = "";
        public string SDT { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public List<PhieuKhuyenMai> Vouchers { get; set; } = new List<PhieuKhuyenMai>();
    }
}