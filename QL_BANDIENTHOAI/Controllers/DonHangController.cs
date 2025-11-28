using System.Web.Mvc;
using QL_BANDIENTHOAI.Services;

namespace QL_BANDIENTHOAI.Controllers
{
    public class DonHangController : Controller
    {
        private readonly HeaderService service = new HeaderService();

        // ĐÃ SỬA ĐÚNG 100% – DÙNG Session["UserID"] VÀ "Username"
        private string CurrentUserId => Session["UserID"]?.ToString();
        private string CurrentUsername => Session["Username"]?.ToString();

        public ActionResult Index()
        {
            // KIỂM TRA ĐĂNG NHẬP ĐÚNG CÁCH
            if (string.IsNullOrEmpty(CurrentUserId))
            {
                // Lưu lại trang hiện tại để quay lại sau khi đăng nhập
                Session["ReturnUrl"] = Request.Url?.ToString();
                return RedirectToAction("Login", "Account");
            }

            // LẤY ĐƠN HÀNG THEO MaTK (UserID)
            var orders = service.GetOrders(CurrentUserId);
            ViewBag.Username = CurrentUsername; // hiện tên người dùng nếu cần
            return View(orders);
        }

        // Nếu bạn có Action ChiTiet thì cũng sửa tương tự
        public ActionResult ChiTiet(string madh)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return RedirectToAction("Login", "Account");

            var order = service.GetOrderDetail(madh, CurrentUserId);
            if (order == null) return HttpNotFound();

            return View(order);
        }
    }
}