using System;
using System.Collections.Generic;
using GridForColliders.Core;
using UnityEngine;

namespace GridForColliders
{
    public abstract class ObjectInGridWithColliderCircle<TObjectTypeInGrid> : ObjectInGridWithCollider<TObjectTypeInGrid>
    {
        public readonly float CircleColliderRadius;

        /// <summary>
        /// Precision (number of points) for circle approximation
        /// (small number for optimization)
        /// todo it would be possible to calculate Precision from circleColliderRadius, but for simplicity we set it explicitly
        /// </summary>
        protected enum CircleApproximationPrecision
        {
            Point4,
            Point8,
        }

        protected ObjectInGridWithColliderCircle(Vector2 pos, TObjectTypeInGrid objectTypeInGrid,
            float circleColliderRadius, CircleApproximationPrecision circleApproximationPrecision)
            // todo may be add flag: addCenterToColliderPolygon, and for large objects use it in the calculation of belonging to cells (at least such a compromise due to non-continuous-collider)
            : base(pos, objectTypeInGrid)
        {
            CircleColliderRadius = circleColliderRadius;

            ColliderPolygonCached = BuildColliderPolygonByCircleApproximation(
                circleColliderRadius, circleApproximationPrecision);

            //DebugDumpColliderPolygon(pos, objectTypeInGrid); // todo debug only
        }

        private void DebugDumpColliderPolygon(Vector2 pos, TObjectTypeInGrid objectTypeInGrid)
        {
            Debug.LogWarning("DebugDumpColliderPolygon: objectTypeInGrid = " + objectTypeInGrid + "; pos = {" + pos.x + "; " + pos.y + "}");
            for (int i = 0; i < ColliderPolygonCached.Count; i++)
            {
                Vector2 p = ColliderPolygonCached[i];
                Debug.LogWarning("i = " + i + "; p = {" + p.x + "; " + p.y + "}");
            }
        }

        /// <returns>List of points (in local CS) clockwise starting from the leftmost point (Y increases upwards)</returns>
        private static List<Vector2> BuildColliderPolygonByCircleApproximation(
            float radius, CircleApproximationPrecision precision)
        {
            int pointCount;
            switch (precision)
            {
                case CircleApproximationPrecision.Point4:
                    pointCount = 4;
                    break;

                case CircleApproximationPrecision.Point8:
                    pointCount = 8;
                    break;

                default:
                    throw new NotImplementedException("precision = " + precision);
            }

            List<Vector2> result = new List<Vector2>(pointCount);
            float currentRad = Mathf.PI;
            float stepRad = -Mathf.PI * 2 / pointCount;
            for (int i = 0; i < pointCount; i++)
            {
                result.Add(new Vector2(
                    Mathf.Cos(currentRad) * radius,
                    Mathf.Sin(currentRad) * radius
                    ));

                currentRad += stepRad;
            }

            return result;
        }

        public override bool CheckCollideWithObject(IObjectInGridWithCollider<TObjectTypeInGrid> other)
        {
            return other.CheckCollideWithCircle(Pos, CircleColliderRadius);
        }

        public override bool CheckCollideWithCircle(Vector2 pos, float radius)
        {
            return IntersectCheckers.CircleCircle(
                Pos.x, Pos.y, CircleColliderRadius,
                pos.x, pos.y, radius);
        }

        // todo !!
        /*public override bool CheckCollideWithLine(Vector2 posStart, Vector2 posEnd)
        {
        }*/
    }
}