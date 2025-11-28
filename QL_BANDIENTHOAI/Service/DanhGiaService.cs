using System;
using System.Configuration;
using System.Data.SqlClient;

namespace QL_BANDIENTHOAI.Services
{
    public class DanhGiaService
    {
        private readonly string _cs = ConfigurationManager
            .ConnectionStrings["SqlDbContext"].ConnectionString;

        // ================================
        // LẤY ĐIỂM TRUNG BÌNH THEO MASP
        // ================================
        public double GetAvgStar(string masp)
        {
            using (var conn = new SqlConnection(_cs))
            {
                conn.Open();

                const string sql = @"
                    SELECT AVG(dg.SOSAO)
                    FROM DANHGIA dg
                    JOIN HOADON hd ON dg.MAHD = hd.MAHD
                    JOIN CHITIETHD ct ON ct.MAHD = hd.MAHD
                    WHERE ct.MASP = @masp";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@masp", masp);

                    var result = cmd.ExecuteScalar();
                    return (result == DBNull.Value) ? 0 : Convert.ToDouble(result);
                }
            }
        }

        // ================================
        // LẤY SỐ LƯỢNG ĐÁNH GIÁ
        // ================================
        public int GetReviewCount(string masp)
        {
            using (var conn = new SqlConnection(_cs))
            {
                conn.Open();

                const string sql = @"
                    SELECT COUNT(*)
                    FROM DANHGIA dg
                    JOIN HOADON hd ON dg.MAHD = hd.MAHD
                    JOIN CHITIETHD ct ON ct.MAHD = hd.MAHD
                    WHERE ct.MASP = @masp";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@masp", masp);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
    }
}
