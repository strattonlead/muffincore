using System.Collections.Generic;
using System.Linq;

namespace Muffin.Common.DataTables
{
    public class DataTableModel
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public string[][] data { get; set; }
    }

    public class DataTableRequestModel
    {
        public class ColumnInfo
        {
            public string Data { get; set; }
            public string Name { get; set; }
            public bool Searchable { get; set; }
            public bool Orderable { get; set; }
        }

        public class SortInfo
        {
            public int Column { get; set; }
            public string OrderStyle { get; set; }
            public bool Descending
            {
                get
                {
                    return string.Equals(OrderStyle, "desc");
                }
            }
        }

        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }

        private string _searchValue = null;
        public string SearchValue
        {
            get
            {
                if (_searchValue == null && Search != null && Search.ContainsKey("value"))
                    _searchValue = Search["value"];
                return _searchValue;
            }
            set { _searchValue = value; }
        }

        private string _searchRegex = null;
        public string SearchRegex
        {
            get
            {
                if (_searchRegex == null && Search != null && Search.ContainsKey("regex"))
                    _searchRegex = Search["regex"];
                return _searchRegex;
            }
            set { _searchRegex = value; }
        }

        public Dictionary<string, string> Search { get; set; }

        public ColumnInfo[] ColumnInfos { get; set; }
        public SortInfo[] SortInfos { get; set; }

        public DataTableRequestModel() { }

        // TODO
        //public static DataTableRequestModel FromRequest(HttpRequest request)
        //{
        //    var model = new DataTableRequestModel();

        //    model.Draw = Convert.ToInt32(request.Params["draw"]);
        //    model.Start = Convert.ToInt32(request.Params["start"]);
        //    model.Length = Math.Max(Convert.ToInt32(request.Params["length"]), 0);

        //    model.SearchValue = request.Params["search[value]"];
        //    model.SearchRegex = request.Params["search[regex]"];

        //    var columnInfos = new List<ColumnInfo>();
        //    var sortInfos = new List<SortInfo>();
        //    int i = 0;
        //    while (true)
        //    {
        //        var dataKey = string.Format("columns[{0}][data]", i);
        //        var nameKey = string.Format("columns[{0}][name]", i);
        //        var searchableKey = string.Format("columns[{0}][searchable]", i);
        //        var orderableKey = string.Format("columns[{0}][orderable]", i);

        //        var data = request.Params[dataKey];
        //        if (string.IsNullOrWhiteSpace(data))
        //            break;

        //        var name = request.Params[nameKey];
        //        var searchable = Convert.ToBoolean(request.Params[searchableKey]);
        //        var orderable = Convert.ToBoolean(request.Params[orderableKey]);

        //        var orderColumnKey = string.Format("order[{0}][column]", i);
        //        var orderDirKey = string.Format("order[{0}][dir]", i);

        //        var orderColumn = Convert.ToInt32(request.Params[orderColumnKey]);
        //        var orderDir = request.Params[orderDirKey];

        //        if (!string.IsNullOrWhiteSpace(orderDir))
        //        {
        //            sortInfos.Add(new SortInfo()
        //            {
        //                Column = orderColumn,
        //                OrderStyle = orderDir,
        //            });
        //        }

        //        columnInfos.Add(new ColumnInfo()
        //        {
        //            Data = data,
        //            Name = name,
        //            Searchable = searchable,
        //            Orderable = orderable
        //        });
        //        i++;
        //    }
        //    model.ColumnInfos = columnInfos.ToArray();
        //    model.SortInfos = sortInfos.ToArray();

        //    return model;
        //}

        public static DataTableRequestModel Generic()
        {
            return new DataTableRequestModel();
        }

        public DataTableModel Prepare<T>(IEnumerable<T> data, DataTableDescription<T> description)
        {
            var model = new DataTableModel();
            model.draw = Draw;

            model.data = new string[data.Count()][];
            for (int i = 0; i < data.Count(); i++)
            {
                var source = data.ElementAt(i);
                model.data[i] = description.GetRows(source).ToArray();
            }
            return model;
        }
    }
}
