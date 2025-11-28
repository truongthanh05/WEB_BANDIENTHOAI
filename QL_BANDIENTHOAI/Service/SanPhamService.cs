using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using QL_BANDIENTHOAI.Models;

namespace QL_BANDIENTHOAI.Services
{
    public class SanPhamService
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;

        // =========================
        // 1) Lấy danh mục
        // =========================
        public List<LoaiSp> GetCategories()
        {
            var list = new List<LoaiSp>();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                var cmd = new SqlCommand("SELECT MALOAI, TENLOAI FROM LOAISP", conn);
                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    list.Add(new LoaiSp
                    {
                        MaLoai = r.GetString(0),
                        TenLoai = r.GetString(1)
                    });
                }
            }
            return list;
        }

        // =========================
        // 2) Lấy toàn bộ SP (tìm kiếm + lọc loại)
        // =========================
        public List<SanPham> GetAll(string search, string maloai)
        {
            var list = new List<SanPham>();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                string sql = @"
                    SELECT MASP, MALOAI, TENSP, GIABAN, ANHSANPHAM 
                    FROM SANPHAM
                    WHERE (TENSP LIKE @search OR @search = '')
                      AND (MALOAI = @maloai OR @maloai = '')
                ";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@search", string.IsNullOrEmpty(search) ? "" : "%" + search + "%");
                    cmd.Parameters.AddWithValue("@maloai", maloai ?? "");

                    var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        list.Add(new SanPham
                        {
                            MaSp = r.GetString(0),
                            MaLoai = r.GetString(1),
                            TenSp = r.GetString(2),
                            GiaBan = r.GetDouble(3),
                            AnhSanPham = r.IsDBNull(4) ? "" : r.GetString(4)
                        });
                    }
                }
            }

            return list;
        }

        // =========================
        // 3) SP HOT (giá cao)
        // =========================
        public List<SanPham> GetHotProducts(int n)
        {
            var list = new List<SanPham>();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var cmd = new SqlCommand($@"
                    SELECT TOP {n} MASP, MALOAI, TENSP, GIABAN, ANHSANPHAM 
                    FROM SANPHAM 
                    ORDER BY GIABAN DESC", conn);

                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    list.Add(new SanPham
                    {
                        MaSp = r.GetString(0),
                        MaLoai = r.GetString(1),
                        TenSp = r.GetString(2),
                        GiaBan = r.GetDouble(3),
                        AnhSanPham = r.IsDBNull(4) ? "" : r.GetString(4)
                    });
                }
            }

            return list;
        }

        // =========================
        // 4) SP Giá rẻ
        // =========================
        public List<SanPham> GetCheapProducts(int n)
        {
            var list = new List<SanPham>();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                var cmd = new SqlCommand($@"
                    SELECT TOP {n} MASP, MALOAI, TENSP, GIABAN, ANHSANPHAM 
                    FROM SANPHAM 
                    ORDER BY GIABAN ASC", conn);

                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    list.Add(new SanPham
                    {
                        MaSp = r.GetString(0),
                        MaLoai = r.GetString(1),
                        TenSp = r.GetString(2),
                        GiaBan = r.GetDouble(3),
                        AnhSanPham = r.IsDBNull(4) ? "" : r.GetString(4)
                    });
                }
            }

            return list;
        }

        // =========================
        // 5) GET BY ID (sửa lại chuẩn ADO.NET)
        // =========================
        public SanPham GetById(string masp)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                string sql = @"
            SELECT MASP, MALOAI, TENSP, GIABAN, ANHSANPHAM, MOTA,
                   Ram, Rom, Os, Chipset, Gpu, Camera, Pin,
                   ManHinh, KichThuoc, TrongLuong, SimCard
            FROM SANPHAM
            WHERE MASP = @MASP";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MASP", masp);

                    var r = cmd.ExecuteReader();
                    if (!r.Read()) return null;

                    return new SanPham
                    {
                        MaSp = r["MASP"].ToString(),
                        MaLoai = r["MALOAI"].ToString(),
                        TenSp = r["TENSP"].ToString(),
                        GiaBan = r["GIABAN"] == DBNull.Value ? 0 : Convert.ToDouble(r["GIABAN"]),
                        AnhSanPham = r["ANHSANPHAM"].ToString(),
                        MoTa = r["MOTA"].ToString(),

                        Ram = r["Ram"].ToString(),
                        Rom = r["Rom"].ToString(),
                        Os = r["Os"].ToString(),
                        Chipset = r["Chipset"].ToString(),
                        Gpu = r["Gpu"].ToString(),
                        Camera = r["Camera"].ToString(),
                        Pin = r["Pin"].ToString(),
                        ManHinh = r["ManHinh"].ToString(),
                        KichThuoc = r["KichThuoc"].ToString(),
                        TrongLuong = r["TrongLuong"].ToString(),
                        SimCard = r["SimCard"].ToString()
                    };
                }
            }
        }

    }
}
