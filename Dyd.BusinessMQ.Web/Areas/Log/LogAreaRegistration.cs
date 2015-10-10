using System.Web.Mvc;

namespace Dyd.BusinessMQ.Web.Areas.Log
{
    public class LogAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Log";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Log_default",
                "Log/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
