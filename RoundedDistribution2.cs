using System;
using System.Collections.Generic;
using System.Linq;

namespace RoundedDistributionComparison5
{
    public static class RoundedDistribution2<TKey>
        where TKey: unmanaged
    {
        private const int MaxSize = 128;
        private const double Epsilon = 1e-8;
        private const double Threshold = 1e-3;

        private static bool IsNegative(int value) => value < 0;
        private static bool IsNegative(long value) => value < 0;
        private static bool IsNegative(double value) => value < Epsilon;
        private static double Summary(double current, int next) => current + next;
        private static double Summary(double current, long next) => current + next;
        private static double Summary(double current, double next) => current + next;

        private static double GetMultiplier(int precision) => 100 * Math.Pow(10, precision);
        private static double GetResultMultiplier(double multiplier, bool asPercents) => asPercents ? multiplier / 100 : multiplier;

        public static IReadOnlyDictionary<TKey, double> Create(IReadOnlyDictionary<TKey, long> elements, int precision = 0, bool asPercents = true)
        {
            var sum = ValidateAndGetSum(elements.Values, precision, IsNegative, Summary);
            if (sum < Epsilon)
                return elements.ToDictionary(e => e.Key, _ => 0.0);

            var multiplier = GetMultiplier(precision);
            var span = elements.Count > MaxSize
                ? new DistributionElement<TKey>[elements.Count]
                : stackalloc DistributionElement<TKey>[elements.Count];
            var remain = InitAndGetRemain(elements, multiplier, sum, ref span);

            return CalculateAsDictionary(ref span, remain, GetResultMultiplier(multiplier, asPercents));
        }

        public static double[] Create(IReadOnlyList<long> elements, int precision = 0, bool asPercents = true)
        {
            var sum = ValidateAndGetSum(elements, precision, IsNegative, Summary);
            if (sum < Epsilon)
                return elements.Count == 0 ? Array.Empty<double>() : new double[elements.Count];

            var multiplier = GetMultiplier(precision);
            var span = elements.Count > MaxSize
                ? new DistributionElement<int>[elements.Count]
                : stackalloc DistributionElement<int>[elements.Count];
            var remain = InitAndGetRemain(elements, multiplier, sum, ref span);

            return CreateArray(ref span, remain, GetResultMultiplier(multiplier, asPercents));
        }

        public static Dictionary<TKey, double> Create(IReadOnlyDictionary<TKey, double> elements, int precision = 0, bool asPercents = true)
        {
            var sum = ValidateAndGetSum(elements.Values, precision, IsNegative, Summary);
            if (sum < Epsilon)
                return elements.ToDictionary(e => e.Key, _ => 0.0);

            var multiplier = GetMultiplier(precision);
            var span = elements.Count > MaxSize
                ? new DistributionElement<TKey>[elements.Count]
                : stackalloc DistributionElement<TKey>[elements.Count];

            var remain = InitAndGetRemain(elements, multiplier, sum, ref span);

            return CalculateAsDictionary(ref span, remain, GetResultMultiplier(multiplier, asPercents));
        }

        public static double[] Create(IReadOnlyList<double> elements, int precision = 0, bool asPercents = true)
        {
            var sum = ValidateAndGetSum(elements, precision, IsNegative, Summary);
            if (sum < Epsilon)
                return elements.Count == 0 ? Array.Empty<double>() : new double[elements.Count];

            var multiplier = GetMultiplier(precision);
            var span = elements.Count > MaxSize
                ? new DistributionElement<int>[elements.Count]
                : stackalloc DistributionElement<int>[elements.Count];

            var remain = InitAndGetRemain(elements, multiplier, sum, ref span);

            return CreateArray(ref span, remain, GetResultMultiplier(multiplier, asPercents));
        }

        public static Dictionary<TKey, double> Create(IReadOnlyDictionary<TKey, int> elements, int precision = 0, bool asPercents = true)
        {
            var sum = ValidateAndGetSum(elements.Values, precision, IsNegative, Summary);
            if (sum < Epsilon)
                return elements.ToDictionary(e => e.Key, _ => 0.0);

            var multiplier = GetMultiplier(precision);
            var span = elements.Count > MaxSize
                ? new DistributionElement<TKey>[elements.Count]
                : stackalloc DistributionElement<TKey>[elements.Count];

            var remain = InitAndGetRemain(elements, precision, sum, ref span);

            return CalculateAsDictionary(ref span, remain, GetResultMultiplier(multiplier, asPercents));
        }

        public static double[] Create(IReadOnlyList<int> elements, int precision = 0, bool asPercents = true)
        {
            var sum = ValidateAndGetSum(elements, precision, IsNegative, Summary);
            if (sum < Epsilon)
                return elements.Count == 0 ? Array.Empty<double>() : new double[elements.Count];

            var multiplier = GetMultiplier(precision);
            var span = elements.Count > MaxSize
                ? new DistributionElement<int>[elements.Count]
                : stackalloc DistributionElement<int>[elements.Count];

            var remain = InitAndGetRemain(elements, precision, sum, ref span);

            return CreateArray(ref span, remain, GetResultMultiplier(multiplier, asPercents));
        }

        private static double ValidateAndGetSum<TValue>(IEnumerable<TValue> elements, int precision, Func<TValue, bool> isNegative, Func<double, TValue, double> summary)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            if (precision is < 0 or > 12)
                throw new ArgumentException("Invalid precision", nameof(precision));

            var maxCount = Math.Pow(10, precision + 1) / 2;
            var sum = 0.0;
            var count = 0;
            foreach (var element in elements)
            {
                if (isNegative(element))
                    throw new ArgumentException("Collection can not contains negative values", nameof(elements));
                
                if (++count > maxCount)
                    throw new ArgumentException("Collection too big for specified precision");

                sum = summary(sum, element);
            }

            return sum;
        }

        private static double[] CreateArray(ref Span<DistributionElement<int>> elements, int remain, double multiplier)
        {
            var result = new double[elements.Length];

            elements.Sort(new DistributionElementRemainDescendingComparer<int>());

            var i = 0;
            while (remain > 0)
            {
                elements[i].Percent++;
                i++;
                remain--;
            }

            elements.Sort(new DistributionElementPercentAscendingComparer<int>());

            i = 0;
            var j = elements.Length - 1;
            while (elements[i].Percent == 0 && elements[i].Remain < Threshold)
                i++;

            while (elements[i].Percent == 0 && elements[i].Remain > 0)
            {
                elements[i].Percent++;
                while (elements[j - 1].Percent == elements[j].Percent)
                    j--;
                elements[j].Percent--;
                if (j <= i)
                    j = elements.Length - 1;
            }

            for (i = 0; i < elements.Length; i++)
            {
                result[elements[i].Key] = elements[i].Percent / multiplier;
            }

            return result;
        }

        private static Dictionary<TKey, double> CalculateAsDictionary(ref Span<DistributionElement<TKey>> elements, int remain, double multiplier)
        {
            var result = new Dictionary<TKey, double>(elements.Length);

            elements.Sort(new DistributionElementRemainDescendingComparer<TKey>());

            var i = 0;
            while (remain > 0)
            {
                elements[i].Percent++;
                i++;
                remain--;
            }

            elements.Sort(new DistributionElementPercentAscendingComparer<TKey>());

            i = 0;
            var j = elements.Length - 1;
            while (elements[i].Percent == 0 && elements[i].Remain < Threshold)
                i++;

            while (elements[i].Percent == 0 && elements[i].Remain > 0)
            {
                elements[i].Percent++;
                while (elements[j - 1].Percent == elements[j].Percent)
                    j--;
                elements[j].Percent--;
                if (j <= i)
                    j = elements.Length - 1;
            }

            for (i = 0; i < elements.Length; i++)
            {
                result[elements[i].Key] = elements[i].Percent / multiplier;
            }

            return result;
        }
        
        private static int InitAndGetRemain(IReadOnlyList<double> elements, double multiplier, double sum, ref Span<DistributionElement<int>> span)
        {
            var remain = 0.0;
            for (var i = 0; i < elements.Count; i++)
            {
                span[i] = new DistributionElement<int>(i, elements[i] * multiplier / sum);
                remain += span[i].Remain;
            }

            return (int)Math.Round(remain);
        }

        private static int InitAndGetRemain(IReadOnlyList<int> elements, double multiplier, double sum, ref Span<DistributionElement<int>> span)
        {
            var remain = 0.0;
            for (var i = 0; i < elements.Count; i++)
            {
                span[i] = new DistributionElement<int>(i, elements[i] * multiplier / sum);
                remain += span[i].Remain;
            }

            return (int)Math.Round(remain);
        }

        private static int InitAndGetRemain(IReadOnlyList<long> elements, double multiplier, double sum, ref Span<DistributionElement<int>> span)
        {
            var remain = 0.0;
            for (var i = 0; i < elements.Count; i++)
            {
                span[i] = new DistributionElement<int>(i, elements[i] * multiplier / sum);
                remain += span[i].Remain;
            }

            return (int)Math.Round(remain);
        }

        private static int InitAndGetRemain(IReadOnlyDictionary<TKey, double> elements, double multiplier, double sum, ref Span<DistributionElement<TKey>> span)
        {
            var remain = 0.0;
            var i = 0;
            foreach (var (key, element) in elements)
            {
                span[i] = new DistributionElement<TKey>(key, element * multiplier / sum);
                remain += span[i].Remain;
                i++;
            }

            return (int)Math.Round(remain);
        }

        private static int InitAndGetRemain(IReadOnlyDictionary<TKey, int> elements, double multiplier, double sum, ref Span<DistributionElement<TKey>> span)
        {
            var remain = 0.0;
            var i = 0;
            foreach (var (key, element) in elements)
            {
                span[i] = new DistributionElement<TKey>(key, element * multiplier / sum);
                remain += span[i].Remain;
                i++;
            }

            return (int)Math.Round(remain);
        }

        private static int InitAndGetRemain(IReadOnlyDictionary<TKey, long> elements, double multiplier, double sum, ref Span<DistributionElement<TKey>> span)
        {
            var remain = 0.0;
            var i = 0;
            foreach (var (key, element) in elements)
            {
                span[i] = new DistributionElement<TKey>(key, element * multiplier / sum);
                remain += span[i].Remain;
                i++;
            }

            return (int)Math.Round(remain);
        }
    }
}