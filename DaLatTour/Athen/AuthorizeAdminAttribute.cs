using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaLatTour.Athen
{
    public class AuthorizeAdminAttribute : AuthorizeAttribute
    {
        private readonly string[] allowedRoles;

        // Constructor nhận danh sách các vai trò được phép truy cập
        public AuthorizeAdminAttribute(params string[] roles)
        {
            allowedRoles = roles;
        }

        // Xử lý khi người dùng không có quyền truy cập
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Kiểm tra nếu người dùng chưa đăng nhập thì chuyển đến trang đăng nhập
            if (filterContext.HttpContext.Session["AdminEmail"] == null)
            {
                filterContext.Result = new RedirectResult("~/Auth/AdminLogin");
            }

        }

        // Kiểm tra người dùng có quyền truy cập không
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Lấy thông tin người dùng từ session
            var AdminEmail = httpContext.Session["AdminEmail"];
            var AdminRole = httpContext.Session["AdminRole"];

            // Kiểm tra người dùng đã đăng nhập chưa và có thuộc danh sách vai trò được phép không
            return AdminEmail != null && AdminRole != null && allowedRoles.Contains(AdminRole.ToString());
        }
    }
}
