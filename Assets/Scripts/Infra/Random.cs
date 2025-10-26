namespace Infra
{
    public interface IRandom
    {
        int Range(int minInclusive, int maxExclusive);
    }

    public sealed class DefaultRandom : IRandom
    {
        private readonly System.Random r = new();
        public int Range(int a, int b) => r.Next(a, b);
    }
}