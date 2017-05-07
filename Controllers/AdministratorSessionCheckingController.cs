﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SampleProject.Controllers
{
    public class AdministratorSessionCheckingController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var activeSession = Session["Administrator"];

            if (activeSession != null)
            {
                base.OnActionExecuting(filterContext);
            }
            else
            {
                filterContext.Result = new RedirectResult(Url.Action("Logout", "Account"));
            }
        }
    }
}