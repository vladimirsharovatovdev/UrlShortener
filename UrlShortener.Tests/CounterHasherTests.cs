using UrlShortener.Data.Services;
using Xunit;

namespace UrlShortener.Tests;

public class CounterHasherTests
{
    [Theory]
    [InlineData(1, 1582919479)]
    [InlineData(2, 1018355310)]
    [InlineData(530169004, 849485044)]
    public void ScrambleHashesCounter(int counter, int expected)
    {
        long result = CounterHasher.Scramble(counter);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData(1582919479, 1)]
    [InlineData(1018355310, 2)]
    [InlineData(849485044, 530169004)]
    public void UnscrambleHashesCounter(int counter, int expected)
    {
        long result = CounterHasher.Unscramble(counter);
        Assert.Equal(expected, result);
    }
}