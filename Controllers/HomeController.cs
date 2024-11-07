using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrefixRenomObjets.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }

    public class ResultSet // Variables de retour
    {
        public bool success { get; set; }
        public string status { get; set; }
        public string statusDetail { get; set; }
        public string errorMessage { get; set; }

        public void MapResultSet(ResultSet resultSet)
        {
            success = resultSet.success;
            status = resultSet.status;
            statusDetail = resultSet.statusDetail;
            errorMessage = resultSet.errorMessage != null ? resultSet.errorMessage.Replace("\r", "").Replace("\n", "") : "";
        }
    }
}