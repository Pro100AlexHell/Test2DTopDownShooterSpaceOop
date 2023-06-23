using System.Collections.Generic;
using GridForColliders.Core;
using UnityEngine;

namespace GridForColliders
{
    public abstract class ObjectInGridWithCollider<TObjectTypeInGrid> : MovableObject, IObjectInGridWithCollider<TObjectTypeInGrid>
    {
        public int[] CellIdsCached { get; set; }

        public List<Vector2> ColliderPolygonCached { get; protected set; }

        public TObjectTypeInGrid ObjectTypeInGrid { get; }

        protected ObjectInGridWithCollider(Vector2 pos, TObjectTypeInGrid objectTypeInGrid)
            : base(pos)
        {
            ObjectTypeInGrid = objectTypeInGrid;
        }

        public void DebugDumpCellIds()
        {
            Debug.LogWarning("DebugDumpCellIds: ObjectTypeInGrid = " + ObjectTypeInGrid + "; Pos = {" + Pos.x + "; " + Pos.y + "}");
            for (int i = 0; i < CellIdsCached.Length; i++)
            {
                int cellId = CellIdsCached[i];
                if (cellId == -1)
                {
                    Debug.Log("cellId = " + cellId);
                }
                else
                {
                    Debug.LogWarning("cellId = " + cellId);
                }
            }
        }

        public abstract bool CheckCollideWithObject(IObjectInGridWithCollider<TObjectTypeInGrid> other);

        public abstract bool CheckCollideWithCircle(Vector2 pos, float radius);

        public abstract bool CheckCollideWithLine(Vector2 lineStart, Vector2 lineEnd, out Vector2 closestToStartIntersectionPoint);
    }
}