using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dyd.BusinessMQ.Web.Base
{
    public class CommonHelper
    {
       public static string Help(string text)
        {
            return string.Format("<img class='texthelp' width=\"20\" height=\"20\" title=\"{0}\" style=\"\" src=\"/content/images/help.png\">", text);
        }
    }
}