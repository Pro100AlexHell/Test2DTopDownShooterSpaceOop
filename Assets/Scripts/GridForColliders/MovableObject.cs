using GridForColliders.Core;
using UnityEngine;

namespace GridForColliders
{
    public abstract class MovableObject : IMovableObject
    {
        /// <summary>
        /// Position (center)
        /// </summary>
        public Vector2 Pos { get; private set; }

        /// <summary>
        /// Position delta (in sec) for the current frame (at least, and possibly for subsequent ones)
        /// </summary>
        public Vector2 DeltaPosPerSec;

        protected MovableObject(Vector2 pos)
        {
            Pos = pos;
            DeltaPosPerSec = Vector2.zero;
        }

        /// <summary>
        /// Getting a future position without updating itself (for example, for out-of-bounds checks)
        /// </summary>
        public Vector2 GetFuturePos(float deltaTimeSec)
        {
            return Pos + deltaTimeSec * DeltaPosPerSec;
        }

        /// <summary>
        /// Position update
        /// todo can use ECS for optimization (or custom float[] x,y,dx,dy), but it's harder to maintain
        /// </summary>
        public void UpdatePos(float deltaTimeSec)
        {
            Pos += deltaTimeSec * DeltaPosPerSec;
        }
    }
}