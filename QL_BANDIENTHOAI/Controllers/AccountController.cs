using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using QL_BANDIENTHOAI.Models;
using QL_BANDIENTHOAI.Models.ViewModel;

using System.Net;
using System.Net.Mail;
using System.Text;
using System.Configuration;


namespace QL_BANDIENTHOAI.Controllers
{
    public class AccountController : Controller
    {

        // GET: Account
        [HttpGet, AllowAnonymous]
        public ActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Login(TaiKhoan model, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(model.TENTK) || string.IsNullOrWhiteSpace(model.MATKHAU))
            {
                ViewBag.Error = "Vui lòng nhập Tên tài khoản và Mật khẩu.";
                return View();
            }

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString))
                {
                    conn.Open();

                    // Mã hóa mật khẩu người dùng nhập vào
                    string hashedPassword = HashPassword(model.MATKHAU?.Trim());

                    const string sql = @"
                SELECT 
                    ISNULL(tk.TRANGTHAI, 'PENDING') AS TrangThai,
                    tk.ROLE
                FROM TAIKHOAN tk
                WHERE tk.TENTK = @pUser
                  AND tk.MATKHAU = @pPass";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@pUser", model.TENTK?.Trim());
                        cmd.Parameters.AddWithValue("@pPass", hashedPassword);  // So sánh mật khẩu đã mã hóa

                        using (var r = cmd.ExecuteReader())
                        {
                            if (!r.Read())
                            {
                                ViewBag.Error = "Sai tài khoản hoặc mật khẩu.";
                                return View();
                            }

                            var status = r.GetString(0)?.Trim();
                            var role = r.GetString(1)?.Trim();

                            // Kiểm tra trạng thái tài khoản
                            if (status != "ACTIVE")
                            {
                                ViewBag.Error = "Tài khoản không hoạt động hoặc bị khóa.";
                                return View();
                            }

                            // Lưu thông tin vào session
                            Session["User"] = model.TENTK?.Trim();
                            Session["Role"] = role;

                            // Điều hướng dựa trên vai trò
                            if (role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
                            {
                                // Nếu là Admin, điều hướng đến trang Admin
                                return RedirectToAction("Home", "Admin");
                            }

                            // Nếu không phải Admin, điều hướng về trang Index của trang chủ
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return View();
            }
        }




        // Hàm hash mật khẩu trực tiếp trên code (SHA-256)
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hashedBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            // Xoá thông tin đăng nhập trên Session
            Session.Clear();
            Session.Abandon();
            TempData["Msg"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Login", "Account");
        }

        // ================== HÀM KIỂM TRA ROLE ==================
        private bool IsDoctor(string staffType)
        {
            return string.Equals(staffType, "BacSi", StringComparison.OrdinalIgnoreCase);
        }

        // ================== REGISTER ==================
        [HttpGet, AllowAnonymous]
        public ActionResult Register()
        {
            var model = new TaiKhoan { ROLE = "USER", TRANGTHAI = "ACTIVE" }; // Khởi tạo vai trò mặc định và trạng thái tài khoản
            return View(model);
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Register(TaiKhoan model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kiểm tra trùng tên tài khoản
            if (string.IsNullOrWhiteSpace(model.TENTK) || string.IsNullOrWhiteSpace(model.MATKHAU))
            {
                ModelState.AddModelError("", "Vui lòng nhập Tên tài khoản và Mật khẩu.");
                return View(model);
            }

            if (!string.Equals(model.MATKHAU?.Trim(), model.ConfirmPassword?.Trim()))
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu và xác nhận mật khẩu không khớp.");
                return View(model);
            }

            model.ROLE = "USER";
            model.TRANGTHAI = "ACTIVE";

            try
            {
                var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"]?.ConnectionString;
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();

                    using (var tx = conn.BeginTransaction())
                    {
                        try
                        {
                            // Kiểm tra xem tên tài khoản đã tồn tại trong cơ sở dữ liệu chưa
                            using (var cmdCheck = new SqlCommand("SELECT COUNT(*) FROM TAIKHOAN WHERE TENTK = @u", conn))
                            {
                                cmdCheck.Transaction = tx;
                                cmdCheck.Parameters.AddWithValue("@u", model.TENTK?.Trim());

                                if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0)
                                {
                                    ModelState.AddModelError("TENTK", "Tên tài khoản đã tồn tại trong hệ thống.");
                                    tx.Rollback();
                                    return View(model);
                                }
                            }

                            // Sinh mã người dùng tự động
                            string nguoiDungId = NextId(conn, tx, "NGUOIDUNG", "ID", "ND");

                            // Insert thông tin vào bảng NGUOIDUNG
                            using (var cmdNguoiDung = new SqlCommand(@"
                        INSERT INTO NGUOIDUNG
                            (ID, HOTEN, SDT, EMAIL, DIACHI, GIOITINH, DANTOC)
                        VALUES
                            (@nguoiDungId, @hoTen, @sdt, @email, @diaChi, @gioiTinh, @danToc)", conn))
                            {
                                cmdNguoiDung.Transaction = tx;
                                cmdNguoiDung.Parameters.AddWithValue("@nguoiDungId", nguoiDungId);
                                cmdNguoiDung.Parameters.AddWithValue("@hoTen", model.TENTK);  // Hoặc thông tin khác từ form
                                cmdNguoiDung.Parameters.AddWithValue("@sdt", model.PhoneNumber);
                                cmdNguoiDung.Parameters.AddWithValue("@email", model.Email);
                                cmdNguoiDung.Parameters.AddWithValue("@diaChi", ""); // Thêm thông tin địa chỉ nếu cần
                                cmdNguoiDung.Parameters.AddWithValue("@gioiTinh", ""); // Thêm thông tin giới tính nếu cần
                                cmdNguoiDung.Parameters.AddWithValue("@danToc", ""); // Thêm thông tin dân tộc nếu cần

                                cmdNguoiDung.ExecuteNonQuery();
                            }

                            // Sinh mã tài khoản tự động
                            string tkId = NextId(conn, tx, "TAIKHOAN", "MaTK", "TK");

                            // Mã hóa mật khẩu trước khi lưu vào cơ sở dữ liệu
                            string hashedPassword = HashPassword(model.MATKHAU?.Trim());

                            // Insert vào bảng TAIKHOAN
                            using (var cmdTk = new SqlCommand(@"
                                INSERT INTO TAIKHOAN
                                    (MaTK, TENTK, MATKHAU, ROLE, TRANGTHAI, ID, NGAYTAO)
                                VALUES
                                    (@tkId, @username, @password, @role, @status, @nguoiDungId, GETDATE())", conn))
                                                        {
                                cmdTk.Transaction = tx;
                                cmdTk.Parameters.AddWithValue("@tkId", tkId);
                                cmdTk.Parameters.AddWithValue("@username", model.TENTK?.Trim());
                                cmdTk.Parameters.AddWithValue("@password", hashedPassword);  // Mã hóa mật khẩu trước khi lưu
                                cmdTk.Parameters.AddWithValue("@role", model.ROLE);  // Role đã gán là "USER"
                                cmdTk.Parameters.AddWithValue("@status", model.TRANGTHAI);  // Trạng thái tài khoản
                                cmdTk.Parameters.AddWithValue("@nguoiDungId", nguoiDungId); // Liên kết với bảng NGUOIDUNG

                                cmdTk.ExecuteNonQuery();
                            }


                            // Thêm bản ghi vào bảng GIOHANG
                            using (var cmdGioHang = new SqlCommand(@"
                        INSERT INTO GIOHANG
                            (MATK, TONGSANPHAM, TONGTIEN)
                        VALUES
                            (@tkId, 0, 0)", conn))
                            {
                                cmdGioHang.Transaction = tx;
                                cmdGioHang.Parameters.AddWithValue("@tkId", tkId);
                                cmdGioHang.ExecuteNonQuery();
                            }

                            tx.Commit();
                            TempData["Msg"] = "Tài khoản của bạn đã được tạo thành công.";
                            return RedirectToAction("Login");
                        }
                        catch (Exception ex)
                        {
                            tx.Rollback();
                            ModelState.AddModelError("", "Lỗi khi lưu tài khoản: " + ex.Message);
                            return View(model);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
                return View(model);
            }
        }




        // Sinh mã kế tiếp
        private static string NextId(SqlConnection conn, SqlTransaction tx, string table, string idColumn, string prefix)
        {
            using (var lockCmd = new SqlCommand($"SELECT MAX(CAST(SUBSTRING({idColumn}, 3, 8) AS INT)) FROM {table}", conn))
            {
                lockCmd.Transaction = tx;
                var result = lockCmd.ExecuteScalar();
                int nextVal = (result != DBNull.Value ? Convert.ToInt32(result) : 0) + 1;
                return prefix + nextVal.ToString("D8");
            }
        }

        [HttpGet, AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            // Trả về model rỗng để view binding
            return View(new ForgotPasswordOtpViewModel());
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordOtpViewModel model)
        {
            bool isVerifyPhase = !string.IsNullOrWhiteSpace(model.Otp);  // Kiểm tra nếu đang ở bước xác thực OTP

            // Validate tài khoản và email
            if (string.IsNullOrWhiteSpace(model.UserName))
                ModelState.AddModelError("UserName", "Vui lòng nhập tài khoản.");

            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError("Email", "Vui lòng nhập email đã đăng ký.");

            // Kiểm tra mật khẩu mới nếu đang ở bước xác thực OTP
            if (isVerifyPhase)
            {
                if (string.IsNullOrWhiteSpace(model.NewPassword))
                    ModelState.AddModelError("NewPassword", "Vui lòng nhập mật khẩu mới.");

                if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
                    ModelState.AddModelError("ConfirmPassword", "Vui lòng xác nhận mật khẩu mới.");

                if (!string.Equals(model.NewPassword?.Trim(), model.ConfirmPassword?.Trim()))
                    ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
            }

            if (!ModelState.IsValid)
            {
                model.OtpSent = isVerifyPhase || model.OtpSent;  // Nếu OTP đã gửi, vẫn hiển thị ô OTP
                return View(model);
            }

            try
            {
                var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();

                    // Kiểm tra tài khoản với tên người dùng và email
                    const string sqlFind = @"
                SELECT tk.MATK, nd.EMAIL
                FROM TAIKHOAN tk
                JOIN NGUOIDUNG nd ON nd.ID = tk.ID
                WHERE tk.TENTK = @UserName AND nd.EMAIL = @Email";

                    string tkId = null;
                    string encEmailDb = null;

                    using (var cmd = new SqlCommand(sqlFind, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", model.UserName.Trim());
                        cmd.Parameters.AddWithValue("@Email", model.Email.Trim());

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tkId = reader.GetString(0);   // Lấy mã tài khoản
                                encEmailDb = reader.GetString(1); // Lấy email mã hóa từ DB
                                Console.WriteLine($"Account found: {tkId}, Email: {encEmailDb}");
                            }
                        }
                    }

                    if (tkId == null)
                    {
                        ViewBag.Error = "Không tìm thấy tài khoản với Username và Email này.";
                        model.OtpSent = false;
                        return View(model);
                    }

                    // Giải mã email
                  //  string emailFromDb = DecryptEmail(encEmailDb);

                    // So sánh email từ cơ sở dữ liệu và email người dùng nhập
                    if (!string.Equals(encEmailDb, model.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        ViewBag.Error = "Email không chính xác.";
                        model.OtpSent = false;
                        return View(model);
                    }

                    // ================= BƯỚC 1: GỬI OTP =================
                    if (!isVerifyPhase)
                    {
                        string otp = GenerateOtpCode(6);  // Tạo mã OTP
                        DateTime expire = DateTime.UtcNow.AddMinutes(5);  // OTP hết hạn sau 5 phút

                        // Lưu OTP vào Session theo TK_MaTK
                        Session["FP_OTP_" + tkId] = otp;
                        Session["FP_OTP_EXP_" + tkId] = expire;

                        // Gửi OTP qua email
                        SendOtpEmail(model.Email, model.UserName, otp);

                        ViewBag.Info = "Đã gửi mã OTP đến email của bạn. Vui lòng kiểm tra và nhập OTP để xác nhận đổi mật khẩu.";
                        model.OtpSent = true;  // Để View hiển thị ô OTP
                        return View(model);
                    }

                    // ================= BƯỚC 2: XÁC THỰC OTP =================
                    string otpInSession = Session["FP_OTP_" + tkId] as string;
                    DateTime? exp = (DateTime?)Session["FP_OTP_EXP_" + tkId];

                    // Kiểm tra OTP trong session và thời gian hết hạn
                    if (string.IsNullOrEmpty(otpInSession) || !exp.HasValue)
                    {
                        ViewBag.Error = "Mã OTP không tồn tại hoặc đã hết hạn.";
                        model.OtpSent = true;
                        return View(model);
                    }

                    if (DateTime.UtcNow > exp.Value)
                    {
                        ViewBag.Error = "Mã OTP đã hết hạn.";
                        model.OtpSent = true;
                        return View(model);
                    }

                    if (!string.Equals(model.Otp, otpInSession))
                    {
                        ViewBag.Error = "Mã OTP không chính xác.";
                        model.OtpSent = true;
                        return View(model);
                    }

                    // Mã OTP hợp lệ, tiến hành thay đổi mật khẩu
                    string hashedPassword = HashPassword(model.NewPassword);

                    const string sqlUpdatePassword = @"
                UPDATE TAIKHOAN
                SET MATKHAU = @NewPassword
                WHERE MATK = @TKId";

                    using (var cmd = new SqlCommand(sqlUpdatePassword, conn))
                    {
                        cmd.Parameters.AddWithValue("@NewPassword", hashedPassword);
                        cmd.Parameters.AddWithValue("@TKId", tkId);
                        cmd.ExecuteNonQuery();
                    }

                    // Xoá OTP khỏi Session sau khi sử dụng
                    Session.Remove("FP_OTP_" + tkId);
                    Session.Remove("FP_OTP_EXP_" + tkId);

                    TempData["Msg"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                model.OtpSent = true;
                return View(model);
            }
        }




        // ================== HÀM HỖ TRỢ OTP & EMAIL ==================

        private string GenerateOtpCode(int length)
        {
            var rnd = new Random();
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(rnd.Next(0, 10)); // chỉ 0-9 => mã số
            }
            return sb.ToString();
        }

        private void SendOtpEmail(string toEmail, string userName, string otp)
        {
            // Đọc cấu hình SMTP từ web.config
            string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];

            var msg = new MailMessage();
            msg.To.Add(new MailAddress(toEmail));
            msg.From = new MailAddress(smtpUser, "UMC CARE");
            msg.Subject = "Mã OTP đổi mật khẩu tài khoản THẾ GIỚI ẢO";
            msg.Body = $@"
                        Chào {userName},

                        Bạn vừa yêu cầu đổi mật khẩu cho tài khoản trên hệ thống UMC CARE.

                        Mã OTP của bạn là: {otp}

                        Mã này có hiệu lực trong 5 phút. Vui lòng không chia sẻ mã cho bất kỳ ai.

                        Trân trọng,
                        UMC CARE";
            msg.IsBodyHtml = false;

            using (var client = new SmtpClient())
            {
                client.Host = "smtp.gmail.com";
                client.Port = 587;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.Send(msg);
            }
        }

        public ActionResult Profile()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login");

            var maTK = Session["UserID"].ToString();
            var model = new QL_BANDIENTHOAI.Models.ViewModel.ProfileViewModel();

            try
            {
                // CÁCH LẤY CHUỖI KẾT NỐI ĐÚNG 100% CHO FILE CỦA BẠN
                string connString = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // SỬA LỖI "Invalid column name 'NGAYTAO'"
                    string sql = @"
                SELECT 
                    nd.HOTEN, 
                    nd.SDT, 
                    nd.DIACHI, 
                    nd.EMAIL,
                    ISNULL(tk.NGAYDANGKY, GETDATE()) AS NgayTao
                FROM TAIKHOAN tk
                INNER JOIN NGUOIDUNG nd ON tk.ID = nd.ID
                WHERE tk.MATK = @matk";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@matk", maTK);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                model.HoTen = reader["HOTEN"]?.ToString() ?? "Chưa đặt tên";
                                model.SDT = reader["SDT"]?.ToString() ?? "";
                                model.DiaChi = reader["DIACHI"]?.ToString() ?? "";
                                model.Email = reader["EMAIL"]?.ToString() ?? "";
                                model.NgayTao = reader["NgayTao"] != DBNull.Value
                                                ? Convert.ToDateTime(reader["NgayTao"])
                                                : DateTime.Now;
                            }
                            else
                            {
                                model.HoTen = "Khách vãng lai";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                model.HoTen = "Lỗi hệ thống";
            }

            return View(model);
        }

    }


}
