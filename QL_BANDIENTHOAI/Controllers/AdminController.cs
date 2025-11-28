using QL_BANDIENTHOAI.Filters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_BANDIENTHOAI.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Dashboard()
        {
            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;

            var statistics = new Dictionary<string, object>();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                // Thống kê số lượng tài khoản
                var totalAccountsQuery = "SELECT COUNT(*) FROM TAIKHOAN";
                using (var cmd = new SqlCommand(totalAccountsQuery, conn))
                {
                    statistics["TotalAccounts"] = cmd.ExecuteScalar();
                }

                // Thống kê số lượng sản phẩm
                var totalProductsQuery = "SELECT COUNT(*) FROM SANPHAM";
                using (var cmd = new SqlCommand(totalProductsQuery, conn))
                {
                    statistics["TotalProducts"] = cmd.ExecuteScalar();
                }

                // Thống kê tổng doanh thu
                var totalRevenueQuery = "SELECT SUM(THANHTIEN) FROM HOADON";
                using (var cmd = new SqlCommand(totalRevenueQuery, conn))
                {
                    statistics["TotalRevenue"] = cmd.ExecuteScalar();
                }

                // Thống kê số lượng phiếu khuyến mãi đã sử dụng
                var totalCouponsUsedQuery = "SELECT COUNT(*) FROM LICHSUDUNGPKM";
                using (var cmd = new SqlCommand(totalCouponsUsedQuery, conn))
                {
                    statistics["TotalCouponsUsed"] = cmd.ExecuteScalar();
                }

                // Thống kê tổng số tiền giảm giá từ các phiếu khuyến mãi
                var totalDiscountAmountQuery = "SELECT SUM(SOTIENGIAM) FROM LICHSUDUNGPKM";
                using (var cmd = new SqlCommand(totalDiscountAmountQuery, conn))
                {
                    statistics["TotalDiscountAmount"] = cmd.ExecuteScalar();
                }
            }

            ViewBag.Statistics = statistics;

            return View();
        }




        [HttpGet]
        public ActionResult Home()
        {
            ViewBag.Active = "Home";
            return View();
        }

        //// ====== LỊCH SỬ ĐĂNG KÝ ======
        //[AdminOnly]
        //public ActionResult History(DateTime? from = null, DateTime? to = null, string keyword = null, string sortBy = "date", string sortDir = "desc")
        //{
        //    var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString; 
        //    var tb = new DataTable();

        //    string orderClause = sortBy == "date" ? $"tk.NGAYTAO {sortDir}" : $"tk.TENTK {sortDir}";

        //    using (var conn = new SqlConnection(cs))
        //    {
        //        conn.Open();
        //        var sql = $@"
        //            SELECT tk.MATK, tk.TENTK, tk.NGAYTAO, u.HOTEN, u.EMAIL
        //            FROM TAIKHOAN tk
        //            JOIN NGUOIDUNG u ON tk.ID = u.ID
        //            WHERE (@from IS NULL OR tk.NGAYTAO >= @from)
        //            AND (@to IS NULL OR tk.NGAYTAO <= @to)
        //            AND (@keyword IS NULL OR tk.TENTK LIKE '%' + @keyword + '%')
        //            ORDER BY {orderClause}";

        //        using (var cmd = new SqlCommand(sql, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@from", (object)from ?? DBNull.Value);
        //            cmd.Parameters.AddWithValue("@to", (object)to ?? DBNull.Value);
        //            cmd.Parameters.AddWithValue("@keyword", (object)keyword ?? DBNull.Value);

        //            using (var r = cmd.ExecuteReader())
        //            {
        //                tb.Load(r);
        //            }
        //        }
        //    }

        //    ViewBag.SortBy = sortBy;
        //    ViewBag.SortDir = sortDir == "asc" ? "desc" : "asc";
        //    return View(tb);
        //}

        //// ====== CHỈNH SỬA THÔNG TIN TÀI KHOẢN ======
        //// Phương thức GET - Hiển thị form chỉnh sửa tài khoản
        [HttpGet]
        public ActionResult EditAccount(string matk)
        {
            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
            var tb = new DataTable();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var sql = @"
            SELECT tk.MATK, tk.TENTK, tk.MATKHAU, tk.ROLE, tk.TRANGTHAI, tk.NGAYTAO
            FROM TAIKHOAN tk
            WHERE tk.MATK = @matk";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@matk", matk);

                    using (var r = cmd.ExecuteReader())
                    {
                        tb.Load(r);
                    }
                }
            }

            if (tb.Rows.Count == 0)
                return RedirectToAction("ManageUsers");  // Nếu không tìm thấy tài khoản

            return View(tb.Rows[0]);  // Trả về DataRow cho view
        }





        [HttpPost]
        public ActionResult EditAccount(string matk, string tentk, string matkhau, string role, string trangthai, DateTime? ngaytao)
        {
            //// Kiểm tra session và role
            //if (Session["Role"]?.ToString() != "ADMIN")
            //{
            //    TempData["Err"] = "Bạn không có quyền thực hiện thao tác này.";
            //    return RedirectToAction("Home", "Admin");
            //}

            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                // Cập nhật các trường của bảng TAIKHOAN
                var sql = @"
            UPDATE TAIKHOAN
            SET TENTK = @tentk,
                MATKHAU = @matkhau,
                ROLE = @role,
                TRANGTHAI = @trangthai,
                NGAYTAO = @ngaytao
            WHERE MATK = @matk";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@matk", matk);
                    cmd.Parameters.AddWithValue("@tentk", tentk);
                    cmd.Parameters.AddWithValue("@matkhau", matkhau);
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@trangthai", trangthai);

                    // Kiểm tra và xử lý ngày tạo
                    if (ngaytao.HasValue)
                        cmd.Parameters.AddWithValue("@ngaytao", ngaytao.Value);
                    else
                        cmd.Parameters.AddWithValue("@ngaytao", DBNull.Value);  // Nếu ngày tạo không có, gán là NULL

                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Msg"] = "Thông tin tài khoản đã được cập nhật.";
            return RedirectToAction("ManageUsers");  // Sau khi lưu thành công, quay lại trang quản lý tài khoản
        }


        // Hiển thị danh sách tài khoản

        // Phương thức GET để quản lý tài khoản
        public ActionResult ManageUsers()
        {
            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString; // Thống nhất tên connection string
            var tb = new DataTable();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var sql = @"
            SELECT tk.MATK, tk.ID, tk.TENTK, tk.MATKHAU, tk.ROLE, tk.TRANGTHAI, tk.NGAYTAO
            FROM TAIKHOAN tk
            WHERE tk.ROLE = 'USER'"; // Lọc tài khoản có role là 'USER'

                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var r = cmd.ExecuteReader())
                    {
                        tb.Load(r);
                    }
                }
            }
            return View(tb); // Trả về DataTable chứa danh sách tài khoản
        }

        [HttpGet]
        public ActionResult DeleteAccount(string matk)
        {
            if (string.IsNullOrWhiteSpace(matk))
            {
                TempData["Err"] = "Mã tài khoản không hợp lệ.";
                return RedirectToAction("ManageUsers");
            }

            try
            {
                var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())  // Đảm bảo các bước xóa tài khoản và người dùng thực hiện trong 1 giao dịch
                    {
                        try
                        {
                            // Lấy ID từ bảng TAIKHOAN dựa trên MATK
                            var sqlGetUserId = "SELECT ID FROM TAIKHOAN WHERE MATK = @matk";
                            var userId = (string)null;

                            using (var cmdGetUserId = new SqlCommand(sqlGetUserId, conn, tx))
                            {
                                cmdGetUserId.Parameters.AddWithValue("@matk", matk);
                                var result = cmdGetUserId.ExecuteScalar();
                                if (result != null)
                                {
                                    userId = result.ToString(); // Lưu ID của người dùng
                                }
                            }

                            // Xóa dữ liệu trong bảng GIOHANG liên quan đến tài khoản
                            var sqlDeleteCart = "DELETE FROM GIOHANG WHERE MATK = @matk";
                            using (var cmdDeleteCart = new SqlCommand(sqlDeleteCart, conn, tx))
                            {
                                cmdDeleteCart.Parameters.AddWithValue("@matk", matk);
                                cmdDeleteCart.ExecuteNonQuery();
                            }

                            // Xóa tài khoản trong bảng TAIKHOAN
                            var sqlDeleteAccount = "DELETE FROM TAIKHOAN WHERE MATK = @matk";
                            using (var cmdDeleteAccount = new SqlCommand(sqlDeleteAccount, conn, tx))
                            {
                                cmdDeleteAccount.Parameters.AddWithValue("@matk", matk);
                                cmdDeleteAccount.ExecuteNonQuery();
                            }

                            // Xóa người dùng trong bảng NGUOIDUNG
                            var sqlDeleteUser = "DELETE FROM NGUOIDUNG WHERE ID = @userId";
                            using (var cmdDeleteUser = new SqlCommand(sqlDeleteUser, conn, tx))
                            {
                                cmdDeleteUser.Parameters.AddWithValue("@userId", userId); // Sử dụng ID đã lấy từ bảng TAIKHOAN
                                cmdDeleteUser.ExecuteNonQuery();
                            }

                            // Commit giao dịch
                            tx.Commit();
                            TempData["Msg"] = "Tài khoản đã được xóa thành công.";
                        }
                        catch (Exception ex)
                        {
                            // Nếu có lỗi xảy ra, rollback giao dịch
                            tx.Rollback();
                            TempData["Err"] = "Lỗi khi xóa tài khoản: " + ex.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Err"] = "Lỗi kết nối cơ sở dữ liệu: " + ex.Message;
            }

            // Sau khi xóa xong, quay lại trang quản lý tài khoản
            return RedirectToAction("ManageUsers");
        }

        [HttpGet]
        public ActionResult CreateCoupon()
        {
            return View();
        }

        // Action để xử lý form tạo mã khuyến mãi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCoupon(string loaiPhieu, decimal giaTri, DateTime ngayHetHan, string dieuKien, int soLuong)
        {
            if (string.IsNullOrEmpty(loaiPhieu) || giaTri <= 0 || ngayHetHan <= DateTime.Now || soLuong <= 0)
            {
                TempData["Err"] = "Thông tin không hợp lệ. Vui lòng nhập lại!";
                return View();
            }

            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                // Sinh mã khuyến mãi tự động (NextId)
                var couponCode = NextId(conn, "PHIEUKHUYENMAI", "MAPKM", "KM");

                // Thêm thông tin mã khuyến mãi vào cơ sở dữ liệu
                var query = @"
                INSERT INTO PHIEUKHUYENMAI (MAPKM, LOAIPHIEU, GIATRI, NGAYHETHAN, DIEUKIEN, SOLUONG, TRANGTHAI)
                VALUES (@couponCode, @loaiPhieu, @giaTri, @ngayHetHan, @dieuKien, @soLuong, 'ACTIVE')";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@couponCode", couponCode);
                    cmd.Parameters.AddWithValue("@loaiPhieu", loaiPhieu);
                    cmd.Parameters.AddWithValue("@giaTri", giaTri);
                    cmd.Parameters.AddWithValue("@ngayHetHan", ngayHetHan);
                    cmd.Parameters.AddWithValue("@dieuKien", dieuKien);
                    cmd.Parameters.AddWithValue("@soLuong", soLuong);

                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Msg"] = "Mã khuyến mãi đã được tạo thành công!";
            return RedirectToAction("Home");
        }

        // Method để sinh mã khuyến mãi tự động (NextId)
        private string NextId(SqlConnection conn, string tableName, string columnName, string prefix)
        {
            string nextId = prefix + "000001"; // Default value if no data
            var sql = $"SELECT MAX(CAST(SUBSTRING({columnName}, 3, LEN({columnName})) AS INT)) + 1 FROM {tableName}";

            using (var cmd = new SqlCommand(sql, conn))
            {
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    nextId = prefix + result.ToString().PadLeft(6, '0');
                }
            }

            return nextId;
        }

        private string NextIdForMASP(SqlConnection conn)
        {
            string nextId = "SP000001";  

            var sql = @"
        SELECT MAX(CAST(SUBSTRING(MASP, 3, LEN(MASP)) AS INT)) + 1 
        FROM SANPHAM 
        WHERE MASP LIKE 'SP%'"; 

            using (var cmd = new SqlCommand(sql, conn))
            {
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    nextId = "SP" + result.ToString().PadLeft(6, '0'); 
                }
            }

            return nextId;
        }

        // Quản lý các mã khuyến mãi
        public ActionResult ManageCoupons()
        {
            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
            var tb = new DataTable();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var sql = @"
            SELECT MAPKM, LOAIPHIEU, GIATRI, NGAYHETHAN, DIEUKIEN, SOLUONG, TRANGTHAI
            FROM PHIEUKHUYENMAI"; // Lọc tất cả các mã khuyến mãi

                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var r = cmd.ExecuteReader())
                    {
                        tb.Load(r);
                    }
                }
            }

            return View(tb);  // Trả về DataTable chứa danh sách mã khuyến mãi
        }



        private DataRow GetCouponById(string mapkm)
        {
            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
            var tb = new DataTable();
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var sql = @"
            SELECT MAPKM, LOAIPHIEU, GIATRI, NGAYHETHAN, DIEUKIEN, SOLUONG, TRANGTHAI
            FROM PHIEUKHUYENMAI
            WHERE MAPKM = @mapkm";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@mapkm", mapkm);
                    using (var r = cmd.ExecuteReader())
                    {
                        tb.Load(r);
                    }
                }
            }
            return tb.Rows.Count > 0 ? tb.Rows[0] : null;
        }

        // Chỉnh sửa mã khuyến mãi
        [HttpGet]
        public ActionResult EditCoupon(string mapkm)
        {
            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
            var tb = new DataTable();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var sql = @"
            SELECT MAPKM, LOAIPHIEU, GIATRI, NGAYHETHAN, DIEUKIEN, SOLUONG, TRANGTHAI
            FROM PHIEUKHUYENMAI 
            WHERE MAPKM = @mapkm";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@mapkm", mapkm);

                    using (var r = cmd.ExecuteReader())
                    {
                        tb.Load(r);
                    }
                }
            }

            if (tb.Rows.Count == 0)
                return RedirectToAction("ManageCoupons"); // Nếu không tìm thấy mã khuyến mãi

            return View(tb.Rows[0]);  // Trả về DataRow cho View
        }


        // Action để xử lý chỉnh sửa mã khuyến mãi
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult EditCoupon(string mapkm, string loaiPhieu, decimal giaTri, DateTime ngayHetHan, string dieuKien, int soLuong, string trangthai)
        //{
        //    if (string.IsNullOrEmpty(loaiPhieu) || giaTri <= 0 || ngayHetHan <= DateTime.Now || soLuong <= 0)
        //    {
        //        TempData["Err"] = "Thông tin không hợp lệ. Vui lòng nhập lại!";
        //        return View();
        //    }

        //    if (string.IsNullOrEmpty(mapkm))  // Kiểm tra nếu mã khuyến mãi không có giá trị
        //    {
        //        TempData["Err"] = "Mã khuyến mãi không hợp lệ.";
        //        return View();
        //    }

        //    var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;

        //    using (var conn = new SqlConnection(cs))
        //    {
        //        conn.Open();

        //        // Cập nhật thông tin mã khuyến mãi vào cơ sở dữ liệu
        //        var query = @"
        //    UPDATE PHIEUKHUYENMAI 
        //    SET LOAIPHIEU = @loaiPhieu,
        //        GIATRI = @giaTri,
        //        NGAYHETHAN = @ngayHetHan,
        //        DIEUKIEN = @dieuKien,
        //        SOLUONG = @soLuong,
        //        TRANGTHAI = @trangthai
        //    WHERE MAPKM = @mapkm";  // Đảm bảo @mapkm được sử dụng trong câu truy vấn

        //        using (var cmd = new SqlCommand(query, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@mapkm", mapkm);  // Truyền giá trị của mapkm vào truy vấn
        //            cmd.Parameters.AddWithValue("@loaiPhieu", loaiPhieu);
        //            cmd.Parameters.AddWithValue("@giaTri", giaTri);
        //            cmd.Parameters.AddWithValue("@ngayHetHan", ngayHetHan);
        //            cmd.Parameters.AddWithValue("@dieuKien", dieuKien);
        //            cmd.Parameters.AddWithValue("@soLuong", soLuong);
        //            cmd.Parameters.AddWithValue("@trangthai", trangthai);

        //            cmd.ExecuteNonQuery();
        //        }
        //    }

        //    TempData["Msg"] = "Mã khuyến mãi đã được cập nhật thành công!";
        //    return RedirectToAction("ManageCoupons");  // Sau khi lưu thành công, quay lại trang quản lý mã khuyến mãi
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCoupon(string mapkm, string loaiPhieu, decimal giaTri, DateTime ngayHetHan, string dieuKien, int soLuong, string trangthai)
        {
            if (string.IsNullOrEmpty(mapkm))
            {
                TempData["Err"] = "Mã khuyến mãi không hợp lệ.";
                return RedirectToAction("ManageCoupons"); // Redirect thay vì return View()
            }

            if (string.IsNullOrEmpty(loaiPhieu) || giaTri <= 0 || ngayHetHan <= DateTime.Now || soLuong <= 0)
            {
                TempData["Err"] = "Thông tin không hợp lệ. Vui lòng nhập lại!";
                var row = GetCouponById(mapkm); // Lấy lại data từ DB
                if (row == null)
                {
                    return RedirectToAction("ManageCoupons");
                }
                return View(row); // Truyền model vào View
            }

            // Phần update DB giữ nguyên...
            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = @"
            UPDATE PHIEUKHUYENMAI
            SET LOAIPHIEU = @loaiPhieu,
                GIATRI = @giaTri,
                NGAYHETHAN = @ngayHetHan,
                DIEUKIEN = @dieuKien,
                SOLUONG = @soLuong,
                TRANGTHAI = @trangthai
            WHERE MAPKM = @mapkm";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@mapkm", mapkm);
                    cmd.Parameters.AddWithValue("@loaiPhieu", loaiPhieu);
                    cmd.Parameters.AddWithValue("@giaTri", giaTri);
                    cmd.Parameters.AddWithValue("@ngayHetHan", ngayHetHan);
                    cmd.Parameters.AddWithValue("@dieuKien", dieuKien);
                    cmd.Parameters.AddWithValue("@soLuong", soLuong);
                    cmd.Parameters.AddWithValue("@trangthai", trangthai);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Msg"] = "Mã khuyến mãi đã được cập nhật thành công!";
            return RedirectToAction("ManageCoupons");
        }

        [HttpGet]
        public ActionResult DeleteCoupon(string mapkm)
        {
            if (string.IsNullOrWhiteSpace(mapkm))
            {
                TempData["Err"] = "Mã khuyến mãi không hợp lệ.";
                return RedirectToAction("ManageCoupons");
            }

            try
            {
                var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;

                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())  // Đảm bảo các bước xóa khuyến mãi thực hiện trong 1 giao dịch
                    {
                        try
                        {
                            // Xóa mã khuyến mãi trong bảng PHIEUKHUYENMAI
                            var sqlDeleteCoupon = "DELETE FROM PHIEUKHUYENMAI WHERE MAPKM = @mapkm";
                            using (var cmdDeleteCoupon = new SqlCommand(sqlDeleteCoupon, conn, tx))
                            {
                                cmdDeleteCoupon.Parameters.AddWithValue("@mapkm", mapkm);
                                cmdDeleteCoupon.ExecuteNonQuery();
                            }

                            // Commit giao dịch
                            tx.Commit();
                            TempData["Msg"] = "Mã khuyến mãi đã được xóa thành công.";
                        }
                        catch (Exception ex)
                        {
                            // Nếu có lỗi xảy ra, rollback giao dịch
                            tx.Rollback();
                            TempData["Err"] = "Lỗi khi xóa mã khuyến mãi: " + ex.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Err"] = "Lỗi kết nối cơ sở dữ liệu: " + ex.Message;
            }

            return RedirectToAction("ManageCoupons");
        }

        [HttpGet]
        public ActionResult CreateProduct()
        {
     
            ViewBag.OsList = new List<string> { "Android", "iOS", "Windows" };
            ViewBag.SimCardList = new List<string> { "1 SIM", "2 SIM" };

 
            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var sql = "SELECT MALOAI, TENLOAI FROM LOAISP"; 
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        var maLoaiList = new List<SelectListItem>();
                        while (reader.Read())
                        {
                            maLoaiList.Add(new SelectListItem
                            {
                                Value = reader["MALOAI"].ToString(),
                                Text = reader["TENLOAI"].ToString()
                            });
                        }
                        ViewBag.MaLoaiList = maLoaiList; 
                    }
                }
            }

            return View();
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProduct(string maLoai, string tenSp, decimal giaBan, string moTa,
     string ram, string rom, string os, string chipset, string gpu, string camera,
     string pin, string manHinh, string kichThuoc, string trongLuong, string simCard, HttpPostedFileBase anhSanPham)
        {
            if (string.IsNullOrEmpty(tenSp) || giaBan <= 0)
            {
                TempData["Err"] = "Thông tin không hợp lệ. Vui lòng nhập lại!";
                return View();
            }

            // Xử lý ảnh tải lên
            string imagePath = null;
            if (anhSanPham != null && anhSanPham.ContentLength > 0)
            {
                var fileName = Path.GetFileName(anhSanPham.FileName);
                imagePath = "/img/" + fileName;
                var path = Path.Combine(Server.MapPath("~/Content/img"), fileName);
                anhSanPham.SaveAs(path);  // Lưu ảnh vào thư mục Content/img
            }

            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                // Sử dụng hàm NextIdForMASP để sinh mã sản phẩm
                var productCode = NextIdForMASP(conn);

                var query = @"
            INSERT INTO SANPHAM (MASP, MALOAI, TENSP, GIABAN, MOTA, RAM, ROM, OS, CHIPSET, GPU, CAMERA, 
                PIN, MANHINH, KICHTHUOC, TRONGLUONG, SIMCARD, ANHSANPHAM)
            VALUES (@productCode, @maLoai, @tenSp, @giaBan, @moTa, @ram, @rom, @os, @chipset, @gpu, 
                @camera, @pin, @manHinh, @kichThuoc, @trongLuong, @simCard, @anhSanPham)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@productCode", productCode);
                    cmd.Parameters.AddWithValue("@maLoai", maLoai);
                    cmd.Parameters.AddWithValue("@tenSp", tenSp);
                    cmd.Parameters.AddWithValue("@giaBan", giaBan);
                    cmd.Parameters.AddWithValue("@moTa", moTa);
                    cmd.Parameters.AddWithValue("@ram", ram);
                    cmd.Parameters.AddWithValue("@rom", rom);
                    cmd.Parameters.AddWithValue("@os", os);
                    cmd.Parameters.AddWithValue("@chipset", chipset);
                    cmd.Parameters.AddWithValue("@gpu", gpu);
                    cmd.Parameters.AddWithValue("@camera", camera);
                    cmd.Parameters.AddWithValue("@pin", pin);
                    cmd.Parameters.AddWithValue("@manHinh", manHinh);
                    cmd.Parameters.AddWithValue("@kichThuoc", kichThuoc);
                    cmd.Parameters.AddWithValue("@trongLuong", trongLuong);
                    cmd.Parameters.AddWithValue("@simCard", simCard);
                    cmd.Parameters.AddWithValue("@anhSanPham", imagePath);

                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Msg"] = "Sản phẩm đã được tạo thành công!";
            return RedirectToAction("Home");
        }





        public ActionResult ManageProducts()
        {
            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
            var tb = new DataTable();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var sql = "SELECT MASP, TENSP, GIABAN FROM SANPHAM";  

                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var r = cmd.ExecuteReader())
                    {
                        tb.Load(r);
                    }
                }
            }

            return View(tb);  // Trả về DataTable chứa danh sách sản phẩm
        }


        [HttpGet]
        public ActionResult EditProduct(string masp)
        {
            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
            var tb = new DataTable();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var sql = @"
            SELECT MASP, MALOAI, TENSP, GIABAN, MOTA, RAM, ROM, OS, CHIPSET, GPU, CAMERA, PIN, MANHINH, KICHTHUOC, TRONGLUONG, SIMCARD, ANHSANPHAM
            FROM SANPHAM 
            WHERE MASP = @masp";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@masp", masp);
                    using (var reader = cmd.ExecuteReader())
                    {
                        tb.Load(reader);
                    }
                }
            }

            if (tb.Rows.Count == 0)
                return RedirectToAction("ManageProducts");  // Nếu không tìm thấy sản phẩm

            ViewBag.MaLoaiList = GetMaLoaiList(); // Lấy danh sách mã loại từ cơ sở dữ liệu
            ViewBag.OsList = new List<string> { "Android", "iOS", "Windows" };
            ViewBag.SimCardList = new List<string> { "1 SIM", "2 SIM" };

            return View(tb.Rows[0]);  // Trả về DataRow cho View
        }

        private List<SelectListItem> GetMaLoaiList()
        {
            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
            var maLoaiList = new List<SelectListItem>();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var sql = "SELECT MALOAI, TENLOAI FROM LOAISP";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            maLoaiList.Add(new SelectListItem
                            {
                                Value = reader["MALOAI"].ToString(),
                                Text = reader["TENLOAI"].ToString()
                            });
                        }
                    }
                }
            }

            return maLoaiList;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(string masp, string maLoai, string tenSp, decimal giaBan, string moTa,
    string ram, string rom, string os, string chipset, string gpu, string camera, string pin,
    string manHinh, string kichThuoc, string trongLuong, string simCard, HttpPostedFileBase anhSanPham)
        {
            if (string.IsNullOrEmpty(tenSp) || giaBan <= 0)
            {
                TempData["Err"] = "Thông tin không hợp lệ. Vui lòng nhập lại!";
                return View();
            }

            // Xử lý ảnh tải lên
            string imagePath = null;
            if (anhSanPham != null && anhSanPham.ContentLength > 0)
            {
                var fileName = Path.GetFileName(anhSanPham.FileName);
                imagePath = "/img/" + fileName;
                var path = Path.Combine(Server.MapPath("~/Content/img"), fileName);
                anhSanPham.SaveAs(path);  // Lưu ảnh vào thư mục Content/img
            }

            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                var query = @"
            UPDATE SANPHAM
            SET MALOAI = @maLoai,
                TENSP = @tenSp,
                GIABAN = @giaBan,
                MOTA = @moTa,
                RAM = @ram,
                ROM = @rom,
                OS = @os,
                CHIPSET = @chipset,
                GPU = @gpu,
                CAMERA = @camera,
                PIN = @pin,
                MANHINH = @manHinh,
                KICHTHUOC = @kichThuoc,
                TRONGLUONG = @trongLuong,
                SIMCARD = @simCard,
                ANHSANPHAM = @anhSanPham
            WHERE MASP = @masp";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@masp", masp);
                    cmd.Parameters.AddWithValue("@maLoai", maLoai);
                    cmd.Parameters.AddWithValue("@tenSp", tenSp);
                    cmd.Parameters.AddWithValue("@giaBan", giaBan);
                    cmd.Parameters.AddWithValue("@moTa", moTa);
                    cmd.Parameters.AddWithValue("@ram", ram);
                    cmd.Parameters.AddWithValue("@rom", rom);
                    cmd.Parameters.AddWithValue("@os", os);
                    cmd.Parameters.AddWithValue("@chipset", chipset);
                    cmd.Parameters.AddWithValue("@gpu", gpu);
                    cmd.Parameters.AddWithValue("@camera", camera);
                    cmd.Parameters.AddWithValue("@pin", pin);
                    cmd.Parameters.AddWithValue("@manHinh", manHinh);
                    cmd.Parameters.AddWithValue("@kichThuoc", kichThuoc);
                    cmd.Parameters.AddWithValue("@trongLuong", trongLuong);
                    cmd.Parameters.AddWithValue("@simCard", simCard);
                    cmd.Parameters.AddWithValue("@anhSanPham", imagePath);

                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Msg"] = "Sản phẩm đã được cập nhật thành công!";
            return RedirectToAction("ManageProducts");  // Sau khi lưu thành công, quay lại trang quản lý sản phẩm
        }

        private string NextIdForMaLoai(SqlConnection conn)
        {
            string nextId = "LO" + "000001";  // Default value if no data  

            var sql = @"
        SELECT MAX(CAST(SUBSTRING(MALOAI, 3, LEN(MALOAI)) AS INT)) + 1 
        FROM LOAISP 
        WHERE MALOAI LIKE 'LO%'";  // Lọc các MALOAI bắt đầu với 'LO'

            using (var cmd = new SqlCommand(sql, conn))
            {
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    nextId = "LO" + result.ToString().PadLeft(6, '0');
                }
            }

            return nextId;
        }

        [HttpGet]
        public ActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(string tenLoai)
        {
            if (string.IsNullOrEmpty(tenLoai))
            {
                TempData["Err"] = "Tên loại không hợp lệ. Vui lòng nhập lại!";
                return View();
            }

            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                // Sinh mã loại sản phẩm tự động
                var maLoai = NextIdForMaLoai(conn);

                var query = "INSERT INTO LOAISP (MALOAI, TENLOAI) VALUES (@maLoai, @tenLoai)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@maLoai", maLoai);
                    cmd.Parameters.AddWithValue("@tenLoai", tenLoai);
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Msg"] = "Loại sản phẩm đã được tạo thành công!";
            return RedirectToAction("ManageCategories");
        }


        public ActionResult ManageCategories()
        {
            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
            var tb = new DataTable();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var sql = "SELECT MALOAI, TENLOAI FROM LOAISP";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var r = cmd.ExecuteReader())
                    {
                        tb.Load(r);
                    }
                }
            }

            return View(tb);
        }

        [HttpGet]
        public ActionResult EditCategory(string maLoai)
        {
            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
            var tb = new DataTable();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var sql = "SELECT MALOAI, TENLOAI FROM LOAISP WHERE MALOAI = @maLoai";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@maLoai", maLoai);

                    using (var r = cmd.ExecuteReader())
                    {
                        tb.Load(r);
                    }
                }
            }

            if (tb.Rows.Count == 0)
                return RedirectToAction("ManageCategories");  // Nếu không tìm thấy loại sản phẩm

            return View(tb.Rows[0]);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(string maLoai, string tenLoai)
        {
            if (string.IsNullOrEmpty(maLoai) || string.IsNullOrEmpty(tenLoai))
            {
                TempData["Err"] = "Thông tin không hợp lệ. Vui lòng nhập lại!";
                return View();
            }

            var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var query = "UPDATE LOAISP SET TENLOAI = @tenLoai WHERE MALOAI = @maLoai";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@maLoai", maLoai);
                    cmd.Parameters.AddWithValue("@tenLoai", tenLoai);
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Msg"] = "Loại sản phẩm đã được cập nhật thành công!";
            return RedirectToAction("ManageCategories");
        }


        [HttpGet]
        public ActionResult DeleteCategory(string maLoai)
        {
            if (string.IsNullOrWhiteSpace(maLoai))
            {
                TempData["Err"] = "Mã loại sản phẩm không hợp lệ.";
                return RedirectToAction("ManageCategories");
            }

            try
            {
                var cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;

                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    var sqlDeleteCategory = "DELETE FROM LOAISP WHERE MALOAI = @maLoai";
                    using (var cmdDeleteCategory = new SqlCommand(sqlDeleteCategory, conn))
                    {
                        cmdDeleteCategory.Parameters.AddWithValue("@maLoai", maLoai);
                        cmdDeleteCategory.ExecuteNonQuery();
                    }
                }

                TempData["Msg"] = "Loại sản phẩm đã được xóa thành công.";
            }
            catch (Exception ex)
            {
                TempData["Err"] = "Lỗi khi xóa loại sản phẩm: " + ex.Message;
            }

            return RedirectToAction("ManageCategories");
        }


    }
}