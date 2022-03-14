using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RoundedDistributionComparison5
{
    internal class Program
    {
        private const int Count = 1000000;
        private const long Ticks = TimeSpan.TicksPerMillisecond / 1000;

        static void Main()
        {
            var d0 = RoundedDistribution.Create(_enumLongData);
            var d1 = RoundedDistribution2<DataTypes>.Create(_enumLongData);

            MeasureFirstImpl();
            Console.WriteLine();
            MeasureNewImpl();
        }

        static void MeasureFirstImpl()
        {
            var total = 0.0;
            var stopWatch = new Stopwatch();
            Dictionary<DataTypes, int> d0 = null;

            for (var i = 0; i < Count; i++)
            {
                stopWatch.Start();

                d0 = RoundedDistribution.Create(_enumLongData).AsIntegerPercents();

                stopWatch.Stop();

                total += stopWatch.ElapsedTicks;
                stopWatch.Reset();
            }

            total /= Ticks;

            Console.WriteLine($"Total: {total:#.00}{Environment.NewLine}Average: {total / Count:##.00} mcs");
        }

        static void MeasureNewImpl()
        {
            var total = 0.0;
            var stopWatch = new Stopwatch();
            IReadOnlyDictionary<DataTypes, double> d0 = null;

            for (var i = 0; i < Count; i++)
            {
                stopWatch.Start();

                d0 = RoundedDistribution2<DataTypes>.Create(_enumLongData);

                stopWatch.Stop();

                total += stopWatch.ElapsedTicks;
                stopWatch.Reset();
            }

            total /= Ticks;

            Console.WriteLine($"Total: {total:#.00}{Environment.NewLine}Average: {total / Count:##.00} mcs");
        }

        private static Dictionary<DataTypes, int> _enumIntData = new()
        {
            { DataTypes.First, 146 },
            { DataTypes.Second, 123 },
            { DataTypes.Third, 323 },
            { DataTypes.Fourth, 982 },
            { DataTypes.Fifth, 457 }
        };
        private static Dictionary<int, int> _intIntData;
        private static Dictionary<long, int> _longIntData;

        private static Dictionary<DataTypes, long> _enumLongData = new()
        {
            { DataTypes.First, 146 },
            { DataTypes.Second, 123 },
            { DataTypes.Third, 323 },
            { DataTypes.Fourth, 982 },
            { DataTypes.Fifth, 457 }
        };
        private static Dictionary<int, long> _intLongData;
        private static Dictionary<long, long> _longLongData;

        private static Dictionary<DataTypes, double> _enumDoubleData;
        private static Dictionary<int, double> _intDoubleData;
        private static Dictionary<long, double> _longDoubleData;
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
