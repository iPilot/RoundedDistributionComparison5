using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RoundedDistributionComparison5
{
    public class RoundedDistribution<TKey> : IReadOnlyDictionary<TKey, double>
        where TKey : unmanaged
    {
        private readonly int _precision;
        private const double Epsilon = 1e-8;
        private const double Threshold = 1e-2;
        private readonly Dictionary<TKey, double> _distribution;

        public RoundedDistribution(IDictionary<TKey, long> data, int precision = 0)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (precision is < 0 or > 12)
                throw new ArgumentException("Invalid precision", nameof(precision));
            if (data.Count > Math.Pow(10, precision + 1) / 2) 
                throw new ArgumentException("Collection too big for specified precision");
            if (data.Any(d => d.Value < 0))
                throw new ArgumentException("Collection can not contains negative values", nameof(data));

            _precision = precision;

            DistributionElement<TKey> first;
            var buffer = 0.0;
            var multiplier = 100 * Math.Pow(10, precision);
            var sum = data.Sum(p => p.Value);
            _distribution = data.ToDictionary(d => d.Key, _ => 0.0);

            if (sum == 0)
                return;

            var list = data.Select(p =>
                {
                    var e = new DistributionElement<TKey>(p.Key, p.Value * multiplier / sum);
                    buffer += e.Remain;
                    return e;
                })
                .Where(de => de.Percent > 0 || de.Remain > Threshold)
                .ToList();
            
            var elements = new SortedSet<DistributionElement<TKey>>(list, DistributionElementComparer<TKey>.PercentAscendingComparer);
            list.Sort(DistributionElementComparer<TKey>.PercentDescendingComparer);

            while ((first = elements.First()).Percent == 0)
            {
                if (buffer > Epsilon)
                    buffer--;
                else
                    SubtractFromDonor(list);
                elements.Remove(first);

                var z = 0;
                while (!list[z].Key.Equals(first.Key))
                    z++;
                
                list[z] = list[z].IncreasePercent();
            }

            elements = new SortedSet<DistributionElement<TKey>>(elements, DistributionElementComparer<TKey>.RemainDescendingComparer);
            while (buffer > Epsilon)
            {
                var recipient = elements.First();
                elements.Remove(recipient);

                var z = 0;
                while (!list[z].Key.Equals(recipient.Key))
                    z++;

                list[z] = list[z].IncreasePercent();

                buffer--;
            }
            
            if (list.Sum(e => e.Percent) != (int)multiplier)
                throw new InvalidOperationException("Algorithm error");

            foreach (var element in list)
                _distribution[element.Key] = element.Percent / multiplier;
        }

        private static void SubtractFromDonor(IList<DistributionElement<TKey>> list)
        {
            var i = 0;
            while (list[i].Percent - list[i + 1].Percent < 1)
                i++;

            list[i] = list[i].DecreasePercent();
        }

        public IEnumerator<KeyValuePair<TKey, double>> GetEnumerator() => _distribution.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _distribution.Count;
        public bool ContainsKey(TKey key) => _distribution.ContainsKey(key);
        public bool TryGetValue(TKey key, out double value) => _distribution.TryGetValue(key, out value);
        public double this[TKey key] => _distribution[key];
        public IEnumerable<TKey> Keys => _distribution.Keys;
        public IEnumerable<double> Values => _distribution.Values;
        public Dictionary<TKey, double> AsPercents() => _distribution.ToDictionary(e => e.Key, e => e.Value * 100);
        public Dictionary<TKey, int> AsIntegerPercents() => _precision == 0 
            ? _distribution.ToDictionary(e => e.Key, e => (int)(e.Value * 100)) 
            : throw new InvalidOperationException("Operation not allowed if precision digits is not equal 0");
    }

    public static class RoundedDistribution
    {
        /// <summary>
        /// Создание округленного процентного распределения указанного набора значений с указанной точностью
        /// </summary>
        /// <param name="data">Коллекция значений</param>
        /// <param name="precision">Количество знаков после десятичного разделителя в результате</param>
        /// <returns></returns>
        public static RoundedDistribution<T> Create<T>(IDictionary<T, long> data, int precision = 0)
            where T : unmanaged
        {
            return new RoundedDistribution<T>(data, precision);
        }

        public static RoundedDistribution<int> Create(ICollection<long> data, int precision = 0)
        {
            return new RoundedDistribution<int>(new Dictionary<int, long>(data.Select((e, i) => KeyValuePair.Create(i, e))), precision);
        }
    }
}