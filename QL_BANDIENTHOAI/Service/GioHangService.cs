using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using QL_BANDIENTHOAI.Models;

namespace QL_BANDIENTHOAI.Services
{
    public class GioHangService
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;

        // =============================
        // ĐẾM SỐ SẢN PHẨM TRONG GIỎ
        // =============================
        public int GetCartCount(string matk)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT SUM(SOLUONG) 
                    FROM CHITIETGH 
                    WHERE MATK = @m", conn);

                cmd.Parameters.AddWithValue("@m", matk);

                var result = cmd.ExecuteScalar();
                return (result == DBNull.Value || result == null) ? 0 : Convert.ToInt32(result);
            }
        }

        // =============================
        // LẤY ITEMS TRONG GIỎ
        // =============================
        public List<CartItem> GetCartItems(string matk)
        {
            var list = new List<CartItem>();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                string sql = @"
                    SELECT gh.MaSP, sp.TenSP, sp.GiaBan, sp.AnhSanPham, gh.SoLuong
                    FROM CHITIETGH gh
                    JOIN SANPHAM sp ON gh.MaSP = sp.MaSP
                    WHERE gh.MaTK = @m";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@m", matk);
                    var r = cmd.ExecuteReader();

                    while (r.Read())
                    {
                        list.Add(new CartItem
                        {
                            MaSp = r.GetString(0),
                            TenSp = r.GetString(1),
                            GiaBan = Convert.ToDouble(r.GetValue(2)),
                            AnhSanPham = r.GetString(3),
                            SoLuong = r.GetInt32(4)
                        });
                    }
                }
            }

            return list;
        }

        // =============================
        // THÊM VÀO GIỎ HÀNG
        // =============================
        public bool AddToCart(string matk, string masp)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                // 1) Kiểm tra đã có SP trong giỏ chưa
                string check = @"
                    SELECT SoLuong 
                    FROM CHITIETGH 
                    WHERE MaTK = @tk AND MaSP = @sp";

                using (var cmd = new SqlCommand(check, conn))
                {
                    cmd.Parameters.AddWithValue("@tk", matk);
                    cmd.Parameters.AddWithValue("@sp", masp);

                    var result = cmd.ExecuteScalar();

                    // Đã tồn tại → tăng số lượng
                    if (result != null && result != DBNull.Value)
                    {
                        string update = @"
                            UPDATE CHITIETGH
                            SET SoLuong = SoLuong + 1
                            WHERE MaTK = @tk AND MaSP = @sp";

                        var cmdU = new SqlCommand(update, conn);
                        cmdU.Parameters.AddWithValue("@tk", matk);
                        cmdU.Parameters.AddWithValue("@sp", masp);

                        return cmdU.ExecuteNonQuery() > 0;
                    }
                }

                // 2) Chưa có → INSERT
                string insert = @"
                    INSERT INTO CHITIETGH (MaTK, MaSP, SoLuong)
                    VALUES (@tk, @sp, 1)";

                var cmdI = new SqlCommand(insert, conn);
                cmdI.Parameters.AddWithValue("@tk", matk);
                cmdI.Parameters.AddWithValue("@sp", masp);

                return cmdI.ExecuteNonQuery() > 0;
            }
        }
    }
}
