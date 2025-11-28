using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using QL_BANDIENTHOAI.Models;

namespace QL_BANDIENTHOAI.Services
{
    public class DetailService
    {
        private readonly string cs = ConfigurationManager
            .ConnectionStrings["SqlDbContext"].ConnectionString;

        // ==========================
        // Lấy sản phẩm theo mã
        // ==========================
        public SanPham GetProduct(string masp)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                string sql = @"
                  SELECT MASP, MALOAI, TENSP, GIABAN, ANHSANPHAM, MOTA,
                   RAM, ROM, OS, CHIPSET, GPU, CAMERA, PIN, MANHINH,
                   KICHTHUOC, TRONGLUONG, SIMCARD
            FROM SANPHAM WHERE MASP = @id";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", masp);

                var r = cmd.ExecuteReader();
                if (!r.Read()) return null;

                return new SanPham
                {
                    MaSp = r.GetString(0),
                    MaLoai = r.GetString(1),
                    TenSp = r.GetString(2),
                    GiaBan = Convert.ToDouble(r.GetValue(3)),
                    AnhSanPham = r.IsDBNull(4) ? "" : r.GetString(4),
                    MoTa = r.IsDBNull(5) ? "" : r.GetString(5),
                    Ram = r.IsDBNull(6) ? "Không có thông tin" : r.GetString(6),
                    Rom = r.IsDBNull(7) ? "Không có thông tin" : r.GetString(7),
                    Os = r.IsDBNull(8) ? "Không có thông tin" : r.GetString(8),
                    Chipset = r.IsDBNull(9) ? "Không có thông tin" : r.GetString(9),
                    Gpu = r.IsDBNull(10) ? "Không có thông tin" : r.GetString(10),
                    Camera = r.IsDBNull(11) ? "Không có thông tin" : r.GetString(11),
                    Pin = r.IsDBNull(12) ? "Không có thông tin" : r.GetString(12),
                    ManHinh = r.IsDBNull(13) ? "Không có thông tin" : r.GetString(13),
                    KichThuoc = r.IsDBNull(14) ? "Không có thông tin" : r.GetString(14),
                    TrongLuong = r.IsDBNull(15) ? "Không có thông tin" : r.GetString(15),
                    SimCard = r.IsDBNull(16) ? "Không có thông tin" : r.GetString(16)
                };
            }
        }

        // ==========================
        // Lấy sản phẩm liên quan
        // ==========================
        public List<SanPham> GetRelated(string maloai, string except)
        {
            var list = new List<SanPham>();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                string sql = @"
                    SELECT TOP 6 MASP, TENSP, GIABAN, ANHSANPHAM
                    FROM SANPHAM
                    WHERE MALOAI = @m AND MASP <> @id";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@m", maloai);
                cmd.Parameters.AddWithValue("@id", except);

                var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    list.Add(new SanPham
                    {
                        MaSp = r.GetString(0),
                        TenSp = r.GetString(1),
                        GiaBan = Convert.ToDouble(r.GetValue(2)),
                        AnhSanPham = r.IsDBNull(3) ? "" : r.GetString(3)
                    });
                }
            }

            return list;
        }
    }
}
