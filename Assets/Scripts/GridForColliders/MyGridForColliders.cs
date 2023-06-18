namespace GridForColliders
{
    // NOTE: as alias
    public class MyGridForColliders : Core.GridForColliders<ObjectInGridWithCollider<ObjectTypeInGrid>, ObjectTypeInGrid>
    {
        public MyGridForColliders(int cellSize, int fullSizeX, int fullSizeY)
            : base(cellSize, fullSizeX, fullSizeY)
        {
        }
    }
}