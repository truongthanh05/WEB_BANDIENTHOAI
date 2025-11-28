using QL_BANDIENTHOAI.Services;
using System.Linq;
using System.Web.Mvc;

public class CartController : Controller
{
    private readonly GioHangService _cart = new GioHangService();
    private string CurrentUserId => Session["UserID"]?.ToString();

    // TRANG GIỎ HÀNG - KHÔNG ĐƯỢC THÊM SP QUA URL NỮA
    public ActionResult Index()
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return RedirectToAction("Login", "Account");

        var list = _cart.GetCart(CurrentUserId);
        return View(list);
    }

    public ActionResult Add(string masp)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return RedirectToAction("Login", "Account");

        _cart.AddToCart(CurrentUserId, masp);
        return RedirectToAction("Index"); // bình thường
    }

    // ĐÃ SỬA: Remove không reload trang nữa → dùng AJAX
    public ActionResult Remove(string masp)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);

        _cart.Remove(CurrentUserId, masp);
        var newCount = _cart.GetCartCount(CurrentUserId);
        var newTotal = _cart.GetCart(CurrentUserId).Sum(x => x.SoLuong * x.GiaBan);

        return Json(new { success = true, cart = newCount, total = newTotal }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult AddAjax(string masp)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Json(new { success = false, message = "not_login" });

        _cart.AddToCart(CurrentUserId, masp);
        int count = _cart.GetCartCount(CurrentUserId);
        return Json(new { success = true, cart = count });
    }

    // DÙNG CHO AJAX GIẢM SỐ LƯỢNG
    [HttpPost]
    public JsonResult MinusAjax(string masp)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Json(new { success = false });

        _cart.Minus(CurrentUserId, masp);
        int count = _cart.GetCartCount(CurrentUserId);
        return Json(new { success = true, cart = count });
    }
}