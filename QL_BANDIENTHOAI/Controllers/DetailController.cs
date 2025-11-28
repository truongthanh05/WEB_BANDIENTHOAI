using System.Web.Mvc;
using QL_BANDIENTHOAI.Services;

namespace QL_BANDIENTHOAI.Controllers
{
    public class DetailController : Controller
    {
        private readonly DetailService _detail;
        private readonly DanhGiaService _dg;

        public DetailController()
        {
            _detail = new DetailService();
            _dg = new DanhGiaService();
        }

        public ActionResult Index(string masp)
        {
            if (masp == null) return RedirectToAction("Index", "Home");

            var sp = _detail.GetProduct(masp);
            if (sp == null) return HttpNotFound();

            ViewBag.AvgStar = _dg.GetAvgStar(masp);
            ViewBag.ReviewCount = _dg.GetReviewCount(masp);
            ViewBag.Related = _detail.GetRelated(sp.MaLoai, masp);

            return View(sp);
        }
    }
}
