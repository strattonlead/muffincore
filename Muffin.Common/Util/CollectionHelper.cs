using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using static Muffin.Common.Util.CollectionHelper._Permutation;

namespace Muffin.Common.Util
{
    public static class CollectionHelper
    {
        public static TimeSpan Sum<T>(this IEnumerable<T> collection, Func<T, TimeSpan?> selector)
        {
            var result = TimeSpan.Zero;
            foreach (var item in collection)
            {
                var value = selector(item);
                if (value.HasValue)
                {
                    result += value.Value;
                }
            }
            return result;
        }

        public static IEnumerable<TSource> RearrangeByPattern<TSource, TKey>(this IEnumerable<TSource> list, Func<TSource, TKey> keySelector, IEnumerable<TKey> pattern)
        {
            var groups = list.GroupBy(keySelector).OrderBy(x => pattern.IndexOf(x.Key));
            var maxOccurences = groups.Select(x => x.Count()).Max();

            var result = new List<TSource>();
            for (var i = 0; i < maxOccurences; i++)
            {
                foreach (var group in groups)
                {
                    if (group.Count() > i)
                    {
                        result.Add(group.ElementAt(i));
                    }
                }
            }

            return result;
        }

        public static IEnumerable<TSource> IntersectBy<TSource, TKey>(this IEnumerable<TSource> collection, IEnumerable<TSource> otherCollection, Func<TSource, TKey> predicate)
        {
            var result = new List<TSource>();
            var fastCollection = collection.ToLookup(predicate);
            var grouped = otherCollection.GroupBy(predicate);
            foreach (var group in grouped)
            {
                if (fastCollection.TryGetValue(group.Key, out IEnumerable<TSource> values))
                {
                    result.AddRange(values);
                }
            }
            return result;
        }

        public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> collection, IEnumerable<TSource> otherCollection, Func<TSource, TKey> predicate)
        {
            var result = new List<TSource>();
            var groupedCollection = collection.ToLookup(predicate);
            var groupedOtherCollection = otherCollection.ToLookup(predicate);
            foreach (var group in groupedCollection)
            {
                if (!groupedOtherCollection.Contains(group.Key))
                {
                    result.AddRange(group);
                }
            }
            return result;
        }

        public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    collection.Add(item);
                }
            }
            return collection;
        }

        //public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first,
        //    IEnumerable<TSource> second,
        //    Func<TSource, TKey> keySelector,
        //    IEqualityComparer<TKey>? keyComparer)
        //{
        //    if (first == null) throw new ArgumentNullException(nameof(first));
        //    if (second == null) throw new ArgumentNullException(nameof(second));
        //    if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

        //    return _(); IEnumerable<TSource> _()
        //    {
        //        // TODO Use ToHashSet
        //        var keys = new HashSet<TKey>(second.Select(keySelector), keyComparer);
        //        foreach (var element in first)
        //        {
        //            var key = keySelector(element);
        //            if (keys.Contains(key))
        //                continue;
        //            yield return element;
        //            keys.Add(key);
        //        }
        //    }
        //}

        public static TResult MaxOrDefault<T, TResult>(this IQueryable<T> source,
                                                   Expression<Func<T, TResult>> selector)
        where TResult : struct
        {
            UnaryExpression castedBody = Expression.Convert(selector.Body, typeof(TResult?));
            Expression<Func<T, TResult?>> lambda = Expression.Lambda<Func<T, TResult?>>(castedBody, selector.Parameters);
            return source.Max(lambda) ?? default(TResult);
        }

        public static IEnumerable<T> Clone<T>(this IEnumerable<T> list)
            where T : ICloneable
        {
            return list.Select(x => (T)x.Clone());
        }

        /// <summary>
        /// Splittet eine Collection in kleinere Collections mit N einträgen
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="batchCount"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> items, int batchCount)
        {
            return items
                    .Select((s, i) => new { Value = s, Index = i })
                    .GroupBy(x => x.Index / batchCount)
                    .Select(grp => grp.Select(x => x.Value).ToArray())
                    .ToArray();
        }

        //public static IEnumerable<IEnumerable<T>> SplitBy<T>(this T[] array, int size)
        //{
        //    for (var i = 0; i < (float)array.Length / size; i++)
        //    {
        //        yield return array.Skip(i * size).Take(size);
        //    }
        //}

        public static IEnumerable<IGrouping<int, T>> GroupByWeek<T>(this IEnumerable<T> data, Func<T, DateTime> selector, CultureInfo cultureInfo, CalendarWeekRule calendarWeekRule = CalendarWeekRule.FirstFourDayWeek, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
        {
            if (cultureInfo == null)
            {
                cultureInfo = CultureInfo.InvariantCulture;
            }

            return data.GroupBy(x => WeekProjector(cultureInfo, calendarWeekRule, firstDayOfWeek)(selector(x)));
        }

        public static Func<DateTime, int> WeekProjector(CultureInfo cultureInfo, CalendarWeekRule calendarWeekRule, DayOfWeek firstDayOfWeek)
        {
            return d => cultureInfo.Calendar.GetWeekOfYear(d, calendarWeekRule, firstDayOfWeek);
        }

        public static IEnumerable<IEnumerable<T>> SplitBy<T>(this IEnumerable<T> items, int divider)
        {
            if (divider <= 0)
            {
                throw new ArgumentException("Divider must be greater than 0!");
            }

            if (items == null)
            {
                return null;
            }

            if (items.Count() == 1)
            {
                return new T[][] {
                    items.ToArray()
                };
            }

            var count = items.Count();
            var chunks = (count / divider) + 1;
            var result = new T[divider][];
            for (var i = 0; i < divider; i++)
            {
                result[i] = items.Skip(i * chunks).Take(chunks).ToArray();
            }
            return result;
        }

        public static IEnumerable<T> FillEmptyEntries<T, TKey>(this IEnumerable<T> sourceData, IEnumerable<TKey> allKeys, Func<T, TKey> selector, Func<TKey, T> defaultItemFactory)
        {
            var result = new List<T>();
            foreach (var desiredKey in allKeys)
            {
                var items = sourceData.Where(x => object.Equals(selector(x), desiredKey));
                if (items != null)
                {
                    if (items.Count() > 0)
                    {
                        result.AddRange(items);
                        continue;
                    }
                }
                result.Add(defaultItemFactory(desiredKey));
            }
            return result;
        }

        public static void SetValue<T, TResult>(this IEnumerable<T> sourceData, Func<T, TResult> valueSetter)
        {
            foreach (var item in sourceData)
                valueSetter(item);
        }

        public static T[] MoveNullsToEnd<T>(this T[] model)
        {
            if (model == null)
                return null;

            for (int i = 1; i < model.Length; i++)
                if (model[i - 1] == null)
                    model[i - 1] = model[i];
            return model;
        }

        public static IEnumerable<T> ToIEnumerable<T>(this IEnumerable<T> list)
        {
            return list;
        }

        public static IEnumerable<IEnumerable<T>> Permutations<T>(this IEnumerable<T> list)
        {
            List<List<T>> result = new List<List<T>>();
            if (list.Count() == 1)
            {
                result.Add(list.ToList());
                return result;
            }
            foreach (T element in list)
            {
                var remainingList = new List<T>(list);
                remainingList.Remove(element);
                foreach (List<T> permutation in Permutations<T>(remainingList))
                {
                    permutation.Add(element);
                    result.Add(permutation);
                }
            }
            return result;
        }

        public static IEnumerable<IEnumerable<T>> Permutations<T>(this IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return Permutations(list, length - 1)
                .SelectMany((t) => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        public static double PermutationLength<T>(this IEnumerable<T> list, int length)
        {
            // n hoch k
            var n = (double)list.Count();
            var k = (double)length;

            return Math.Pow(n, k);
        }

        public static void Permutations<T>(this IEnumerable<T> list, int length, Action<IEnumerable<T>> permutation)
        {
            if (length == 1)
            {
                foreach (var t in list)
                    permutation(new T[] { t });
                return;
            }

            new PermutationGenerator<T>(list).GeneratePermutations(length, permutation);
        }

        public class _Permutation
        {
            public class RolloverCounter
            {
                private readonly int _min;
                private readonly int _max;
                private int _value;

                public RolloverCounter(int min, int max, int value)
                {
                    _min = min;
                    _max = max;
                    _value = value;
                }

                public RolloverCounter(int min, int max) : this(min, max, min)
                {
                }

                public int Value { get { return _value; } }

                // increases the counter and returns true if rolledover
                public bool Increase()
                {
                    if (++_value < _max)
                    {
                        return false;
                    }
                    _value = _min;
                    return true;
                }

                // makes things easier
                public static explicit operator int(RolloverCounter rolloverCounter)
                {
                    return rolloverCounter._value;
                }
            }

            public class Odometer
            {
                private readonly int _gearCount;
                private readonly int _min;
                private readonly int _max;

                public Odometer(int gearCount, int min, int max, params int[] gearValues)
                {
                    _gearCount = gearCount;
                    _min = min;
                    _max = max;
                    Gears =
                        gearValues.Length > 0
                        // start at the specified state - for multithreading
                        ? gearValues.Select(x => new RolloverCounter(min, max, x)).ToList()
                        // start at min
                        : new List<RolloverCounter>(gearCount) { new RolloverCounter(min, max) };
                    Max = max;
                }

                public int Max { get; private set; }

                public List<RolloverCounter> Gears { get; private set; }

                // increases the odometer and returns true if rolledover
                public bool Increase()
                {
                    var gear = 0;
                    while (gear < Gears.Count && Gears[gear].Increase())
                    {
                        gear++;

                        // add new gear
                        if (Gears.Count - 1 < gear)
                        {
                            Gears.Add(new RolloverCounter(_min, _max, _min));
                            break;
                        }
                    };

                    // rollover
                    return gear == _gearCount;
                }
            }

            public class PermutationGenerator<T>
            {
                private readonly T[] _wordSet;

                public PermutationGenerator(IEnumerable<T> words)
                {
                    _wordSet = words.ToArray();
                }

                public void GeneratePermutations(int count, Action<IEnumerable<T>> permutation)
                {
                    var odometer = new Odometer(count, 0, _wordSet.Length);
                    do
                    {
                        var result = new T[odometer.Gears.Count];
                        for (var i = 0; i < odometer.Gears.Count; i++)
                        {
                            var gear = odometer.Gears[i];
                            result[i] = _wordSet[(int)gear];
                        }

                        permutation(result);

                    } while (!odometer.Increase());
                }
            }
        }

        public static IEnumerable<T> UnionAll<T>(this IEnumerable<T> source, params IEnumerable<T>[] others)
        {
            var length = source.Count() + (others != null ? others.Sum(x => x.Count()) : 0);
            var result = new T[length];
            for (int i = 0; i < source.Count(); i++)
                result[i] = source.ElementAt(i);

            var cnt = source.Count();
            if (others != null)
                for (int i = 0; i < others.Length; i++)
                    for (int j = 0; j < others[i].Count(); j++)
                        result[cnt++] = others[i].ElementAt(j);

            return result;
        }

        /// <summary>
        /// Generates tree of items from item list
        /// https://stackoverflow.com/questions/19648166/nice-universal-way-to-convert-list-of-items-to-tree
        /// </summary>
        /// 
        /// <typeparam name="T">Type of item in collection</typeparam>
        /// <typeparam name="K">Type of parent_id</typeparam>
        /// 
        /// <param name="collection">Collection of items</param>
        /// <param name="id_selector">Function extracting item's id</param>
        /// <param name="parent_id_selector">Function extracting item's parent_id</param>
        /// <param name="root_id">Root element id</param>
        /// 
        /// <returns>Tree of items</returns>
        public static IEnumerable<TreeItem<T>> GenerateTree<T, K>(
            this IEnumerable<T> collection,
            Func<T, K> id_selector,
            Func<T, K> parent_id_selector,
            K root_id = default(K))
        {
            foreach (var c in collection.Where(c => parent_id_selector(c).Equals(root_id)))
            {
                yield return new TreeItem<T>
                {
                    Item = c,
                    Children = collection.GenerateTree(id_selector, parent_id_selector, id_selector(c))
                };
            }
        }

        public static IEnumerable<T> Reduce<T>(this IEnumerable<TreeItem<T>> tree)
            where T : ITreeItem<T>
        {
            var result = new List<T>();
            foreach (var item in tree)
            {
                result.Add(item.Item);
                if (item.Children != null && item.Children.Any())
                    item.Item.Children = item.Children.Reduce().ToList();
                else
                    item.Item.Children = new List<T>();
            }
            return result;
        }

        public static IEnumerable<T> Iterate<T>(this IEnumerable<T> tree, Action<T> action)
            where T : ITreeItem<T>
        {
            var result = new List<T>();
            foreach (var item in tree)
            {
                action?.Invoke(item);
                if (item.Children != null)
                    item.Children.Iterate(action);
            }
            return tree;
        }

        public interface ITreeItem { }
        public interface ITreeItem<T> : ITreeItem
            where T : ITreeItem
        {
            List<T> Children { get; set; }
        }

        public class TreeItem<T>
        {
            public T Item { get; set; }
            public IEnumerable<TreeItem<T>> Children { get; set; }
        }

        ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        ///https://stackoverflow.com/questions/2471588/how-to-get-index-using-linq
        ///<param name="items">The enumerable to search.</param>
        ///<param name="predicate">The expression to test the items against.</param>
        ///<returns>The index of the first matching item, or -1 if no items match.</returns>
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            int retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }
        ///<summary>Finds the index of the first occurrence of an item in an enumerable.</summary>
        ///https://stackoverflow.com/questions/2471588/how-to-get-index-using-linq
        ///<param name="items">The enumerable to search.</param>
        ///<param name="item">The item to find.</param>
        ///<returns>The index of the first matching item, or -1 if the item was not found.</returns>
        public static int IndexOf<T>(this IEnumerable<T> items, T item) { return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i)); }

        // https://stackoverflow.com/questions/9314172/getting-all-messages-from-innerexceptions
        public static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource> nextItem, Func<TSource, bool> canContinue)
        {
            for (var current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem)
            where TSource : class
        {
            return FromHierarchy(source, nextItem, s => s != null);
        }

        public static Exception[] GetAllInnerExceptions(this Exception exception)
        {
            return exception.FromHierarchy(ex => ex.InnerException).ToArray();
        }

        public static IEnumerable<T> AsEnumerable<T>(this T obj)
        {
            return new T[] { obj }.Where(x => x != null).ToArray();
        }

        public static IEnumerable<TSource> ExceptPredicate<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TSource, bool> compare)
        {
            foreach (var itmFirst in first)
            {
                if (!second.Any(itmsecond => compare(itmFirst, itmsecond)))
                {
                    yield return itmFirst;
                }
            }
            yield break;
        }

        //public static IEnumerable<T> ExceptBy<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        //{
        //    return source.Where(x => !predicate(x));
        //}

        public static IEnumerable<TKey> FindMissing<T, TKey>(this IEnumerable<T> source, IEnumerable<TKey> otherCollection, Func<T, TKey> predicate)
        {
            var existingKeys = source.Select(predicate).Distinct().ToArray();
            return otherCollection.Except(existingKeys);
        }

        public static T[] ArrayInitWithValue<T>(int x, T value)
        {
            var result = new T[x];
            for (int i = 0; i < result.Length; i++)
                result[i] = value;
            return result;
        }

        public static T[][] ArrayInit<T>(int x, int y)
        {
            var result = new T[x][];
            for (int i = 0; i < result.Length; i++)
                result[i] = new T[y];
            return result;
        }

        public static T[][][] ArrayInit<T>(int x, int y, int z)
        {
            var result = new T[x][][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new T[y][];
                for (int j = 0; j < y; j++)
                    result[i][j] = new T[z];
            }

            return result;
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
            {
                action(item);
            }
            return list;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> list, Action<T, int> actionWithIndex)
        {
            int cnt = 0;
            foreach (var item in list)
            {
                actionWithIndex(item, cnt++);
            }
            return list;
        }

        //public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
        //{
        //    int i = 0;
        //    return source.Select(x => selector(x, i++));
        //}


        public static byte[] TrimEnd(this byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);
            Array.Resize(ref array, lastIndex + 1);
            return array;
        }

        public static short[] TrimEnd(this short[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);
            Array.Resize(ref array, lastIndex + 1);
            return array;
        }

        public static char[] TrimEnd(this char[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);
            Array.Resize(ref array, lastIndex + 1);
            return array;
        }

        public static int[] TrimEnd(this int[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);
            Array.Resize(ref array, lastIndex + 1);
            return array;
        }

        public static long[] TrimEnd(this long[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);
            Array.Resize(ref array, lastIndex + 1);
            return array;
        }

        public static float[] TrimEnd(this float[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);
            Array.Resize(ref array, lastIndex + 1);
            return array;
        }

        public static double[] TrimEnd(this double[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);
            Array.Resize(ref array, lastIndex + 1);
            return array;
        }

        public static decimal[] TrimEnd(this decimal[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);
            Array.Resize(ref array, lastIndex + 1);
            return array;
        }

        public static IEnumerable<T> InsertFirst<T>(this IEnumerable<T> list, T item)
        {
            return new T[] { item }.Concat(list);
        }
    }
}
