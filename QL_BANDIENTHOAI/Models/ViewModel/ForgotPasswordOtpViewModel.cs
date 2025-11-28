using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models.ViewModel
{
    public class ForgotPasswordOtpViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Otp { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        // Cờ xác định xem có cần gửi OTP hay không
        public bool OtpSent { get; set; } = false;
    }

}