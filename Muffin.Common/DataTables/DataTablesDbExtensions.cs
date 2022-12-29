//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;

//namespace Muffin.Common.DataTables
//{
//    public static class DataTablesDbExtensions
//    {
//        public static IEnumerable<T> Filter<T, TEntity>(this DbHelp<T, TEntity> dbHelp, DataTableRequestModel request)
//            where T : BusinessObject
//            where TEntity : class
//        {
//            return dbHelp.Filter(request.Start, request.Length);
//        }

//        public static IEnumerable<T> Filter<T, TEntity>(this DbHelp<T, TEntity> dbHelp, DataTableRequestModel request, Expression<Func<TEntity, bool>> predicate)
//            where T : BusinessObject
//            where TEntity : class
//        {
//            return dbHelp.Filter(predicate, request.Start, request.Length);
//        }

//        public static IEnumerable<T> Filter<T, TEntity, TKey>(this DbHelp<T, TEntity> dbHelp, DataTableRequestModel request, Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TKey>> order)
//            where T : BusinessObject
//            where TEntity : class
//        {
//            return dbHelp.Filter(predicate, order, false, request.Start, request.Length);
//        }

//        public static IEnumerable<T> Filter<T, TEntity, TKey>(this DbHelp<T, TEntity> dbHelp, DataTableRequestModel request, Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TKey>> order, bool descending)
//            where T : BusinessObject
//            where TEntity : class
//        {
//            return dbHelp.Filter(predicate, order, descending, request.Start, request.Length);
//        }

//        public static IEnumerable<T> Filter<T, TEntity>(this DbHelp<T, TEntity> dbHelp, DataTableRequestModel request, DataTableDescription<T> tableDescription)
//            where T : BusinessObject
//            where TEntity : class
//        {
//            var sortInfo = request.SortInfos.FirstOrDefault();
//            var columnName = tableDescription.GetColumnName(sortInfo);
//            return dbHelp.Filter<object>(null, columnName, sortInfo.Descending, request.Start, request.Length);
//        }

//        public static IEnumerable<T> Filter<T, TEntity, TTable>(this DbHelp<T, TEntity> dbHelp, DataTableRequestModel request, DataTableDescription<TTable> tableDescription)
//            where T : BusinessObject
//            where TEntity : class
//        {
//            var sortInfo = request.SortInfos.FirstOrDefault();
//            var columnName = tableDescription.GetColumnName(sortInfo);
//            return dbHelp.Filter<object>(null, columnName, sortInfo.Descending, request.Start, request.Length);
//        }

//        public static IEnumerable<T> Filter<T, TEntity>(this DbHelp<T, TEntity> dbHelp, DataTableRequestModel request, DataTableDescription<T> tableDescription, Expression<Func<TEntity, bool>> predicate)
//            where T : BusinessObject
//            where TEntity : class
//        {
//            var sortInfo = request.SortInfos.FirstOrDefault();
//            var columnName = tableDescription.GetColumnName(sortInfo);
//            if (sortInfo == null)
//                sortInfo = new DataTableRequestModel.SortInfo();
//            return dbHelp.Filter<object>(predicate, columnName, sortInfo.Descending, request.Start, request.Length);
//        }
//    }
//}
