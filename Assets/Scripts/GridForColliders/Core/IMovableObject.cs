using UnityEngine;

namespace GridForColliders.Core
{
    public interface IMovableObject
    {
        Vector2 Pos { get; }

        void UpdatePos(float deltaTimeSec);
    }
}