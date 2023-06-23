using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridForColliders.Core
{
    /// <summary>
    /// Grid (of fixed-size cells) to optimize colliders (collisions)
    /// (optimization by reducing the number of objects in one cell (compared to the entire field),
    /// and due to the possibility of independent check of cells)
    /// ------------
    /// todo NOTE: NOT-THREAD-SAFE !
    /// ------------
    /// todo NOTE: one object can belong to several cells (when on the border);
    /// ---
    /// todo NOTE: the cell size is selected approximately,
    /// todo NOTE: so that the cell does not contain too many objects
    /// todo NOTE: (otherwise there will be no optimization compared to checks on the entire field \ without Grid),
    /// todo NOTE: and so that the object does not belong to too many cells
    /// todo NOTE: (otherwise there will be a lot of overhead when moving - changing cells)
    /// todo NOTE: and also take into account that the polygon-collider of the object, used to check belonging to cells
    /// todo NOTE: is essentially a non-continuous-collider (i.e. the object can "pass through" \ skip belonging to cells
    /// todo NOTE: if the cells are too small, and all the points being checked located only on the boundaries of the object)
    /// todo NOTE: (for this case of too small cells, you need to make additional points in the middle / body of the object (and not only its center));
    /// todo NOTE:
    /// todo NOTE: a good heuristic: cell size is slightly larger than the largest object
    /// todo NOTE: (or alternatively larger than the average object, but then there will be objects occupying more than 4 cells)
    /// ---
    /// todo NOTE: if some point of the object goes beyond the grid - this point is not taken into account
    /// todo NOTE: in belonging to the grid, but the object may still belong to the grid (due to other points)
    /// ------------
    /// todo NOTE: related link https://stackoverflow.com/questions/414553/what-technique-should-be-used-to-prune-2d-collision-checks
    /// todo NOTE: alternatively (and suggested by reference) one could mark changed cells (when someone enters/leaves them)
    /// todo NOTE: and check only changed cells for collisions,
    /// todo NOTE: however, for simplicity (and since there are many objects and they move often), current implementation is chosen
    /// ------------
    /// todo NOTE: related link https://gamedev.stackexchange.com/questions/69310/how-to-implement-uniform-grids
    /// ------------
    /// todo NOTE: related link https://web.archive.org/web/20160418004153/http://freespace.virgin.net/hugo.elias/models/m_colide.htm
    /// todo NOTE: alternatively (and suggested by reference) instead of storing in the CellIds object,
    /// todo NOTE: it would be possible to clear (each frame) the entire field and recalculate what is in which cell,
    /// todo NOTE: however, the optimality of both cases needs to be comparedl;
    /// todo NOTE: current implementation is chosen with the storage of CellIds, and the recalculations were optimized (as far as possible)
    /// </summary>
    /// <typeparam name="TObject">Object class in the grid</typeparam>
    /// <typeparam name="TObjectType">Object-type class (as int, for bit mask)</typeparam>
    public class GridForColliders<TObject, TObjectType>
        where TObject : class, IObjectInGridWithCollider<TObjectType>
        where TObjectType : IAsIntForBitMask
    {
        /// <summary>
        /// Cell size (width == height)
        /// </summary>
        private readonly int _cellSize;

        /// <summary>
        /// X size (width) of the entire field
        /// (X object component (to interpret it as "inside the Grid")
        /// belongs to the range [from 0 to fullSizeX exclusive] or [from 0 to fullSizeX-1 inclusive, for int])
        /// </summary>
        private readonly int _fullSizeX;

        /// <summary>
        /// Y size (height) of the entire field
        /// (Y object component (to interpret it as "inside the Grid")
        /// belongs to the range [from 0 to fullSizeY exclusive] or [from 0 to fullSizeY-1 inclusive, for int])
        /// </summary>
        private readonly int _fullSizeY;

        /// <summary>
        /// Number of cells in X (width)
        /// </summary>
        private readonly int _cellsCountX;

        /// <summary>
        /// Number of cells in Y (height)
        /// </summary>
        private readonly int _cellsCountY;

        /// <summary>
        /// List of objects in each cell (by cell ID, which is calculated by coordinates)
        /// </summary>
        private readonly List<TObject>[] _objectsListByCellId;

        // todo choose a good one (starting capacity - number of objects in a cell) (may resize so it's not critical)
        private const int InitialCapacityObjectsInCell = 32;

        public GridForColliders(int cellSize, int fullSizeX, int fullSizeY)
        {
            _cellSize = cellSize;
            _fullSizeX = fullSizeX;
            _fullSizeY = fullSizeY;

            _cellsCountX = Mathf.CeilToInt((float)_fullSizeX / _cellSize); // (round up, remaining cell will still be full size)
            _cellsCountY = Mathf.CeilToInt((float)_fullSizeY / _cellSize);
            int cellsCountTotal = _cellsCountX * _cellsCountY;
            _objectsListByCellId = new List<TObject>[cellsCountTotal];
            for (int cellId = 0; cellId < cellsCountTotal; cellId++)
            {
                _objectsListByCellId[cellId] = new List<TObject>(InitialCapacityObjectsInCell);
            }

            Debug.Log("GridForColliders.ctor()"
                + "\n_cellSize = " + _cellSize
                + "\n_fullSizeX = " + _fullSizeX
                + "\n_fullSizeY = " + _fullSizeY
                + "\n_cellsCountX = " + _cellsCountX
                + "\n_cellsCountY = " + _cellsCountY
                + "\ncellsCountTotal = " + cellsCountTotal);
        }

        /// <summary>
        /// When object is created: trying to place it in the grid
        /// </summary>
        public void OnObjectCreated(TObject obj)
        {
            if (obj.CellIdsCached != null)
            {
                throw new Exception("OnObjectCreated: Assert failed: CellIdsCached should be null");
            }

            obj.CellIdsCached = BuildCellIdsOfObject(obj);
            foreach (int cellId in obj.CellIdsCached)
            {
                if (cellId == -1) break;

                List<TObject> list = _objectsListByCellId[cellId];
                list.Add(obj);
            }
        }

        /// <summary>
        /// When object is removed: trying to remove it from the grid
        /// </summary>
        public void OnObjectRemoved(TObject obj)
        {
            if (obj.CellIdsCached == null)
            {
                throw new Exception("OnObjectRemoved: Assert failed: CellIdsCached should be not-null");
            }

            foreach (int cellId in obj.CellIdsCached)
            {
                if (cellId == -1) break;
                List<TObject> list = _objectsListByCellId[cellId];
                list.Remove(obj);
            }

            ReturnToPoolCellIds(obj.CellIdsCached);
            obj.CellIdsCached = null;
        }

        /// <summary>
        /// When an object moves: trying to recalculate belonging to Grid cells
        /// </summary>
        public void OnObjectAfterMove(TObject obj)
        {
            if (obj.CellIdsCached == null)
            {
                throw new Exception("OnObjectAfterMove: Assert failed: CellIdsCached should be not-null");
            }

            int[] newCellIds = BuildCellIdsOfObject(obj);

            bool hasChanged = false;

            // TODO TEST - fast check !!!
            // todo NOTE: fast check - optimal for most cases (object belongs to 0 or 1 cells, not changed)
            bool isNotChangedFast = (obj.CellIdsCached[0] == newCellIds[0] &&
                                     obj.CellIdsCached[1] == newCellIds[1] && obj.CellIdsCached[1] == -1);
            if (!isNotChangedFast)
            {
                // find changes: added and deleted cells..
                // todo NOTE: Diff Set<int> emulation on array
                //
                // todo NOTE: NOT TRY TO OPTIMIZE !
                // todo NOTE: O(n * m) is reasonable due to the small number of cells the object belongs to (1-2 average, 4-9 worst)
                // todo NOTE: CellIds is NOT SORTED in general case (so can't use O(N) with while and two indices)
                // todo NOTE: changed CellIds count CAN be > 1 in general case

                // .. deleted
                //
                // todo NOTE: iteration optimization
                int cellIdsCachedLength = obj.CellIdsCached.Length;
                int newCellIdsLength = newCellIds.Length;
                for (int i = 0; i < cellIdsCachedLength; i++)
                {
                    int oldCellId = obj.CellIdsCached[i];
                    if (oldCellId == -1) break;

                    // todo NOTE: optimization of: bool isRemoved = (Array.IndexOf(newCellIds, oldCellId) == -1);
                    bool isRemoved = true;
                    for (int n = 0; n < newCellIdsLength; n++)
                    {
                        int newCellId = newCellIds[n];
                        if (newCellId == -1) break;

                        if (newCellId == oldCellId)
                        {
                            isRemoved = false;
                            break;
                        }
                    }
                    //
                    if (isRemoved)
                    {
                        hasChanged = true;
                        List<TObject> list = _objectsListByCellId[oldCellId];
                        list.Remove(obj);
                    }
                }

                // .. added
                //
                // todo NOTE: iteration optimization
                for (int n = 0; n < newCellIdsLength; n++)
                {
                    int newCellId = newCellIds[n];
                    if (newCellId == -1) break;

                    // todo NOTE: optimization of: bool isAdded = (Array.IndexOf(obj.CellIdsCached, newCellId) == -1);
                    bool isAdded = true;
                    for (int i = 0; i < cellIdsCachedLength; i++)
                    {
                        int oldCellId = obj.CellIdsCached[i];
                        if (oldCellId == -1) break;

                        if (newCellId == oldCellId)
                        {
                            isAdded = false;
                            break;
                        }
                    }
                    //
                    if (isAdded)
                    {
                        hasChanged = true;
                        List<TObject> list = _objectsListByCellId[newCellId];
                        list.Add(obj);
                    }
                }
            }

            if (hasChanged)
            {
                //Debug.LogError("OnObjectAfterMove: hasChanged == true; ObjectTypeInGrid = " + obj.ObjectTypeInGrid + "; Pos = {" + obj.Pos.x + "; " + obj.Pos.y + "}"); // todo debug only
                ReturnToPoolCellIds(obj.CellIdsCached);
                obj.CellIdsCached = newCellIds;
            }
            else
            {
                ReturnToPoolCellIds(newCellIds);
            }
        }

        // todo NOTE: NOT-THREAD-SAFE !
        // todo NOTE: Pool int[] for GC optimization
        // todo NOTE: size 4 as the main one, and 9 in case of not perfectly chosen cell size
        private readonly PoolIntArraySized _poolIntArraySized4 = new PoolIntArraySized(4, 1024);
        private readonly PoolIntArraySized _poolIntArraySized9 = new PoolIntArraySized(9, 1024);
        //
        /// <summary>
        /// Building a set of grid cell IDs (int[]) to which an object belongs
        /// </summary>
        private int[] BuildCellIdsOfObject(TObject obj)
        {
            int filledCount = 0;
            int capacity = 4;
            int[] result = _poolIntArraySized4.Get();

            // todo NOTE: iteration optimization
            int objColliderPolygonCachedCount = obj.ColliderPolygonCached.Count;
            for (int i = 0; i < objColliderPolygonCachedCount; i++)
            {
                Vector2 localPos = obj.ColliderPolygonCached[i];

                Vector2 globalPos = obj.Pos + localPos;
                int cellId = GetCellIdByPos(globalPos);
                if (cellId == int.MinValue) continue;

                // todo NOTE: Diff Set<int> emulation on array
                if (Array.IndexOf(result, cellId, 0, filledCount) == -1)
                {
                    if (filledCount == capacity)
                    {
                        // Resize, *Only* for case the cell size is not perfectly chosen
                        if (capacity == 4)
                        {
                            capacity = 9;
                            int[] resultResized = _poolIntArraySized9.Get();
                            Array.Copy(result, 0, resultResized, 0, filledCount);
                            _poolIntArraySized4.ReturnToPool(result);
                            result = resultResized;
                        }
                        else
                        {
                            // todo apparently it was *Very bad* choice of cell size;
                            // todo simplify and do not implement yet
                            //
                            // todo Resize is needed, but not from the Pool, because all sizes cannot be foreseen;
                            // todo it would be possible to allocate 16, 32 (i.e. x2 from the current one after 16),
                            // todo but the use of Grid itself is not so effective in this case,
                            // todo and the logic of returning to the Pool (only 4 and 9) becomes more complicated
                            throw new NotImplementedException("capacity already == 9, Resize not implemented due to pools");
                        }
                    }

                    result[filledCount] = cellId;
                    filledCount++;
                }
            }

            // by contract, fill in the remaining cells -1
            // (NOTE: when returning to the pool \ or when receiving from the pool - do not fill in -1
            // because only here is enough, and the Array.IndexOf check above - taking length into account)
            for (; filledCount < capacity; filledCount++)
            {
                result[filledCount] = -1;
            }

            return result;
        }
        //
        private void ReturnToPoolCellIds(int[] cellIds)
        {
            if (cellIds.Length == 4)
            {
                _poolIntArraySized4.ReturnToPool(cellIds);
            }
            else if (cellIds.Length == 9)
            {
                _poolIntArraySized9.ReturnToPool(cellIds);
            }
            else
            {
                // todo simplified and not yet implemented (associated with allocation in another method, but not implemented there either)
                throw new NotImplementedException("UniversalReturnToPoolCellIds: capacity == " + cellIds.Length);
            }
        }

        /// <summary>
        /// Get cell ID by coordinates
        /// (int.MinValue - out of map bounds)
        /// </summary>
        private int GetCellIdByPos(Vector2 pos)
        {
            // (consistent with Rect.Contains())
            if (pos.x < 0 || pos.y < 0 || pos.x >= _fullSizeX || pos.y >= _fullSizeY)
            {
                //Debug.LogError("GetCellIdByPos: out of bounds; pos = " + pos); // todo debug only
                return int.MinValue;
            }

            int indexX = (int)(pos.x / _cellSize);
            int indexY = (int)(pos.y / _cellSize);
            return indexY * _cellsCountX + indexX;
        }

        /*private Vector2 GetCenterOfCellById(int id)
        {
            // todo
        }*/

        public delegate void OnCollide(TObject obj1, TObject obj2);

        /// <summary>
        /// An attempt to perform collisions of all objects of the specified two types with each other,
        /// main optimization - cells are checked independently of each other
        /// NOTE: implementation with a delegate was chosen (rather than preparing a list of colliding pairs) as a more universal option
        /// </summary>
        public void TryCollide(TObjectType type1, TObjectType type2, OnCollide onCollide)
        {
            // todo NOTE: iteration optimization
            int objectsListByCellIdLength = _objectsListByCellId.Length;
            for (int cellId = 0; cellId < objectsListByCellIdLength; cellId++)
            {
                List<TObject> objectsListInCell = _objectsListByCellId[cellId];
                TryCollideInCell(objectsListInCell, type1, type2, onCollide);
            }
        }

        /// <summary>
        /// An attempt to perform collisions of one object with all other objects of the specified type,
        /// main optimization - cells are checked independently of each other, and only cells where the first object is located are checked
        /// NOTE: implementation with a delegate was chosen (rather than preparing a list of colliding pairs) as a more universal option
        /// </summary>
        public void TryCollideObjectWithOthers(TObject obj1, TObjectType typeOthers, OnCollide onCollide)
        {
            // todo NOTE: iteration optimization
            int obj1CellIdsCachedLength = obj1.CellIdsCached.Length;
            for (int i = 0; i < obj1CellIdsCachedLength; i++)
            {
                int cellId = obj1.CellIdsCached[i];
                if (cellId == -1) break;

                // todo can be optimized further (but for simplicity we do not do it yet)
                // todo to do this, you need to implement an analogue of TryCollideInCell,
                // todo which takes obj1, and does not filter by type obj1.ObjectTypeInGrid
                // todo (leave this method for a case when there is only one entity (for example, a player) of this type)
                List<TObject> objectsListInCell = _objectsListByCellId[cellId];
                TryCollideInCell(objectsListInCell, obj1.ObjectTypeInGrid, typeOthers, onCollide);

                bool isRemovedObj1 = (obj1.CellIdsCached == null);
                if (isRemovedObj1)
                {
                    break;
                }
            }
        }

        private readonly List<int> _reusableListCellIds = new List<int>(64);
        public void GetObjectsInRange(Vector2 pos, float range, TObjectType type, HashSet<TObject> result)
        {
            // sample cells,
            // first - naively find all the cells in the bounding box,
            // [todo NOTE: not implemented yet!] then filter cells which intersects with the circle
            // ..
            float downLeftCornerX = pos.x - range;
            float downLeftCornerY = pos.y - range;
            //
            float upRightCornerX = pos.x + range;
            float upRightCornerY = pos.y + range;
            //
            float stepFromNyquistFrequency = _cellSize / 2f; // NOTE: Nyquist frequency
            //
            _reusableListCellIds.Clear();
            List<int> listAsSetCellIds = _reusableListCellIds;
            //
            for (float tempX = downLeftCornerX; tempX < upRightCornerX; tempX += stepFromNyquistFrequency)
            {
                for (float tempY = downLeftCornerY; tempY < upRightCornerY; tempY += stepFromNyquistFrequency)
                {
                    int cellId = GetCellIdByPos(new Vector2(tempX, tempY));
                    if (cellId == int.MinValue) continue;

                    if (listAsSetCellIds.Contains(cellId)) continue;
                    listAsSetCellIds.Add(cellId);

                    // todo !!! for optimization, not implemented yet
                    /*Vector2 rectCenterOfCell = GetCenterOfCellById(cellId);
                    if (!IntersectCheckers.RectCircle(rectCenterOfCell.x, rectCenterOfCell.y, _cellSize, _cellSize,
                        pos.x, pos.y, range)) continue;*/

                    // todo refactor next lines .. ???

                    List<TObject> objectsListInCell = _objectsListByCellId[cellId];

                    _reusableListForFilter1.Clear();
                    List<TObject> objectsFilteredType = _reusableListForFilter1;
                    FilterObjectsByType(objectsListInCell, objectsFilteredType, type);

                    // todo NOTE: iteration optimization
                    int objectsFilteredTypeCount = objectsFilteredType.Count;
                    for (int n = 0; n < objectsFilteredTypeCount; n++)
                    {
                        TObject obj = objectsFilteredType[n];
                        bool isObjectInRange = obj.CheckCollideWithCircle(pos, range);
                        if (isObjectInRange)
                        {
                            result.Add(obj);
                        }
                    }
                }
            }
        }

        public void GetFirstObjectOnRay(Vector2 rayStart, float rayAngleRad, float maxRange,
            TObjectType type, out TObject resultObject, out Vector2 rayEnd)
        {
            // sample line by cells
            //
            // (for example: _cellSize = 150, stepFromNyquistFrequency = 75,
            // maxRange = 70, stepsCount = 2, sampled points (tempPos): [rayStart, rayStart+75*(cos(a),sin(a))])
            float stepFromNyquistFrequency = _cellSize / 2f; // NOTE: Nyquist frequency
            int stepsCount = Mathf.CeilToInt(maxRange / stepFromNyquistFrequency) + 1;
            float cos = Mathf.Cos(rayAngleRad);
            float sin = Mathf.Sin(rayAngleRad);
            float stepX = cos * stepFromNyquistFrequency;
            float stepY = sin * stepFromNyquistFrequency;
            //
            Vector2 rayEndIntermediate = new Vector2(
                cos * maxRange + rayStart.x, // todo NOTE: infinity is not currently supported
                sin * maxRange + rayStart.y
            );
            //
            float tempX = rayStart.x;
            float tempY = rayStart.y;
            bool isOutOfBoundsPrev = false;
            bool isRayLiesCompletelyOutsideTheMap = true;
            //
            resultObject = null;
            rayEnd = Vector2.zero;
            //
            _reusableListCellIds.Clear();
            List<int> listAsSetCellIds = _reusableListCellIds;
            //
            for (int step = 0; step < stepsCount;
                step++,
                tempX += stepX,
                tempY += stepY)
            {
                Vector2 tempPos = new Vector2(tempX, tempY);
                int cellId = GetCellIdByPos(tempPos);
                bool isOutOfBounds = cellId == int.MinValue;
                if (isOutOfBounds)
                {
                    // todo NOTE: break on ray state change inside->outside;
                    // todo NOTE: not break immediately - for case when ray starts outside the map
                    if (step == 0 || isOutOfBoundsPrev)
                    {
                        isOutOfBoundsPrev = true;
                        continue;
                    }
                    else
                    {
                        isOutOfBoundsPrev = true;
                        break;
                    }
                }
                //
                isOutOfBoundsPrev = false;
                isRayLiesCompletelyOutsideTheMap = false;

                if (listAsSetCellIds.Contains(cellId)) continue;
                listAsSetCellIds.Add(cellId);

                // todo refactor next lines .. ???
                List<TObject> objectsListInCell = _objectsListByCellId[cellId];

                _reusableListForFilter1.Clear();
                List<TObject> objectsFilteredType = _reusableListForFilter1;
                FilterObjectsByType(objectsListInCell, objectsFilteredType, type);

                TObject foundClosestObject;
                Vector2 foundClosestToStartIntersectionPoint;
                float foundClosestAtDistSquared;
                FindSelectedFromListCollidedObjectClosestToStartOfLine(rayStart, rayEndIntermediate,
                    objectsFilteredType, out foundClosestObject,
                    out foundClosestToStartIntersectionPoint, out foundClosestAtDistSquared);
                if (foundClosestObject != null)
                {
                    // (can be found on more than maxRange because we are sampling by cells)
                    if (Mathf.Sqrt(foundClosestAtDistSquared) <= maxRange)
                    {
                        resultObject = foundClosestObject;
                        rayEnd = foundClosestToStartIntersectionPoint;
                    }

                    // break, since we only want the first closest object
                    // (and found the closest object in that cell),
                    // no need to check other cells (which are located further from the start)
                    break;
                }
            }

            if (resultObject == null)
            {
                if (isRayLiesCompletelyOutsideTheMap)
                {
                    // if ray lies completely outside the map -> end point is irrelevant
                    rayEnd = rayStart;
                }
                else
                {
                    if (isOutOfBoundsPrev)
                    {
                        // todo !! implement: if ray ends outside the map -> end point = with border collision
                        // todo !! we do not currently check for collision !!
                        rayEnd = rayEndIntermediate;
                    }
                    else
                    {
                        // if ray ends inside the map -> end point = on maxRange
                        rayEnd = rayEndIntermediate;
                    }
                }
            }
        }

        private void FindSelectedFromListCollidedObjectClosestToStartOfLine(Vector2 lineStart, Vector2 lineEnd,
            List<TObject> objects, out TObject foundClosestObject,
            out Vector2 foundClosestToStartIntersectionPoint, out float foundClosestAtDistSquared)
        {
            foundClosestObject = null;
            foundClosestToStartIntersectionPoint = Vector2.zero;
            foundClosestAtDistSquared = float.MaxValue;

            // todo NOTE: iteration optimization
            int objectsCount = objects.Count;
            for (int n = 0; n < objectsCount; n++)
            {
                TObject obj = objects[n];
                Vector2 closestToStartIntersectionPoint;
                bool isObjectOnLine = obj.CheckCollideWithLine(lineStart, lineEnd, out closestToStartIntersectionPoint);
                if (isObjectOnLine)
                {
                    float distSquared = (lineStart - closestToStartIntersectionPoint).sqrMagnitude;
                    if (foundClosestObject == null || distSquared < foundClosestAtDistSquared)
                    {
                        foundClosestObject = obj;
                        foundClosestToStartIntersectionPoint = closestToStartIntersectionPoint;
                        foundClosestAtDistSquared = distSquared;
                    }
                }
            }
        }

        // todo NOTE: NOT-THREAD-SAFE !
        // todo NOTE: reusable collections for GC optimization
        private const int ReusableListsCapacity = 1024;
        private readonly List<TObject> _reusableListForFilter1 = new List<TObject>(ReusableListsCapacity);
        private readonly List<TObject> _reusableListForFilter2 = new List<TObject>(ReusableListsCapacity);
        private readonly List<bool> _reusableListForSkipFlag = new List<bool>(ReusableListsCapacity);
        //
        private void TryCollideInCell(List<TObject> objectsListInCell,
            TObjectType type1, TObjectType type2, OnCollide onCollide)
        {
            _reusableListForFilter1.Clear();
            List<TObject> objectsFilteredType1 = _reusableListForFilter1;
            FilterObjectsByType(objectsListInCell, objectsFilteredType1, type1);
            //
            if (objectsFilteredType1.Count == 0) return; // fast exit

            _reusableListForFilter2.Clear();
            List<TObject> objectsFilteredType2 = _reusableListForFilter2;
            FilterObjectsByType(objectsListInCell, objectsFilteredType2, type2);

            _reusableListForSkipFlag.Clear();
            List<bool> skipFlagOfObjectsFilteredType2 = _reusableListForSkipFlag;
            for (int n = 0; n < objectsFilteredType2.Count; n++)
            {
                skipFlagOfObjectsFilteredType2.Add(false);
            }

            // todo NOTE: iteration optimization
            int objectsFilteredType1Count = objectsFilteredType1.Count;
            int objectsFilteredType2Count = objectsFilteredType2.Count;
            for (int n1 = 0; n1 < objectsFilteredType1Count; n1++)
            {
                TObject obj1 = objectsFilteredType1[n1];
                for (int n2 = 0; n2 < objectsFilteredType2Count; n2++)
                {
                    if (skipFlagOfObjectsFilteredType2[n2]) continue;

                    TObject obj2 = objectsFilteredType2[n2];
                    bool isCollide = obj1.CheckCollideWithObject(obj2); // todo NOTE: Visitor pattern for double dispatch
                    if (isCollide)
                    {
                        onCollide(obj1, obj2);

                        bool isRemovedObj2 = (obj2.CellIdsCached == null);
                        if (isRemovedObj2)
                        {
                            skipFlagOfObjectsFilteredType2[n2] = true;
                        }

                        bool isRemovedObj1 = (obj1.CellIdsCached == null);
                        if (isRemovedObj1)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private static void FilterObjectsByType(List<TObject> objects, List<TObject> result, TObjectType type)
        {
            // todo NOTE: iteration optimization
            int objectsCount = objects.Count;
            for (int i = 0; i < objectsCount; i++)
            {
                TObject obj = objects[i];
                if ((obj.ObjectTypeInGrid.AsInt() & type.AsInt()) != 0)
                {
                    result.Add(obj);
                }
            }
        }
    }
}