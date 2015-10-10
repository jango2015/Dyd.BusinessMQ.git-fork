using Dyd.BusinessMQ.Domain;
using Dyd.BusinessMQ.Domain.Model.manage;
using Dyd.BusinessMQ.Web.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using XXF.BasicService.CertCenter;
using XXF.Db;
using XXF.ProjectTool;

namespace Dyd.BusinessMQ.Web.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/

        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Login(string appid, string sign, string returnurl)
        {
            #if DEBUG
            {
                Session.Add("user", "admin");
                return RedirectToAction("Config", "Manage");
            }
           
#else
     {
            if (Session["user"] != null)
                return RedirectToAction("Config", "Manage");
            return View();
            }       
#endif
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            if (System.Configuration.ConfigurationManager.AppSettings["admins"].Contains(username + "," + password + ";"))
            {
                Session.Add("user", username);
                return RedirectToAction("Config", "Manage");
            }
            return View();
        }

        //登出
        public ActionResult Logout(string returnurl)
        {
            Session.Remove("user");
            if (string.IsNullOrEmpty(returnurl))
                return Redirect("/Login/Login");
            else
                return Redirect(returnurl);
        }

        
    }
}
