using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using QL_BANDIENTHOAI.Models;

namespace QL_BANDIENTHOAI.Services
{
    public class HeaderService
    {
        private readonly string cs = ConfigurationManager
            .ConnectionStrings["SqlDbContext"].ConnectionString;

        // ========================
        // 1. LẤY DANH SÁCH CỬA HÀNG (KHO)
        // ========================
        public List<Kho> GetStores()
        {
            var list = new List<Kho>();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                string sql = "SELECT MAKHO, TENKHO, DIACHI FROM KHO";

                var cmd = new SqlCommand(sql, conn);
                var rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    list.Add(new Kho
                    {
                        MaKho = rd.GetString(0),
                        TenKho = rd.IsDBNull(1) ? "" : rd.GetString(1),
                        DiaChi = rd.IsDBNull(2) ? "" : rd.GetString(2)
                    });
                }
            }

            return list;
        }

        // ========================
        // 2. LẤY ĐƠN HÀNG THEO MATK (JOIN TAIKHOAN → NGUOIDUNG.ID → HOADON.ID)
        // ========================
        // ========================
        // 2. LẤY ĐƠN HÀNG THEO MATK (ĐÃ SỬA ĐÚNG 100% VỚI DB HIỆN TẠI)
        // ========================
        // ========================
        // LẤY ĐƠN HÀNG CỦA NGƯỜI DÙNG ĐÃ ĐĂNG NHẬP
        // Dựa trên: TAIKHOAN → có thể có KhachHang → HOADON.Id = KhachHang.Id
        // ========================
        // LẤY DANH SÁCH ĐƠN HÀNG CỦA NGƯỜI DÙNG ĐANG ĐĂNG NHẬP
        public List<HoaDon> GetOrders(string maTK)
        {
            var list = new List<HoaDon>();
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                string sql = @"
            SELECT hd.MAHD, hd.NGAYLAP, hd.THANHTIEN, nd.HOTEN
            FROM HOADON hd
            INNER JOIN KHACHHANG kh ON hd.ID = kh.ID
            INNER JOIN NGUOIDUNG nd ON kh.ID = nd.ID
            INNER JOIN TAIKHOAN tk ON nd.ID = tk.ID
            WHERE tk.MATK = @matk
            ORDER BY hd.NGAYLAP DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@matk", maTK);
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            list.Add(new HoaDon
                            {
                                MaHd = rd.GetString(0),
                                NgayLap = rd.IsDBNull(1) ? (DateTime?)null : rd.GetDateTime(1),
                                ThanhTien = rd.IsDBNull(2) ? 0 : Convert.ToDouble(rd.GetValue(2)),
                                KhachHang = new KhachHang
                                {
                                    HoTen = rd.IsDBNull(3) ? "Khách lẻ" : rd.GetString(3)
                                }
                            });
                        }
                    }
                }
            }
            return list;
        }
        // ========================
        // 3. LẤY KHUYẾN MÃI ĐANG Active
        // ========================
        public List<PhieuKhuyenMai> GetVouchers()
        {
            var list = new List<PhieuKhuyenMai>();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                string sql = @"
                    SELECT MAPKM, LOAIPHIEU, GIATRI, NGAYHETHAN
                    FROM PHIEUKHUYENMAI
                    WHERE TRANGTHAI = 'Active'";

                var cmd = new SqlCommand(sql, conn);
                var rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    list.Add(new PhieuKhuyenMai
                    {
                        MaPKM = rd.GetString(0),
                        LoaiPhieu = rd.IsDBNull(1) ? "" : rd.GetString(1),
                        GiaTri = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd.GetValue(2)),
                        NgayHetHan = rd.IsDBNull(3) ? DateTime.Now : rd.GetDateTime(3)
                    });
                }
            }

            return list;
        }
        // LẤY CHI TIẾT ĐƠN HÀNG + DANH SÁCH SẢN PHẨM TRONG ĐƠN
        public HoaDon GetOrderDetail(string maHd, string maTK)
        {
            HoaDon hoaDon = null;
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                // 1. Lấy thông tin đơn hàng + tên khách
                string sql = @"
            SELECT hd.MAHD, hd.NGAYLAP, hd.THANHTIEN, nd.HOTEN, nd.SDT, nd.DIACHI,
                   gh.TRANGTHAI, gh.NGAYGIAO
            FROM HOADON hd
            INNER JOIN KHACHHANG kh ON hd.ID = kh.ID
            INNER JOIN NGUOIDUNG nd ON kh.ID = nd.ID
            INNER JOIN TAIKHOAN tk ON nd.ID = tk.ID
            LEFT JOIN GIAOHANG gh ON hd.MAHD = gh.MAHD
            WHERE hd.MAHD = @mahd AND tk.MATK = @matk";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@mahd", maHd);
                    cmd.Parameters.AddWithValue("@matk", maTK);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            hoaDon = new HoaDon
                            {
                                MaHd = rd.GetString(0),
                                NgayLap = rd.GetDateTime(1),
                                ThanhTien = Convert.ToDouble(rd.GetValue(2)),
                                KhachHang = new KhachHang
                                {
                                    HoTen = rd.IsDBNull(3) ? "" : rd.GetString(3),
                                    DienThoai = rd.IsDBNull(4) ? "" : rd.GetString(4),
                                    DiaChi = rd.IsDBNull(5) ? "" : rd.GetString(5)
                                }
                            };
                            // Trạng thái giao hàng (nếu có)
                            hoaDon.GiaoHang = new GiaoHang
                            {
                                TrangThai = rd.IsDBNull(6) ? "Chờ xử lý" : rd.GetString(6),
                                NgayGiao = rd.IsDBNull(7) ? (DateTime?)null : rd.GetDateTime(7)
                            };
                        }
                    }
                }

                if (hoaDon == null) return null;

                // 2. Lấy chi tiết sản phẩm trong đơn
                string sqlCT = @"
            SELECT ct.MASP, sp.TENSP, ct.SOLUONG, ct.DONGIA
            FROM CHITIETHD ct
            JOIN SANPHAM sp ON ct.MASP = sp.MASP
            WHERE ct.MAHD = @mahd";

                using (var cmd = new SqlCommand(sqlCT, conn))
                {
                    cmd.Parameters.AddWithValue("@mahd", maHd);
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            hoaDon.ChiTietHds.Add(new ChiTietHD
                            {
                                MaSp = rd.GetString(0),
                                TenSp = rd.GetString(1),
                                SoLuong = rd.GetInt32(2),
                                DonGia = Convert.ToDouble(rd.GetValue(3))
                            });
                        }
                    }
                }
            }
            return hoaDon;
        }
        public KhachHang GetKhachHangInfo(string maTK)
        {
            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                string sql = @"
                SELECT nd.HOTEN, nd.SDT, nd.DIACHI
                FROM TAIKHOAN tk
                INNER JOIN NGUOIDUNG nd ON tk.ID = nd.ID
                WHERE tk.MATK = @matk";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@matk", maTK);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            return new KhachHang
                            {
                                HoTen = rd.IsDBNull(0) ? "Khách lẻ" : rd.GetString(0),
                                DienThoai = rd.IsDBNull(1) ? "" : rd.GetString(1),
                                DiaChi = rd.IsDBNull(2) ? "" : rd.GetString(2)
                            };
                        }
                    }
                }
            }
            return new KhachHang { HoTen = "Khách lẻ" };
        }
    }
}