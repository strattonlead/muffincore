using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Muffin.Common.DataTables
{
    public class DataTableOptions
    {
        #region Columns Order Groupping

        [JsonProperty(PropertyName = "columns", NullValueHandling = NullValueHandling.Ignore)]
        public DataTableColumn[] Columns { get; set; }

        [JsonProperty(PropertyName = "order", NullValueHandling = NullValueHandling.Ignore)]
        public DataTableColumnOrder Order { get; set; }

        [JsonProperty(PropertyName = "rowGroup", NullValueHandling = NullValueHandling.Ignore)]
        public DataTableRowGroup RowGroup { get; set; }

        #endregion

        #region Searching

        [JsonProperty(PropertyName = "searchDelay", NullValueHandling = NullValueHandling.Ignore)]
        public int SearchDelay { get; set; }

        [JsonProperty(PropertyName = "deferLoading", NullValueHandling = NullValueHandling.Ignore)]
        public int DeferLoading { get; set; }

        #endregion

        #region Paging

        [JsonProperty(PropertyName = "paging", NullValueHandling = NullValueHandling.Ignore)]
        public bool Paging { get; set; } = true;

        [JsonProperty(PropertyName = "lengthMenu", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(DataTableLengthMenuConverter))]
        public DataTableLengthMenu LengthMenu { get; set; }

        #endregion

        #region Ajax

        [JsonProperty(PropertyName = "processing", NullValueHandling = NullValueHandling.Ignore)]
        public bool Processing { get; set; }

        [JsonProperty(PropertyName = "serverSide", NullValueHandling = NullValueHandling.Ignore)]
        public bool ServerSide { get; set; }

        [JsonProperty(PropertyName = "ajax", NullValueHandling = NullValueHandling.Ignore)]
        public DataTableAjax AjaxDataSource { get; set; }

        #endregion

        #region Scrolling

        [JsonProperty(PropertyName = "servscrollCollapseerSide", NullValueHandling = NullValueHandling.Ignore)]
        public bool ScrollCollapse { get; set; }

        [JsonProperty(PropertyName = "scrollX", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(DataTableValueConverter))]
        public DataTableValue ScrollX { get; set; }

        [JsonProperty(PropertyName = "scrollY", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(DataTableValueConverter))]
        public DataTableValue ScrollY { get; set; }

        #endregion

        #region Language

        [JsonProperty(PropertyName = "language", NullValueHandling = NullValueHandling.Ignore)]
        public DataTableLanguage Language { get; set; }

        #endregion

        #region Extensions

        [JsonProperty(PropertyName = "responsive", NullValueHandling = NullValueHandling.Ignore)]
        public bool Responsive { get; set; }

        [JsonProperty(PropertyName = "fixedHeader", NullValueHandling = NullValueHandling.Ignore)]
        public bool FixedHeader { get; set; }

        [JsonProperty(PropertyName = "buttons", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Buttons { get; set; }

        #endregion
    }

    public class DataTableValue
    {
        public int Value { get; set; }
        public DataTableValueType ValueType { get; set; }

        public enum DataTableValueType
        {
            Px = 0,
            Percent = 1,
            Em = 2
        }
    }

    public class DataTableValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DataTableValue);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var concreteValue = (DataTableValue)value;
            var valueType = "px";
            switch (concreteValue.ValueType)
            {
                case DataTableValue.DataTableValueType.Px:
                    valueType = "px";
                    break;
                case DataTableValue.DataTableValueType.Percent:
                    valueType = "%";
                    break;
                case DataTableValue.DataTableValueType.Em:
                    valueType = "em";
                    break;
                default: break;
            }

            serializer.Serialize(writer, string.Format("{0}{1}", concreteValue.Value, valueType));
        }
    }

    public class DataTableColumn
    {
        [JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }

    public class DataTableLengthMenu
    {
        public List<DataTableLengthMenuEntry> Entries = new List<DataTableLengthMenuEntry>();

        public class DataTableLengthMenuEntry
        {
            public int Value { get; set; }
            public string Name { get; set; }
        }
    }

    public class DataTableLengthMenuConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DataTableLengthMenu);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var tmp = serializer.Deserialize<object[][]>(reader);
            DataTableLengthMenu result = null;
            if (tmp != null)
            {
                result = new DataTableLengthMenu();
                result.Entries = new List<DataTableLengthMenu.DataTableLengthMenuEntry>();
                var values = tmp[0];
                var names = tmp[1];

                for (var i = 0; i < values.Length; i++)
                    result.Entries.Add(new DataTableLengthMenu.DataTableLengthMenuEntry()
                    {
                        Value = (int)values[i],
                        Name = names[i].ToString()
                    });


            }
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var menu = (DataTableLengthMenu)value;
            var values = menu.Entries.Select(x => x.Value).Cast<object>().ToArray();
            var names = menu.Entries.Select(x => x.Name).Cast<object>().ToArray();
            serializer.Serialize(writer, new object[][] { values, names });
        }
    }

    public class DataTableAjax
    {
        [JsonProperty(PropertyName = "url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }

    public class DataTableLanguage
    {
        [JsonProperty(PropertyName = "url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; } // Ajax Url

        [JsonProperty(PropertyName = "lengthMenu", NullValueHandling = NullValueHandling.Ignore)]
        public string LengthMenu { get; set; } // Display _MENU_ records per page

        [JsonProperty(PropertyName = "zeroRecords", NullValueHandling = NullValueHandling.Ignore)]
        public string ZeroRecords { get; set; } // Nothing found - sorry

        [JsonProperty(PropertyName = "info", NullValueHandling = NullValueHandling.Ignore)]
        public string Info { get; set; } // Showing page _PAGE_ of _PAGES_

        [JsonProperty(PropertyName = "infoEmpty", NullValueHandling = NullValueHandling.Ignore)]
        public string InfoEmpty { get; set; } // No records available

        [JsonProperty(PropertyName = "infoFiltered", NullValueHandling = NullValueHandling.Ignore)]
        public string InfoFiltered { get; set; } // (filtered from _MAX_ total records)

        [JsonProperty(PropertyName = "searchPlaceholder", NullValueHandling = NullValueHandling.Ignore)]
        public string SearchPlaceholder { get; set; } // Search for records...
    }

    public class DataTableRowGroup
    {
        [JsonProperty(PropertyName = "dataSrc", NullValueHandling = NullValueHandling.Ignore)]
        public int DataSource { get; set; }
    }

    public class DataTableColumnOrder
    {
        public List<DataTableColumnOrderEntry> Entries = new List<DataTableColumnOrderEntry>();

        public class DataTableColumnOrderEntry
        {
            public int Index { get; set; }
            public string SortOrder { get; set; }
        }
    }

    public class DataTableColumnOrderConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DataTableColumnOrder);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var tmp = serializer.Deserialize<object[][]>(reader);
            DataTableColumnOrder result = null;
            if (tmp != null)
            {
                result = new DataTableColumnOrder();
                result.Entries = tmp.Select(x => new DataTableColumnOrder.DataTableColumnOrderEntry()
                {
                    Index = (int)x[0],
                    SortOrder = (string)x[1]
                }).ToList();
            }
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var menu = (DataTableColumnOrder)value;
            var touples = menu.Entries.Select(x => new object[] { x.Index, x.SortOrder }).ToArray();
            serializer.Serialize(writer, new object[][] { touples });
        }
    }
}
