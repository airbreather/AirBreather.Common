namespace AirBreather.Core.Utilities
{
    // Inspired by http://stackoverflow.com/a/5450717/1083771
    public static class HashCodeUtility
    {
        // static readonly instead of const
        // means I can change it without having to recompile.
        public static readonly int Seed = 17;

        // Generic solely to avoid boxing / unboxing.
        public static int HashWith<T>(this int hashCode, T nextValue)
        {
            unchecked
            {
                return (hashCode * 31) +
                       (nextValue == null
                            ? 0
                            : nextValue.GetHashCode());
            }
        }
    }
}
