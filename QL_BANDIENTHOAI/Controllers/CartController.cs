using System.Web.Mvc;
using QL_BANDIENTHOAI.Services;

namespace QL_BANDIENTHOAI.Controllers
{
    public class CartController : Controller
    {
        private readonly GioHangService _cart = new GioHangService();

        // ============================
        // ADD TO CART
        // ============================
        public ActionResult Add(string masp)
        {
            // 1. Chưa đăng nhập → chuyển Login
            if (Session["USER"] == null)
            {
                TempData["Msg"] = "Vui lòng đăng nhập để sử dụng giỏ hàng.";
                return RedirectToAction("Login", "Account");
            }

            string matk = Session["USER"].ToString();

            // 2. Gọi service để thêm vào giỏ hàng
            bool ok = _cart.AddToCart(matk, masp);

            if (!ok)
                TempData["Err"] = "Không thể thêm vào giỏ hàng!";

            return RedirectToAction("Index", "Cart");
        }

        // ============================
        // VIEW CART
        // ============================
        public ActionResult Index()
        {
            if (Session["USER"] == null)
                return RedirectToAction("Login", "Account");

            string matk = Session["USER"].ToString();
            var list = _cart.GetCartItems(matk);

            return View(list);
        }
        [HttpPost]
        public JsonResult Addajax(string masp)
        {
            if (Session["USER"] == null)
                return Json(new { success = false, requireLogin = true });

            string matk = Session["USER"].ToString();

            bool ok = _cart.AddToCart(matk, masp);

            int count = _cart.GetCartCount(matk);

            return Json(new { success = ok, cartCount = count });
        }
    }
}
