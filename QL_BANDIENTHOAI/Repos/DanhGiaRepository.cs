using System;
using System.Configuration;
using System.Data.SqlClient;

namespace QL_BANDIENTHOAI.Repository
{
    public class DanhGiaRepository
    {
        private readonly string _cs;

        public DanhGiaRepository()
        {
            _cs = ConfigurationManager.ConnectionStrings["SqlDbContext"].ConnectionString;
        }

        // Lấy điểm TB cho 1 sản phẩm
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
                    return result != DBNull.Value ? Convert.ToDouble(result) : 0;
                }
            }
        }

        // Đếm lượt đánh giá
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
