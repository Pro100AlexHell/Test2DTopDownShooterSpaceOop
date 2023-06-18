using System.Collections.Generic;
using UnityEngine;

namespace GridForColliders.Core
{
    public interface IObjectInGridWithCollider<TObjectTypeInGrid> : IMovableObject
    {
        /// <summary>
        /// Grid cell IDs (cached) (virtual set of ints) to which the object belongs
        /// (-1 value means that this and subsequent ids do not belong to the grid)
        /// ---
        /// todo NOTE: GridForColliders is the owner, but here it is only the container
        /// todo NOTE: (better \ optimal than storage in a dictionary in Grid)
        /// </summary>
        int[] CellIdsCached { get; set; }

        /// <summary>
        /// Getting a (cached) list of collider polygon points (convexity is not mandatory)
        /// (in the Local Coordinate System)
        /// todo NOTE: used primarily to check whether an object belongs to a grid cell
        /// todo NOTE: essentially a non-continuous-collider (also see Grid comments)
        /// ---
        /// todo and secondarily, it can be used (but NOT USED yet) for arbitrary collisions of polygons (when the 1st check is passed)
        /// todo (not used for arbitrary collisions of polygons because, for simplicity, everything is a Circle Collider)
        /// </summary>
        List<Vector2> ColliderPolygonCached { get; }

        /// <summary>
        /// Type of object (not class)
        /// </summary>
        TObjectTypeInGrid ObjectTypeInGrid { get; }

        bool CheckCollideWithObject(IObjectInGridWithCollider<TObjectTypeInGrid> other);

        bool CheckCollideWithCircle(Vector2 pos, float radius);

        //bool CheckCollideWithLine(Vector2 posStart, Vector2 posEnd); // todo !!
    }
}