using System.Web.Mvc;

namespace Dyd.BusinessMQ.Web.Areas.ProConsum
{
    public class ProConsumAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ProConsum";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ProConsum_default",
                "ProConsum/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
