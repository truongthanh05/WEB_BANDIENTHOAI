namespace QL_BANDIENTHOAI.Models
{
    public class CartItem
    {
        public string MaSp { get; set; }
        public string TenSp { get; set; }
        public double GiaBan { get; set; }
        public int SoLuong { get; set; }
        public string AnhSanPham { get; set; }

        public double ThanhTien => GiaBan * SoLuong;
    }
}
