//using System.Collections.Generic;
//using System.Web.Mvc;

//namespace Muffin.Common.DataTables
//{
//    public static class DataTableHtmlHelper
//    {
//        public static MvcHtmlString DataTable<T>(this HtmlHelper helper, DataTable<T> dataTable)
//        {
//            return new MvcHtmlString(dataTable.RenderTable());
//        }

//        public static MvcHtmlString DataTable<T>(this HtmlHelper helper, string name, IEnumerable<T> data, DataTableDescription<T> columnDescription)
//        {
//            return DataTable(helper, new DataTable<T>(name, data, columnDescription));
//        }

//        public static MvcHtmlString DataTableScript<T>(this HtmlHelper helper, DataTable<T> dataTable)
//        {
//            return new MvcHtmlString(dataTable.RenderScript());
//        }

//        public static MvcHtmlString DataTableScript<T>(this HtmlHelper helper, string name)
//        {
//            return DataTableScript(helper, new DataTable<T>(name));
//        }

//        public static MvcHtmlString DataTableScript<T>(this HtmlHelper helper, DataTable<T> dataTable, string action, string controller, object routeValues)
//        {
//            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
//            dataTable.DataSource = urlHelper.Action(action, controller, routeValues);
//            return new MvcHtmlString(dataTable.RenderScript());
//        }

//        public static MvcHtmlString DataTableScript<T>(this HtmlHelper helper, string name, string action, string controller, object routeValues)
//        {
//            return DataTableScript(helper, new DataTable<T>(name), action, controller, routeValues);
//        }

//        public static MvcHtmlString DataTableName<T>(this HtmlHelper helper, DataTable<T> dataTable)
//        {
//            if (dataTable == null)
//                return MvcHtmlString.Empty;
//            return new MvcHtmlString(dataTable.Name);
//        }
//    }
//}
