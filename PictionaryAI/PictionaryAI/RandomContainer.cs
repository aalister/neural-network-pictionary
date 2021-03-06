namespace PictionaryAI
{
    public static class RandomContainer
    {
        private static readonly Random _random = new Random();
        
        public static uint GetRandomUint()
        {
            return (uint)_random.Next();
        }

        public static int GetRandomInRange(int start, int end)
        {
            return _random.Next(start, end);
        }
    }
}
