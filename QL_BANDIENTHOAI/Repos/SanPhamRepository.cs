using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using QL_BANDIENTHOAI.Models;

namespace QL_BANDIENTHOAI.Repository
{
    public class SanPhamRepository
    {
        private readonly string _cs;

        public SanPhamRepository()
        {
            _cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
        }

        public List<SanPham> GetAll(string search, string maloai)
        {
            var list = new List<SanPham>();
            using (var conn = new SqlConnection(_cs))
            {
                conn.Open();

                string sql = "SELECT * FROM SANPHAM WHERE 1=1";

                if (!string.IsNullOrEmpty(search))
                    sql += " AND TENSP LIKE '%' + @s + '%'";

                if (!string.IsNullOrEmpty(maloai))
                    sql += " AND MALOAI = @maloai";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(search))
                        cmd.Parameters.AddWithValue("@s", search);

                    if (!string.IsNullOrEmpty(maloai))
                        cmd.Parameters.AddWithValue("@maloai", maloai);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new SanPham
                            {
                                MaSp = r["MASP"].ToString(),
                                MaLoai = r["MALOAI"].ToString(),
                                TenSp = r["TENSP"].ToString(),
                                GiaBan = Convert.ToDouble(r["GIABAN"]),
                                MoTa = r["MOTA"].ToString()
                            });
                        }
                    }
                }
            }

            return list;
        }

        public List<LoaiSp> GetCategories()
        {
            var list = new List<LoaiSp>();

            using (var conn = new SqlConnection(_cs))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT * FROM LOAISP", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new LoaiSp
                        {
                            MaLoai = r["MALOAI"].ToString(),
                            TenLoai = r["TENLOAI"].ToString()
                        });
                    }
                }
            }

            return list;
        }
    }
}
