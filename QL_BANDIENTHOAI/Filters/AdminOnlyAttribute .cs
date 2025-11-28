using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_BANDIENTHOAI.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kiểm tra xem người dùng có phải là admin không
            var user = filterContext.HttpContext.User;

            if (user == null || !user.IsInRole("Admin"))  // Kiểm tra quyền admin
            {
                // Nếu không phải admin, chuyển hướng đến trang chủ hoặc trang lỗi
                filterContext.Result = new RedirectResult("/Home/Index");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}