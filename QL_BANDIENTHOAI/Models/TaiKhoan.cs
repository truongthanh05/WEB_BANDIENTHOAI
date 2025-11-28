using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class TaiKhoan
    {
        public string MATK { get; set; }     // Mã tài khoản
        public string ID { get; set; }       // Mã người dùng
        public string TENTK { get; set; }    // Tên tài khoản
        public string MATKHAU { get; set; }  // Mật khẩu
        public string ROLE { get; set; }     // Vai trò
        public string TRANGTHAI { get; set; } // Trạng thái tài khoản
        public string ConfirmPassword { get; set; }
        public string Email { get; set; }    // Email người dùng
        public string PhoneNumber { get; set; }
        public NguoiDung NguoiDung { get; set; }
        public ICollection<GioHang> GioHangs { get; set; }

        public TaiKhoan()
        {
            GioHangs = new List<GioHang>();
        }
    }
}