namespace AirBreather.Core.Random
{
    // IMO, the idea of "randomness" is not something that is commonly needed
    // as a dependency in APIs.  Rather, most APIs should depend on something
    // that arbitrarily picks some values.  The "arbitrary picker" could depend
    // on randomness or pseudo-randomness in its implementation, but it doesn't
    // have to.
    public interface IPicker
    {
        int PickFromRange(int minValueInclusive, int rangeSize);
    }
}
