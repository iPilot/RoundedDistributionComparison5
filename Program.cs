using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RoundedDistributionComparison5
{
    internal class Program
    {
        private const int Count = 1_000_000;
        private const long Ticks = TimeSpan.TicksPerMillisecond / 1000;

        public static void Main()
        {
            var d0 = RoundedDistribution.Create(EnumLongData);
            var d1 = RoundedDistribution2<DataTypes>.Create(EnumLongData);

            MeasureFirstImpl();
            Console.WriteLine();
            MeasureNewImpl();
        }

        private static void MeasureFirstImpl()
        {
            var total = 0.0;
            var stopWatch = new Stopwatch();
            Dictionary<DataTypes, int> d0 = null;

            for (var i = 0; i < Count; i++)
            {
                stopWatch.Start();

                d0 = RoundedDistribution.Create(EnumLongData).AsIntegerPercents();

                stopWatch.Stop();

                total += stopWatch.ElapsedTicks;
                stopWatch.Reset();
            }

            total /= Ticks;

            Console.WriteLine($"Total: {total:#.00}{Environment.NewLine}Average: {total / Count:##.00} mcs");
        }

        private static void MeasureNewImpl()
        {
            var total = 0.0;
            var stopWatch = new Stopwatch();
            IReadOnlyDictionary<DataTypes, double> d0;

            for (var i = 0; i < Count; i++)
            {
                stopWatch.Start();

                d0 = RoundedDistribution2<DataTypes>.Create(EnumLongData);

                stopWatch.Stop();

                total += stopWatch.ElapsedTicks;
                stopWatch.Reset();
            }

            total /= Ticks;

            Console.WriteLine($"Total: {total:#.00}{Environment.NewLine}Average: {total / Count:##.00} mcs");
        }

        private static readonly Dictionary<DataTypes, int> EnumIntData = new()
        {
            { DataTypes.First, 146 },
            { DataTypes.Second, 123 },
            { DataTypes.Third, 323 },
            { DataTypes.Fourth, 982 },
            { DataTypes.Fifth, 457 }
        };

        private static readonly Dictionary<int, int> IntIntData = new()
        {
            { 0, 100 },
            { 1, 200 },
            { 2, 300 },
            { 3, 600 }
        };

        private static Dictionary<long, int> _longIntData = new()
        {
            { 0, 0 },
            { 1, 1_000_000 },
            { 2, 2_000_000 },
            { 3, 6_000_000 },
            { 4, 7_000_000 },
            { 5, 12_000_000 },
            { 6, 3_000_000 }
        };

        private static readonly Dictionary<DataTypes, long> EnumLongData = new()
        {
            { DataTypes.First, 146 },
            { DataTypes.Second, 123 },
            { DataTypes.Third, 323 },
            { DataTypes.Fourth, 982 },
            { DataTypes.Fifth, 457 }
        };

        private static readonly Dictionary<int, long> IntLongData = new()
        {

        };

        private static readonly Dictionary<long, long> LongLongData = new()
        {

        };

        private static readonly Dictionary<DataTypes, double> EnumDoubleData = new()
        {
            { DataTypes.First, 1247.32 },
            { DataTypes.Second, 42365.13 },
            { DataTypes.Third, 32332.243 },
            { DataTypes.Fourth, 12982.112 },
            { DataTypes.Fifth, 22223.125 }
        };

        private static readonly Dictionary<int, double> IntDoubleData = new()
        {

        };

        private static readonly Dictionary<long, double> LongDoubleData = new()
        {
            { 0, 1247.32 },
            { 1, 42365.13 },
            { 2, 32332.243 },
            { 3, 12982.112 },
            { 4, 22223.125 },
            { 5, 23156.786 }
        };
    }

    public enum DataTypes
    {
        First = 0,
        Second = 1,
        Third = 2,
        Fourth = 3,
        Fifth = 4
    }
}
