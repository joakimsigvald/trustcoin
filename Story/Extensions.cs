using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Story
{
    public static class Extensions
    {
        public static float? Median(this IEnumerable<(float, float?)> weightedValues)
        {
            return Median(weightedValues
                .Where(wv => wv.Item2.HasValue)
                .Select(wv => (wv.Item1, wv.Item2.Value))
                .ToArray());
        }

        public static float? Median(this IList<(float, float)> weightedValues)
        {
            if (!weightedValues.Any())
                return null;
            var halfTotal = weightedValues.Sum(vv => vv.Item1) / 2;
            var lowerMedian = GetLowerMedian(weightedValues, halfTotal);
            var upperMedian = GetUpperMedian(weightedValues, halfTotal);
            return WeightedSum(lowerMedian, upperMedian) / SumOfWeights(lowerMedian, upperMedian);
        }

        public static TItem SecondOrDefault<TItem>(this IEnumerable<TItem> items)
            => items.Skip(1).FirstOrDefault();

        public static void ForEach<TItem>(this IEnumerable<TItem> items, Action<TItem> action)
            => items.AsList().ForEach(action);

        public static void AddUnique<TItem>(this IList<TItem> list, TItem newItem)
        {
            if (!list.Contains(newItem))
                list.Add(newItem);
        }


        public static TValue SafeGetValue<TKey, TValue>(
            this IDictionary<TKey, TValue> map, 
            TKey key, 
            TValue defaultValue = default(TValue))
            => map.TryGetValue(key, out var value)
                ? value
                : defaultValue;


        public static TValue Drop<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key)
        {
            var value = map[key];
            map.Remove(key);
            return value;
        }

        public static TItem Get<TItem>(this IEnumerable<TItem> items, TItem template)
            => items.SingleOrDefault(item => item.Equals(template));

        private static float WeightedSum(params (float, float)[] weightedValues) 
            => weightedValues.Sum(vv => vv.Item1 * vv.Item2);

        private static (float, float) GetLowerMedian(this IEnumerable<(float, float)> weightedValues, float halfTotal) 
            => GetMedian(weightedValues.OrderByDescending(vv => vv.Item2), halfTotal);

        private static (float, float) GetUpperMedian(this IEnumerable<(float, float)> weightedValues, float halfTotal)
            => GetMedian(weightedValues.OrderBy(vv => vv.Item2), halfTotal);

        private static (float, float) GetMedian(this IEnumerable<(float, float)> orderedweightedValues, float halfTotal)
        {
            var accWeight = 0f;
            var remainingValues = orderedweightedValues
                .SkipWhile(vv => (accWeight += vv.Item1) <= halfTotal).ToArray();
            return GetWeightedMedian(remainingValues);
        }

        private static (float, float) GetWeightedMedian((float, float)[] weightedValues) 
            => (SumOfWeights(weightedValues), weightedValues.First().Item2);

        private static float SumOfWeights(params (float, float)[] weightedValues)
            => weightedValues.Sum(vv => vv.Item1);

        private static List<TItem> AsList<TItem>(this IEnumerable<TItem> items)
            => items as List<TItem> ?? items.ToList();
    }
}