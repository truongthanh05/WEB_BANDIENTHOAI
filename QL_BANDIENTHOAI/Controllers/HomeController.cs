using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using QL_BANDIENTHOAI.Models;
using QL_BANDIENTHOAI.Models.ViewModel;
using QL_BANDIENTHOAI.Services;

namespace QL_BANDIENTHOAI.Controllers
{
    public class HomeController : Controller
    {
        private readonly SanPhamService _spService;
        private readonly DanhGiaService _dgService;

        public HomeController()
        {
            _spService = new SanPhamService();
            _dgService = new DanhGiaService();
        }

        // ===================== TRANG CHỦ =====================
        public ActionResult Index(string search, string maloai)
        {
            var list = _spService.GetAll(search, maloai);

            var data = new List<ProductVM>();
            foreach (var sp in list)
            {
                data.Add(new ProductVM
                {
                    SP = sp,
                    AvgStar = _dgService.GetAvgStar(sp.MaSp),
                    CountStar = _dgService.GetReviewCount(sp.MaSp)
                });
            }

            ViewBag.DanhMuc = _spService.GetCategories();

            return View(data);
        }
    }
}
