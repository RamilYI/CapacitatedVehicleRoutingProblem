using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using System.Web.Mvc;
//using ActionResult = System.Web.Mvc.ActionResult;
//using Controller = System.Web.Mvc.Controller;

namespace TestMapBox
{
    public class AjaxTestController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FirstAjax()
        {
            return Json();
        }

        private ActionResult Json()
        {
            throw new NotImplementedException();
        }
    }
}
