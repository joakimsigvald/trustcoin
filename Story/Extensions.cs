using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Story
{
    public static class Extensions
    {
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
    }
}