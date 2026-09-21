namespace UrlShortener.Data.Services;

public static class CounterHasher
{
    private const long M = 1_582_919_479;
    private const long MaxId = 1L << 31; // 2 147 483 648

    private static readonly long MInverse = ComputeModInverse(M, MaxId);

    // Принимаем int, но внутри считаем через long
    public static int Scramble(int counter)
    {
        int result = (int)(((long)counter * M) % MaxId);
        return result;
    }

    public static int Unscramble(int scrambled)
    {
        int result = (int)(((long)scrambled * MInverse) % MaxId);
        return result;
    }

    private static long ComputeModInverse(long a, long mod)
    {
        long g = ExtGcd(a, mod, out long x, out _);
        if (g != 1) throw new InvalidOperationException("Not coprime");
        return ((x % mod) + mod) % mod;
    }

    private static long ExtGcd(long a, long b, out long x, out long y)
    {
        if (b == 0) { x = 1; y = 0; return a; }
        long g = ExtGcd(b, a % b, out long x1, out long y1);
        x = y1;
        y = x1 - (a / b) * y1;
        return g;
    }
}