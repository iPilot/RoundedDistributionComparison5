using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RoundedDistributionComparison5
{
    internal readonly struct DistributionElementComparer<T> : IComparer<DistributionElement<T>>
        where T : unmanaged
    {
        public static readonly Comparison<DistributionElement<T>> PercentAsc = (x, y) =>
        {
            var percent = x.Percent.CompareTo(y.Percent);
            var rem = x.Remain.CompareTo(y.Remain);
            return percent != 0
                ? percent
                : rem != 0
                    ? rem
                    : x.GetHashCode().CompareTo(y.GetHashCode());
        };

        public static readonly Comparison<DistributionElement<T>> PercentDesc = (x, y) =>
        {
            var percent = y.Percent.CompareTo(x.Percent);
            var rem = y.Remain.CompareTo(x.Remain);
            return percent != 0
                ? percent
                : rem != 0
                    ? rem
                    : y.Key.GetHashCode().CompareTo(x.Key.GetHashCode());
        };

        public static readonly Comparison<DistributionElement<T>> RemAsc = (x, y) =>
        {
            var rem = x.Remain.CompareTo(y.Remain);
            var percent = y.Percent.CompareTo(x.Percent);

            return rem != 0
                ? rem
                : percent != 0
                    ? percent
                    : x.GetHashCode().CompareTo(y.GetHashCode());
        };

        public static readonly Comparison<DistributionElement<T>> RemDsc = (x, y) =>
        {
            var rem = y.Remain.CompareTo(x.Remain);
            var percent = x.Percent.CompareTo(y.Percent);

            return rem != 0
                ? rem
                : percent != 0
                    ? percent
                    : x.GetHashCode().CompareTo(y.GetHashCode());
        };

        private readonly Comparison<DistributionElement<T>> _comparer;

        private DistributionElementComparer(Comparison<DistributionElement<T>> comparer)
        {
            _comparer = comparer;
        }

        public int Compare(DistributionElement<T> x, DistributionElement<T> y)
        {
            return _comparer(x, y);
        }

        public static IComparer<DistributionElement<T>> PercentAscendingComparer { get; } = new DistributionElementComparer<T>(PercentAsc);
        public static IComparer<DistributionElement<T>> PercentDescendingComparer { get; } = new DistributionElementComparer<T>(PercentDesc);
        public static IComparer<DistributionElement<T>> RemainAscendingComparer { get; } = new DistributionElementComparer<T>(RemAsc);
        public static IComparer<DistributionElement<T>> RemainDescendingComparer { get; } = new DistributionElementComparer<T>(RemDsc);
    }

    internal struct DistributionElementPercentAscendingComparer<T> : IComparer<DistributionElement<T>>
        where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(DistributionElement<T> x, DistributionElement<T> y)
        {
            var percent = x.Percent.CompareTo(y.Percent);
            var rem = x.Remain.CompareTo(y.Remain);

            return percent != 0
                ? percent
                : rem != 0
                    ? rem
                    : x.GetHashCode().CompareTo(y.GetHashCode());
        }
    }

    internal struct DistributionElementPercentDescendingComparer<T> : IComparer<DistributionElement<T>>
        where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(DistributionElement<T> x, DistributionElement<T> y)
        {
            var percent = y.Percent.CompareTo(x.Percent);
            var rem = y.Remain.CompareTo(x.Remain);

            return percent != 0
                ? percent
                : rem != 0
                    ? rem
                    : y.Key.GetHashCode().CompareTo(x.Key.GetHashCode());
        }
    }

    internal struct DistributionElementRemainAscendingComparer<T> : IComparer<DistributionElement<T>>
        where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(DistributionElement<T> x, DistributionElement<T> y)
        {
            var rem = x.Remain.CompareTo(y.Remain);
            var percent = x.Percent.CompareTo(y.Percent);

            return rem != 0
                ? rem
                : percent != 0
                    ? percent
                    : x.GetHashCode().CompareTo(y.GetHashCode());
        }
    }

    internal struct DistributionElementRemainDescendingComparer<T> : IComparer<DistributionElement<T>>
        where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(DistributionElement<T> x, DistributionElement<T> y)
        {
            var rem = y.Remain.CompareTo(x.Remain);
            var percent = x.Percent.CompareTo(y.Percent);

            return rem != 0
                ? rem
                : percent != 0
                    ? percent
                    : x.GetHashCode().CompareTo(y.GetHashCode());
        }
    }
}