using QL_BANDIENTHOAI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace QL_BANDIENTHOAI.Services
{
    public class GioHangService
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;

        // Lấy danh sách giỏ hàng
        public List<CartItem> GetCart(string maTK)
        {
            var list = new List<CartItem>();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                const string sql = @"
                    SELECT gh.MaSP, sp.TenSP, sp.AnhSanPham, gh.DonGia, gh.SoLuong
                    FROM CHITIETGH gh
                    JOIN SANPHAM sp ON gh.MaSP = sp.MaSP
                    WHERE gh.MaTK = @tk";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@tk", maTK);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new CartItem
                            {
                                MaSp = r.GetString(0),
                                TenSp = r.GetString(1),
                                AnhSanPham = r.IsDBNull(2) ? "/Content/images/no-image.jpg" : r.GetString(2),
                                GiaBan = Convert.ToDouble(r.GetValue(3)),
                                SoLuong = r.GetInt32(4)
                            });
                        }
                    }
                }
            }
            return list;
        }

        // Lấy tổng số lượng sản phẩm trong giỏ (hiển thị ở icon giỏ hàng)
        public int GetCartCount(string maTK)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                const string sql = @"SELECT ISNULL(SUM(SoLuong), 0) FROM CHITIETGH WHERE MaTK = @tk";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@tk", maTK);
                    var result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToInt32(result);
                }
            }
        }

        // Thêm hoặc tăng số lượng sản phẩm
        public void AddToCart(string maTK, string maSP)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Kiểm tra sản phẩm đã có trong giỏ chưa
                        const string checkSql = @"SELECT SoLuong FROM CHITIETGH WHERE MaTK = @tk AND MaSP = @sp";
                        using (var cmd = new SqlCommand(checkSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@tk", maTK);
                            cmd.Parameters.AddWithValue("@sp", maSP);
                            var result = cmd.ExecuteScalar();

                            if (result != null) // Đã có → tăng số lượng
                            {
                                const string updateSql = @"UPDATE CHITIETGH SET SoLuong = SoLuong + 1 WHERE MaTK = @tk AND MaSP = @sp";
                                using (var updateCmd = new SqlCommand(updateSql, conn, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@tk", maTK);
                                    updateCmd.Parameters.AddWithValue("@sp", maSP);
                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                            else // Chưa có → thêm mới
                            {
                                // Lấy giá bán hiện tại
                                const string priceSql = @"SELECT GiaBan FROM SANPHAM WHERE MaSP = @sp";
                                double giaBan = 0;
                                using (var priceCmd = new SqlCommand(priceSql, conn, transaction))
                                {
                                    priceCmd.Parameters.AddWithValue("@sp", maSP);
                                    var priceResult = priceCmd.ExecuteScalar();
                                    if (priceResult != null && priceResult != DBNull.Value)
                                        giaBan = Convert.ToDouble(priceResult);
                                }

                                const string insertSql = @"
                                    INSERT INTO CHITIETGH (MaTK, MaSP, SoLuong, DonGia)
                                    VALUES (@tk, @sp, 1, @gia)";

                                using (var insertCmd = new SqlCommand(insertSql, conn, transaction))
                                {
                                    insertCmd.Parameters.AddWithValue("@tk", maTK);
                                    insertCmd.Parameters.AddWithValue("@sp", maSP);
                                    insertCmd.Parameters.AddWithValue("@gia", giaBan);
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw; // Để controller bắt lỗi nếu cần
                    }
                }
            }
        }

        // Giảm số lượng
        public void Minus(string maTK, string maSP)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"SELECT SoLuong FROM CHITIETGH WHERE MaTK = @tk AND MaSP = @sp";
                        int soLuong = 0;

                        using (var cmd = new SqlCommand(sql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@tk", maTK);
                            cmd.Parameters.AddWithValue("@sp", maSP);
                            var result = cmd.ExecuteScalar();
                            if (result != null) soLuong = Convert.ToInt32(result);
                        }

                        if (soLuong <= 1)
                        {
                            // Xóa luôn nếu chỉ còn 1
                            const string deleteSql = @"DELETE FROM CHITIETGH WHERE MaTK = @tk AND MaSP = @sp";
                            using (var delCmd = new SqlCommand(deleteSql, conn, transaction))
                            {
                                delCmd.Parameters.AddWithValue("@tk", maTK);
                                delCmd.Parameters.AddWithValue("@sp", maSP);
                                delCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Giảm 1
                            const string updateSql = @"UPDATE CHITIETGH SET SoLuong = SoLuong - 1 WHERE MaTK = @tk AND MaSP = @sp";
                            using (var updCmd = new SqlCommand(updateSql, conn, transaction))
                            {
                                updCmd.Parameters.AddWithValue("@tk", maTK);
                                updCmd.Parameters.AddWithValue("@sp", maSP);
                                updCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Xóa sản phẩm khỏi giỏ
        public void Remove(string maTK, string maSP)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                const string sql = @"DELETE FROM CHITIETGH WHERE MaTK = @tk AND MaSP = @sp";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@tk", maTK);
                    cmd.Parameters.AddWithValue("@sp", maSP);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public bool DatHang(string maTK, string phuongThuc, string maPKM = "")
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Tạo hóa đơn
                        var cart = GetCart(maTK);
                        double tongTien = cart.Sum(x => x.GiaBan * x.SoLuong);

                        // Áp dụng voucher
                        if (!string.IsNullOrEmpty(maPKM))
                        {
                            var voucher = new HeaderService().GetVouchers().FirstOrDefault(v => v.MaPKM == maPKM);
                            if (voucher != null)
                            {
                                if (voucher.LoaiPhieu == "GIAMGIA")
                                    tongTien = tongTien * (100 - voucher.GiaTri) / 100;
                                else
                                    tongTien -= voucher.GiaTri;
                            }
                        }

                        string maHd = "HD" + DateTime.Now.ToString("yyMMddHHmmss");
                        string sqlHD = "INSERT INTO HOADON (MAHD, ID, THANHTIEN, NGAYLAP) VALUES (@mahd, @id, @tong, GETDATE())";
                        using (var cmd = new SqlCommand(sqlHD, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@mahd", maHd);
                            cmd.Parameters.AddWithValue("@id", "U001"); // tạm thời, bạn sửa theo KH thật
                            cmd.Parameters.AddWithValue("@tong", tongTien);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Thêm chi tiết hóa đơn
                        foreach (var item in cart)
                        {
                            string sqlCT = "INSERT INTO CHITIETHD (MAHD, MASP, SOLUONG, DONGIA) VALUES (@mahd, @masp, @sl, @gia)";
                            using (var cmd = new SqlCommand(sqlCT, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@mahd", maHd);
                                cmd.Parameters.AddWithValue("@masp", item.MaSp);
                                cmd.Parameters.AddWithValue("@sl", item.SoLuong);
                                cmd.Parameters.AddWithValue("@gia", item.GiaBan);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 3. Xóa giỏ hàng
                        using (var cmdXoa = new SqlCommand("DELETE FROM CHITIETGH WHERE MATK = @tk", conn, tran))
                        {
                            cmdXoa.Parameters.AddWithValue("@tk", maTK);
                            cmdXoa.ExecuteNonQuery();
                        }

                        tran.Commit();
                        return true;
                    }
                    catch
                    {
                        tran.Rollback();
                        return false;
                    }
                }
            }
        }
    }
}