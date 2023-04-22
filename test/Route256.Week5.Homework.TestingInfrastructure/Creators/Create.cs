namespace Route256.Week5.Homework.TestingInfrastructure.Creators;

public static class Create
{
    private static long _counter = DateTime.UtcNow.Ticks;
    private static readonly Random StaticRandom = new();

    public static long RandomId() => Interlocked.Increment(ref _counter);

    public static long[] RandomId(int count)
    {
        var ids = new long[count];
        for (int i = 0; i < count; i++)
        {
            ids[i] = RandomId();
        }
        return ids;
    }

    public static double RandomDouble() => StaticRandom.NextDouble();

    public static decimal RandomDecimal() => (decimal)StaticRandom.NextDouble();
}
