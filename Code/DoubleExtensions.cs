namespace Code
{
    public static class DoubleExtensions
    {
        public static double PercentOff(this double d, int p)
        {
            return d - (d * p / 100);
        }
    }
}
