using System;

namespace GetBricked.Gameplay
{
    public sealed class DeterministicRandomService
    {
        private readonly Random random;

        public DeterministicRandomService(int seed)
        {
            Seed = NormalizeSeed(seed);
            random = new Random(Seed);
        }

        public int Seed { get; }

        public float NextFloat()
        {
            return (float)random.NextDouble();
        }

        public float Range(float minInclusive, float maxInclusive)
        {
            if (maxInclusive <= minInclusive)
            {
                return minInclusive;
            }

            return minInclusive + ((maxInclusive - minInclusive) * NextFloat());
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                return minInclusive;
            }

            return random.Next(minInclusive, maxExclusive);
        }

        public bool NextBool()
        {
            return random.Next(0, 2) == 0;
        }

        public DeterministicRandomService Fork(int salt)
        {
            return new DeterministicRandomService(CombineSeed(Seed, salt));
        }

        public static int CombineSeed(int seed, int salt)
        {
            unchecked
            {
                var mixed = NormalizeSeed(seed);
                mixed = (mixed * 486187739) ^ (salt * 16777619);
                return NormalizeSeed(mixed);
            }
        }

        private static int NormalizeSeed(int seed)
        {
            if (seed == int.MinValue)
            {
                return int.MaxValue;
            }

            return Math.Abs(seed);
        }
    }
}
