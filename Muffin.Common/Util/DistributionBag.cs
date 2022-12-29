using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Muffin.Common.Util
{
    [JsonObject]
    public class DistributionBag<T> : IList<T>
    {
        #region Properties

        public List<DistributionItem<T>> Values { get; set; } = new List<DistributionItem<T>>();

        #endregion

        #region IList

        public T this[int index]
        {
            get
            {
                return Values[index].Value;
            }

            set
            {
                Values[index] = value;
            }
        }

        public int Count
        {
            get
            {
                if (Values.Any())
                    return Values.Sum(x => x.Count);
                return 0;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(T item)
        {
            decimal probability = 1m;
            if (Values.Any())
                probability = Values.Sum(x => x.ProbabilityValue) + 1m / (decimal)Values.Count;

            Add(item, probability);
        }

        public void Add(T item, decimal probability)
        {
            DistributionItem<T> value = item;
            value.ProbabilityValue = probability;
            Values.Add(value);

            ReWeightPropability();
        }

        public void ReWeightPropability()
        {
            decimal probabilitySum = 1m;
            decimal totalCount = 0;
            if (Values.Any())
            {
                probabilitySum = Values.Sum(x => x.ProbabilityValue);
                totalCount = Values.Sum(x => x.Count);
            }

            foreach (var item in Values)
                item.WeightedProbability = item.ProbabilityValue / probabilitySum;

            Values = Values.OrderBy(x => x.WeightedProbability).ToList();
        }

        private void _reWeightUsedItems()
        {
            decimal totalCount = 0;
            if (Values.Any())
                totalCount = Values.Sum(x => x.Count);

            if (totalCount > 0)
            {
                decimal propabilityPart = 1m / totalCount;
                foreach (var item in Values)
                    item.CurrentWeight = item.Count * propabilityPart;
            }
        }

        public T Get()
        {
            if (!Values.Any())
                return default(T);

            var item = Values.OrderByDescending(x => x.PropabilityDelta).FirstOrDefault();
            item.Count++;
            _reWeightUsedItems();

            return item.Value;
        }

        public void Clear()
        {
            Values.Clear();
        }

        public bool Contains(T item)
        {
            return Values.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Values.Select(x => x.Value)
                .ToArray()
                .CopyTo(array, arrayIndex);
        }

        #endregion

        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return Values
                .Select(x => x.Value)
                .GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return Values.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Values.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return Values.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Values.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        #endregion
    }

    #region Container Class

    public class DistributionItem<TValue> : IEquatable<DistributionItem<TValue>>
    {
        public decimal ProbabilityValue { get; set; }
        public decimal WeightedProbability { get; set; }
        public decimal CurrentWeight { get; set; }
        public decimal PropabilityDelta { get { return WeightedProbability - CurrentWeight; } }
        public int Count { get; set; }
        public TValue Value { get; set; }

        public static implicit operator DistributionItem<TValue>(TValue value)
        {
            return new DistributionItem<TValue>() { Value = value };
        }

        public bool Equals(DistributionItem<TValue> other)
        {
            if (this != null && other == null)
                return false;

            if (this.Value == null && other.Value == null)
                return true;

            if (this.Value == null || other.Value == null)
                return false;

            return this.Value.Equals(other.Value);
        }
    }

    #endregion
}
