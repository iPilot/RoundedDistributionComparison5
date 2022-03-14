using System;

namespace RoundedDistributionComparison5
{
    internal struct DistributionElement<T>
        where T : unmanaged
    {
        internal readonly T Key;
        internal readonly double Remain;
        internal int Percent;

        internal DistributionElement(T key, double percent)
        {
            Key = key;
            Percent = (int)Math.Truncate(percent);
            Remain = percent - Percent;
        }

        private DistributionElement(T key, int percent)
        {
            Remain = 0;
            Key = key;
            Percent = percent;
        }

        public DistributionElement<T> DecreasePercent() => new(Key, Percent - 1);
        public DistributionElement<T> IncreasePercent() => new(Key, Percent + 1);
        public override string ToString() => $"{Percent} {Remain:F3}";
        public override int GetHashCode() => Key.GetHashCode();
    }
}