using GridForColliders.Core;

namespace GridForColliders
{
    public class ObjectTypeInGrid : IAsIntForBitMask
    {
        // NOTE: power of two for bit mask
        public static readonly ObjectTypeInGrid Player = new ObjectTypeInGrid(1);
        public static readonly ObjectTypeInGrid Enemy = new ObjectTypeInGrid(1 << 1);
        public static readonly ObjectTypeInGrid Asteroid = new ObjectTypeInGrid(1 << 2);
        public static readonly ObjectTypeInGrid Bullet = new ObjectTypeInGrid(1 << 3);

        private readonly int _value;

        private ObjectTypeInGrid(int value)
        {
            _value = value;
        }

        public int AsInt() => _value;

        public ObjectTypeInGrid Or(ObjectTypeInGrid other)
        {
            return new ObjectTypeInGrid(_value | other._value);
        }
    }
}