using System.Web;
using System.Web.Optimization;

namespace PrefixRenomObjets
{
    public class BundleConfig
    {
        // Pour plus d'informations sur le regroupement, visitez https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.4.1.js",
                        "~/Scripts/jquery.validate.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Utilisez la version de développement de Modernizr pour le développement et l'apprentissage. Puis, une fois
            // prêt pour la production, utilisez l'outil de génération à l'adresse https://modernizr.com pour sélectionner uniquement les tests dont vous avez besoin.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/js/js").Include(
                      "~/js/bootstrap-{version}.js",
                      "~/Scripts/bootstrap.js",
                      "~/js/bootstrap-select.min.js",
                      "~/js/highstock.js",
                      "~/js/accessibility.js",
                      "~/js/jquery-ui.min.js",
                      "~/js/Date_Hours.js",
                      "~/js/datatables/jquery.dataTables.min.js",
                      "~/js/datatables/dataTables.bootstrap.min.js",
                      "~/js/fileinput.js",
                      "~/js/jqueryupload.js",
                      "~/js/i18n/datepicker-fr.js",
                      "~/js/printThis.js"
                      ));

            bundles.Add(new StyleBundle("~/css/css").Include(
                      "~/css/bootstrap.css",
                      "~/css/datatables/dataTables.bootstrap.css",
                      "~/css/fileinput.css",
                      "~/css/bootstrap-select.min.css",
                      "~/css/nobel.css",
                      "~/css/jquery-ui.css",
                      "~/css/site.css",
                      "~/css/style.css"));
        }
    }
}
