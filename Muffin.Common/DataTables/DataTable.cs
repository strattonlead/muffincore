using Muffin.Common.Util;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Muffin.Common.DataTables
{
    public class DataTable<T>
    {
        public IEnumerable<T> Data { get; protected set; }
        public DataTableDescription<T> ColumnDescription { get; protected set; }
        public string Name { get; protected set; }
        public DataTableOptions Options { get; set; }

        public string TableClass { get; set; } = DEFAULT_TABLE_CLASS;
        public int SearchDelay { get; set; } = 400;
        public int DeferLoading { get; set; } = 0;
        public bool Paging { get; set; }
        public bool Searching { get; set; }
        public bool Ordering { get; set; }
        public bool Processing { get; set; }
        public string[] Buttons { get; set; }
        public bool ServerSide { get; set; }
        public bool Info { get; set; } = true;
        public bool LengthChange { get; set; } = true;
        public bool Responsive { get; set; }
        public bool ScrollX { get; set; }
        public bool FixedHeader { get; set; }
        public int ScrollY { get; set; }
        public bool StateSave { get; set; }

        public string DataSource { get; set; }

        #region Multi Lang

        public DataTableLanguage Language { get; set; }
        /*public string LengthMenu { get; set; } = "_MENU_ Einträge pro Seite";
        public string ZeroRecords { get; set; } = "Keine Einträge gefunden";
        public string Info { get; set; } = "Seite _PAGE_ von _PAGES_";
        public string InfoEmpty { get; set; } = "Keine Einträge verfügbar";
        public string InfoFiltered { get; set; } = "(gefiltert von _MAX_ total Einträgen)";*/

        #endregion

        private const string DEFAULT_TABLE_CLASS = "table table-bordered table-hover dataTable";

        #region Constructor

        public DataTable(string name)
        {
            Name = name;
        }

        public DataTable(string name, IEnumerable<T> data)
        {
            Name = name;
            Data = data;
        }

        public DataTable(string name, DataTableDescription<T> columnDescription)
        {
            Name = name;
            Data = new T[0];
            ColumnDescription = columnDescription;
        }

        public DataTable(string name, IEnumerable<T> data, DataTableDescription<T> columnDescription)
        {
            Name = name;
            Data = data;
            ColumnDescription = columnDescription;
        }

        #endregion

        #region Render

        private const string TABLE_TEMPLATE = "<table class=\"{0}\" id=\"{1}\" style=\"{4}\"><thead><tr>{2}</tr></thead><tbody>{3}</tbody></table>";
        private const string TABLE_HEADER_ROW = "<th class='{1}'>{0}</th>";
        private const string TABLE_BODY_ROW = "<tr>{0}</tr>";
        private const string TABLE_BODY_DATA = "<td>{0}</td>";
        public string RenderTable()
        {
            if (ColumnDescription == null)
                return null;
            if (Data == null)
                return null;

            var headers = ColumnDescription.GetHeaders();
            var tableHeaderRowsBuilder = new StringBuilder();
            foreach (var header in headers)
            {
                var headline = header.DispalyName;
                var sortClass = "";
                if (header.Order != null && !header.Order.ShouldSort)
                    sortClass = "no-sort sorting_disabled";
                tableHeaderRowsBuilder.AppendFormat(TABLE_HEADER_ROW, headline, sortClass);
            }

            var tableHeadRows = tableHeaderRowsBuilder.ToString();

            var tableBodyRowsBuilder = new StringBuilder();
            foreach (var item in Data)
            {
                tableBodyRowsBuilder.Append("<tr>");
                var rows = ColumnDescription.GetRows(item);
                foreach (var row in rows)
                    tableBodyRowsBuilder.AppendFormat(TABLE_BODY_DATA, row);
                tableBodyRowsBuilder.Append("</tr>");
            }

            var tableBodyRows = tableBodyRowsBuilder.ToString();

            var style = Responsive ? "width=100%;" : "";

            return string.Format(TABLE_TEMPLATE, TableClass, Name, tableHeadRows, tableBodyRows, style);
        }

        public string RenderScript()
        {
            var sb = new StringBuilder();
            sb.Append("<script type=\"text/javascript\" charset=\"utf8\">$(document).ready(function(){$('#");
            sb.Append(Name);
            sb.Append("').DataTable( {"); // TODO die Options auslagern 

            sb.AppendFormat("paging: {0},", Paging.ToString().ToLower());
            sb.AppendFormat("searching: {0},", Searching.ToString().ToLower());
            sb.AppendFormat("responsive: {0},", Responsive.ToString().ToLower());
            sb.AppendFormat("info: {0},", Info.ToString().ToLower());
            sb.AppendFormat("lengthChange: {0},", LengthChange.ToString().ToLower());

            if (ScrollX)
                sb.AppendFormat("scrollX: {0},", ScrollX.ToString().ToLower());

            if (ScrollY > 0)
                sb.AppendFormat("scrollY: {0},", ScrollY);

            if (FixedHeader)
                sb.AppendFormat("fixedHeader: {0},", FixedHeader.ToString().ToLower());

            if (StateSave)
                sb.AppendFormat("stateSave: {0},", StateSave.ToString().ToLower());

            if (Searching)
                sb.AppendFormat("searchDelay: {0},", SearchDelay);

            if (DeferLoading != 0)
                sb.AppendFormat("deferLoading: {0},", DeferLoading);

            sb.Append("columnDefs: [{ targets: 'no-sort', orderable: false } ],");
            sb.AppendFormat("ordering: {0},", Ordering.ToString().ToLower());

            var headers = ColumnDescription.GetHeaders();
            if (headers != null)
            {
                var headerItemWithSortInfo = headers.FirstOrDefault(x => x.Order != null && x.Order.ShouldSort);
                if (headerItemWithSortInfo != null)
                {
                    var order = headerItemWithSortInfo.Order;
                    int index = headers.IndexOf(headerItemWithSortInfo);

                    var orderInfo = order.Ascending ? "acs" : "desc";

                    sb.AppendFormat("\"order\": [[ {0}, \"{1}\" ]],", index, orderInfo);
                }
            }

            /*if (!string.IsNullOrWhiteSpace(LengthMenu))
                sb.AppendFormat("lengthMenu: \"{0}\",", LengthMenu);
            if (!string.IsNullOrWhiteSpace(ZeroRecords))
                sb.AppendFormat("zeroRecords: \"{0}\",", ZeroRecords);
            if (!string.IsNullOrWhiteSpace(Info))
                sb.AppendFormat("info: \"{0}\",", Info);
            if (!string.IsNullOrWhiteSpace(InfoEmpty))
                sb.AppendFormat("infoEmpty: \"{0}\",", InfoEmpty);
            if (!string.IsNullOrWhiteSpace(InfoFiltered))
                sb.AppendFormat("infoFiltered: \"{0}\",", InfoFiltered);*/

            //if (ColumnDescription != null
            //    && ColumnDescription.Columns != null
            //    && ColumnDescription.Columns.Any(x => x.Order != null && x.Order.ShouldSort))
            //{
            //    var column = ColumnDescription.Columns.FirstOrDefault(x => x.Order.ShouldSort);
            //    var index = Array.IndexOf(ColumnDescription.Columns, column);
            //    var order = column.Order.Ascending ? "acs" : "desc";
            //    sb.AppendFormat("order:[[{0}, \"{1}\"]],", index, order);
            //}

            if (Language != null && !string.IsNullOrWhiteSpace(Language.Url))
                sb.AppendFormat("language: {{ url: \"{0}\"}},", Language.Url);

            if (Buttons != null && Buttons.Any())
                sb.Append("buttons : ['" + string.Join("','", Buttons) + "'],");

            sb.AppendFormat("processing: {0},", Processing.ToString().ToLower());
            sb.AppendFormat("serverSide: {0}", ServerSide.ToString().ToLower());
            if (ServerSide)
            {
                sb.Append(",ajax: { \"url\": \"");
                sb.Append(DataSource);
                sb.Append("\", \"type\": \"POST\"}");
            }

            sb.Append("} );});</script>");

            return sb.ToString();
        }

        public object GetColumns()
        {
            if (ColumnDescription != null)
                return ColumnDescription.GetHeaders().Select(x => new { title = x.DispalyName }).ToArray();
            return null;
        }

        #endregion
    }
}
