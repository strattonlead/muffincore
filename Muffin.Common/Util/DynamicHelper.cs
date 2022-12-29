//using System.Collections.Generic;
//using System.Dynamic;

//namespace Muffin.Common.Util
//{
//    public class DynamicHelper
//    {
//        public static bool IsPropertyExist(dynamic settings, string name)
//        {
//            if (settings is ExpandoObject)
//                return ((IDictionary<string, object>)settings).ContainsKey(name);

//            return settings.GetType().GetProperty(name) != null;
//        }
//    }
//}
