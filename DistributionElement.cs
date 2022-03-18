using System;

namespace RoundedDistributionComparison5
{
    public struct DistributionElement<T>
        where T : unmanaged
    {
        public T Key;
        public double Remain;
        public int Percent;

        public DistributionElement(T key, double percent)
        {
            Key = key;
            Percent = (int)Math.Truncate(percent);
            Remain = percent - Percent;
        }

        public DistributionElement(T key, int percent)
        {
            Remain = 0;
            Key = key;
            Percent = percent;
        }

        public double InitAndGetRemain(T key, double value)
        {
            Key = key;
            Percent = (int)value;
            Remain = value - Percent;
            return Remain;
        }

        public DistributionElement<T> DecreasePercent() => new(Key, Percent - 1);
        public DistributionElement<T> IncreasePercent() => new(Key, Percent + 1);
        public override string ToString() => $"{Percent} {Remain:F3}";
        public override int GetHashCode() => Key.GetHashCode();
    }
}