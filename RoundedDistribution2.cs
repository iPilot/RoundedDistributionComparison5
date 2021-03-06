using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RoundedDistributionComparison5
{
    public static class RoundedDistributionV2<TKey>
        where TKey : unmanaged
    {
        private const int MaxSize = 128;
        private const double Epsilon = 1e-8;
        private const double Threshold = 1e-3;
        private const int MaxPrecision = 8;
        private const int MaxElements = 1024;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNegative(int value) => value < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNegative(long value) => value < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNegative(double value) => value < Epsilon;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Summary(double current, int next) => current + next;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Summary(double current, long next) => current + next;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Summary(double current, double next) => current + next;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GetFraction(int value, double multiplier, double sum) => value * multiplier / sum;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GetFraction(long value, double multiplier, double sum) => value * multiplier / sum;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GetFraction(double value, double multiplier, double sum) => value * multiplier / sum;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GetMultiplier(int precision) => 100 * Math.Pow(10, precision);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GetResultMultiplier(double multiplier, bool asPercents) => asPercents ? multiplier / 100 : multiplier;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double DefaultMapper(double v) => v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IntMapper(double v) => (int) v;

        /// <summary>
        /// Calculates distribution of dictionary elements with specified precision
        /// </summary>
        /// <typeparam name="TValue">Must be <see cref="int"/>, <see cref="long"/> or <see cref="double"/> otherwise <see cref="InvalidOperationException"></see> is thrown</typeparam>
        /// <param name="elements">Source collection</param>
        /// <param name="precision">Precision of each value in result collection</param>
        /// <param name="asPercents">Convert result to percent in range [0, 100] instead of value in [0; 1]</param>
        /// <returns>New collection with distribution values for keys in source collection</returns>
        public static IDictionary<TKey, double> Create<TValue>(IReadOnlyDictionary<TKey, TValue> elements, int precision = 0, bool asPercents = true) => Create(elements, null, DefaultMapper, precision, asPercents);

        /// <summary>
        /// Calculates distribution of dictionary elements with specified precision
        /// </summary>
        /// <typeparam name="TValue">Must be <see cref="int"/>, <see cref="long"/> or <see cref="double"/> otherwise <see cref="InvalidOperationException"></see> is thrown</typeparam>
        /// <param name="elements">Source collection</param>
        /// <returns>New collection with distribution values for keys in source collection</returns>
        public static IDictionary<TKey, int> CreateAsIntegerPercents<TValue>(IReadOnlyDictionary<TKey, TValue> elements) => Create(elements, null, IntMapper);

        /// <summary>
        /// Calculates distribution of dictionary elements with specified precision
        /// </summary>
        /// <typeparam name="TValue">Must be <see cref="int"/>, <see cref="long"/> or <see cref="double"/> otherwise <see cref="InvalidOperationException"></see> is thrown</typeparam>
        /// <param name="elements">Source collection</param>
        /// <param name="resultCollection">Provided result collection suppresses allocations</param>
        /// <returns>New collection with distribution values for keys in source collection</returns>
        public static IDictionary<TKey, int> CreateAsIntegerPercents<TValue>(IReadOnlyDictionary<TKey, TValue> elements, IDictionary<TKey, int> resultCollection) => Create(elements, resultCollection, IntMapper);

        /// <summary>
        /// Calculates distribution of dictionary elements with specified precision
        /// </summary>
        /// <typeparam name="TValue">Must be <see cref="int"/>, <see cref="long"/> or <see cref="double"/> otherwise <see cref="InvalidOperationException"></see> is thrown</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="elements">Source collection</param>
        /// <param name="mapper">Mapper for result <see cref="double"/> value to required <typeparamref name="TResult"/> output</param>
        /// <param name="precision">Precision of each value in result collection</param>
        /// <param name="asPercents">Convert result to percent in range [0, 100] instead of value in [0; 1]</param>
        /// <returns>New collection with distribution values for keys in source collection</returns>
        public static IDictionary<TKey, TResult> Create<TValue, TResult>(IReadOnlyDictionary<TKey, TValue> elements, Func<double, TResult> mapper, int precision = 0, bool asPercents = true) => Create(elements, null, mapper, precision, asPercents);

        /// <summary>
        /// Calculates distribution of dictionary elements with specified precision
        /// </summary>
        /// <typeparam name="TValue">Must be <see cref="int"/>, <see cref="long"/> or <see cref="double"/> otherwise <see cref="InvalidOperationException"></see> is thrown</typeparam>
        /// <param name="elements">Source collection</param>
        /// <param name="resultCollection">Provided result collection suppresses allocations</param>
        /// <param name="precision">Precision of each value in result collection</param>
        /// <param name="asPercents">Convert result to percent in range [0, 100] instead of value in [0; 1]</param>
        /// <returns>Fills <paramref name="resultCollection"/> by calculated distribution values for corresponding source <typeparamref name="TKey"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IDictionary<TKey, double> Create<TValue>(IReadOnlyDictionary<TKey, TValue> elements, IDictionary<TKey, double> resultCollection, int precision = 0, bool asPercents = true) => Create(elements, resultCollection, DefaultMapper, precision, asPercents);

        /// <summary>
        /// Calculates distribution of dictionary elements with specified precision
        /// </summary>
        /// <typeparam name="TValue">Must be <see cref="int"/>, <see cref="long"/> or <see cref="double"/> otherwise <see cref="InvalidOperationException"></see> is thrown</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="elements">Source collection</param>
        /// <param name="resultCollection">Provided result collection to suppress allocations</param>
        /// <param name="mapper">Mapper for result <see cref="double"/> value to required <typeparamref name="TResult"/> output</param>
        /// <param name="precision">Precision of each value in result collection</param>
        /// <param name="asPercents">Convert result to percent in range [0, 100] instead of value in [0; 1]</param>
        /// <returns>Fills <paramref name="resultCollection"/> by calculated distribution values for corresponding source <typeparamref name="TKey"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IDictionary<TKey, TResult> Create<TValue, TResult>(IReadOnlyDictionary<TKey, TValue> elements, IDictionary<TKey, TResult> resultCollection, Func<double, TResult> mapper, int precision = 0, bool asPercents = true)
        {
            var sum = elements switch
            {
                IReadOnlyDictionary<TKey, int> ints => ValidateAndGetSum(ints.Values, precision, IsNegative, Summary),
                IReadOnlyDictionary<TKey, long> longs => ValidateAndGetSum(longs.Values, precision, IsNegative, Summary),
                IReadOnlyDictionary<TKey, double> doubles => ValidateAndGetSum(doubles.Values, precision, IsNegative, Summary),
                _ => throw new InvalidOperationException($"Type {typeof(TValue)} not supported.")
            };

            if (sum < Epsilon)
                return elements.ToDictionary(e => e.Key, _ => mapper(0.0));

            var multiplier = GetMultiplier(precision);
            var span = elements.Count > MaxSize ? new DistributionElement<TKey>[elements.Count] : stackalloc DistributionElement<TKey>[elements.Count];
            var remain = elements switch
            {
                IReadOnlyDictionary<TKey, int> ints => InitAndGetRemain(ints, GetFraction, multiplier, sum, ref span),
                IReadOnlyDictionary<TKey, long> longs => InitAndGetRemain(longs, GetFraction, multiplier, sum, ref span),
                IReadOnlyDictionary<TKey, double> doubles => InitAndGetRemain(doubles, GetFraction, multiplier, sum, ref span),
                _ => throw new InvalidOperationException($"Type {typeof(TValue)} not supported.")
            };

            return CalculateAsDictionary(ref span, resultCollection, mapper, remain, GetResultMultiplier(multiplier, asPercents));
        }

        /// <summary>
        /// Calculates distribution of collection elements
        /// </summary>
        /// <typeparam name="TValue">Must be <see cref="int"/>, <see cref="long"/> or <see cref="double"/> otherwise <see cref="InvalidOperationException"></see> is thrown</typeparam>
        /// <param name="elements">Source collection</param>
        /// <param name="precision">Precision of each value in result collection</param>
        /// <param name="asPercents">Convert result to percent in range [0, 100] instead of value in [0; 1]</param>
        /// <returns>Array with distribution values for corresponding indexes of source collection</returns>
        public static double[] Create<TValue>(IReadOnlyList<TValue> elements, int precision = 0, bool asPercents = true) => Create(elements, null, DefaultMapper, precision, asPercents);

        /// <summary>
        /// Calculates distribution of collection elements
        /// </summary>
        /// <typeparam name="TValue">Must be <see cref="int"/>, <see cref="long"/> or <see cref="double"/> otherwise <see cref="InvalidOperationException"></see> is thrown</typeparam>
        /// <param name="elements">Source collection</param>
        /// <returns>Array with distribution values for corresponding indexes of source collection</returns>
        public static int[] CreateAsIntegerPercents<TValue>(IReadOnlyList<TValue> elements) => Create(elements, null, IntMapper);

        /// <summary>
        /// Calculates distribution of collection elements
        /// </summary>
        /// <typeparam name="TValue">Must be <see cref="int"/>, <see cref="long"/> or <see cref="double"/> otherwise <see cref="InvalidOperationException"></see> is thrown</typeparam>
        /// <param name="elements">Source collection</param>
        /// <param name="resultArray">Provided result collection to suppress allocations</param>
        /// <returns>Array with distribution values for corresponding indexes of source collection</returns>
        public static int[] CreateAsIntegerPercents<TValue>(IReadOnlyList<TValue> elements, int[] resultArray) => Create(elements, resultArray, IntMapper);

        /// <summary>
        /// Calculates distribution of collection elements
        /// </summary>
        /// <typeparam name="TValue">Must be <see cref="int"/>, <see cref="long"/> or <see cref="double"/> otherwise <see cref="InvalidOperationException"></see> is thrown</typeparam>
        /// <param name="elements">Source collection</param>
        /// <param name="resultCollection">Provided result collection to suppress allocations</param>
        /// <param name="precision">Precision of each value in result collection</param>
        /// <param name="asPercents">Convert result to percent in range [0, 100] instead of value in [0; 1]</param>
        /// <returns>Array with distribution values for corresponding indexes of source collection</returns>
        public static double[] Create<TValue>(IReadOnlyList<TValue> elements, double[] resultCollection, int precision = 0, bool asPercents = true) => Create(elements, resultCollection, DefaultMapper, precision, asPercents);

        /// <summary>
        /// Calculates distribution of collection elements
        /// </summary>
        /// <typeparam name="TValue">Must be <see cref="int"/>, <see cref="long"/> or <see cref="double"/> otherwise <see cref="InvalidOperationException"></see> is thrown</typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="elements">Source collection</param>
        /// <param name="mapper">Function maps <see cref="double"/> to output <typeparamref name="TResult"/></param>
        /// <param name="precision">Precision of each value in result collection</param>
        /// <param name="asPercents">Convert result to percent in range [0, 100] instead of value in [0; 1]</param>
        /// <returns>Array with distribution values for corresponding indexes of source collection</returns>
        public static TResult[] Create<TValue, TResult>(IReadOnlyList<TValue> elements, Func<double, TResult> mapper, int precision = 0, bool asPercents = true) => Create(elements, null, mapper, precision, asPercents);

        /// <summary>
        /// Calculates distribution of collection elements
        /// </summary>
        /// <typeparam name="TValue">Must be <see cref="int"/>, <see cref="long"/> or <see cref="double"/> otherwise <see cref="InvalidOperationException"></see> is thrown</typeparam>
        /// <typeparam name="TResult">Output element type</typeparam>
        /// <param name="elements">Source collection</param>
        /// <param name="resultArray">Provided result collection to suppress allocations</param>
        /// <param name="mapper">Function maps <see cref="double"/> to output <typeparamref name="TResult"/></param>
        /// <param name="precision">Precision of each value in result collection</param>
        /// <param name="asPercents">Convert result to percent in range [0, 100] instead of value in [0; 1]</param>
        /// <returns>Fills <paramref name="resultArray"/> with distribution values for corresponding indexes of source collection</returns>
        public static TResult[] Create<TValue, TResult>(IReadOnlyList<TValue> elements, TResult[] resultArray, Func<double, TResult> mapper, int precision = 0, bool asPercents = true)
        {
            if (resultArray != null && resultArray.Length < elements.Count)
                throw new InvalidOperationException("Source and result collections must have equal length.");

            var sum = elements switch
            {
                IReadOnlyList<int> ints => ValidateAndGetSum(ints, precision, IsNegative, Summary),
                IReadOnlyList<long> longs => ValidateAndGetSum(longs, precision, IsNegative, Summary),
                IReadOnlyList<double> doubles => ValidateAndGetSum(doubles, precision, IsNegative, Summary),
                _ => throw new InvalidOperationException($"Type {typeof(TValue)} not supported.")
            };
            if (sum < Epsilon)
                return elements.Count == 0 ? Array.Empty<TResult>() : new TResult[elements.Count];

            var multiplier = GetMultiplier(precision);
            var span = elements.Count > MaxSize ? new DistributionElement<int>[elements.Count] : stackalloc DistributionElement<int>[elements.Count];
            var remain = elements switch
            {
                IReadOnlyList<int> ints => InitAndGetRemain(ints, GetFraction, multiplier, sum, ref span),
                IReadOnlyList<long> longs => InitAndGetRemain(longs, GetFraction, multiplier, sum, ref span),
                IReadOnlyList<double> doubles => InitAndGetRemain(doubles, GetFraction, multiplier, sum, ref span),
                _ => throw new InvalidOperationException($"Type {typeof(TValue)} not supported.")
            };

            return CalculateAsArray(ref span, resultArray, mapper, remain, GetResultMultiplier(multiplier, asPercents));
        }

        private static double ValidateAndGetSum<TValue>(IEnumerable<TValue> elements, int precision, Func<TValue, bool> isNegative, Func<double, TValue, double> summary)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            if (precision is < 0 or > MaxPrecision)
                throw new ArgumentException("Invalid precision", nameof(precision));

            var maxCount = Math.Min(MaxElements, Math.Pow(10, precision + 1) / 2);
            var sum = 0.0;
            var count = 0;
            foreach (var element in elements)
            {
                if (++count > maxCount)
                    throw new ArgumentException("Collection too big or too big for specified precision");

                if (isNegative(element))
                    throw new ArgumentException("Collection can not contains negative values", nameof(elements));

                sum = summary(sum, element);
            }

            return sum;
        }

        private static TResult[] CalculateAsArray<TResult>(ref Span<DistributionElement<int>> elements, TResult[] resultArray, Func<double, TResult> mapper, int remain, double multiplier)
        {
            resultArray ??= new TResult[elements.Length];

            void Init(int key, TResult value) => resultArray[key] = value;

            ProcessElements(ref elements, remain, Init, mapper, multiplier);

            return resultArray;
        }

        private static IDictionary<TKey, TResult> CalculateAsDictionary<TResult>(ref Span<DistributionElement<TKey>> elements, IDictionary<TKey, TResult> resultCollection, Func<double, TResult> mapper, int remain, double multiplier)
        {
            resultCollection ??= new Dictionary<TKey, TResult>(elements.Length);

            void Init(TKey key, TResult value) => resultCollection[key] = value;

            ProcessElements(ref elements, remain, Init, mapper, multiplier);

            return resultCollection;
        }

        private static int InitAndGetRemain<TValue>(IReadOnlyList<TValue> elements, Func<TValue, double, double, double> elementConverter, double multiplier, double sum, ref Span<DistributionElement<int>> span)
        {
            var remain = 0.0;
            for (var i = 0; i < elements.Count; i++)
            {
                remain += span[i].InitAndGetRemain(i, elementConverter(elements[i], multiplier, sum));
            }

            return (int)Math.Round(remain);
        }

        private static int InitAndGetRemain<TValue>(IReadOnlyDictionary<TKey, TValue> elements, Func<TValue, double, double, double> elementConverter, double multiplier, double sum, ref Span<DistributionElement<TKey>> span)
        {
            var remain = 0.0;
            var i = 0;
            foreach (var (key, element) in elements)
            {
                remain += span[i++].InitAndGetRemain(key, elementConverter(element, multiplier, sum));
            }

            return (int)Math.Round(remain);
        }

        private static void ProcessElements<T, TResult>(ref Span<DistributionElement<T>> elements, int remain, Action<T, TResult> init, Func<double, TResult> mapper, double multiplier)
            where T : unmanaged
        {
            elements.Sort(new DistributionElementRemainDescendingComparer<T>());

            var i = 0;
            while (remain > 0)
            {
                elements[i].Percent++;
                i++;
                remain--;
            }

            elements.Sort(new DistributionElementPercentAscendingComparer<T>());

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
                if (j <= i || elements[j].Percent == 1)
                    j = elements.Length - 1;
            }

            for (i = 0; i < elements.Length; i++)
            {
                init(elements[i].Key, mapper(elements[i].Percent / multiplier));
            }
        }
    }
}