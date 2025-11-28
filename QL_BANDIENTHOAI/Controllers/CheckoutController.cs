using QL_BANDIENTHOAI.Models.ViewModel;
using QL_BANDIENTHOAI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_BANDIENTHOAI.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly GioHangService _gioHangService = new GioHangService();
        private readonly HeaderService _headerService = new HeaderService();

        public ActionResult Index()
        {
            var maTK = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(maTK)) return RedirectToAction("Login", "Account");

            var cart = _gioHangService.GetCart(maTK);
            var kh = _headerService.GetKhachHangInfo(maTK); // bạn cần thêm hàm này, hoặc lấy từ Session

            var model = new CheckoutViewModel
            {
                CartItems = cart,
                TongTien = cart.Sum(x => x.GiaBan * x.SoLuong),
                SoLuongSP = cart.Sum(x => x.SoLuong),
                HoTen = kh?.HoTen ?? "Khách lẻ",
                SDT = kh?.DienThoai ?? "",
                DiaChi = kh?.DiaChi ?? "",
                Vouchers = _headerService.GetVouchers().Where(v => v.NgayHetHan >= DateTime.Now).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public JsonResult DatHang(string phuongThuc, string maPKM)
        {
            var maTK = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(maTK)) return Json(new { success = false, message = "Chưa đăng nhập" });

            var result = _gioHangService.DatHang(maTK, phuongThuc, maPKM);
            return Json(new { success = result });
        }
    }
}