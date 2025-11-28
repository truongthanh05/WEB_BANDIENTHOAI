using System.Web.Mvc;
using QL_BANDIENTHOAI.Services;

namespace QL_BANDIENTHOAI.Controllers
{
    public class KhuyenMaiController : Controller
    {
        private readonly HeaderService service = new HeaderService();

        public ActionResult Index()
        {
            var list = service.GetVouchers();
            return View(list);
        }
    }
}
