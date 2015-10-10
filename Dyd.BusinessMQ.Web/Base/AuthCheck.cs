using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Dyd.BusinessMQ.Web.Base
{
    public class AuthCheck : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            try
            {
                if (httpContext.Session["user"] == null)
                    return false;
                else
                    return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }
       
    }
}