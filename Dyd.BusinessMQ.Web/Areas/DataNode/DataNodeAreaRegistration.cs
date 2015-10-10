using System.Web.Mvc;

namespace Dyd.BusinessMQ.Web.Areas.DataNode
{
    public class DataNodeAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DataNode";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DataNode_default",
                "DataNode/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
