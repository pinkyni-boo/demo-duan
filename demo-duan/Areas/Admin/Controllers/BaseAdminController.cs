using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace demo_duan.Areas.Admin
{
    // AdminAreaAuthorization attribute is defined elsewhere to avoid duplicate definition.

    [Area("Admin")]
    [AdminAreaAuthorization]
    public abstract class BaseAdminController : Controller
    {
        // Các phương thức chung cho Admin controllers có thể được thêm ở đây
    }
}