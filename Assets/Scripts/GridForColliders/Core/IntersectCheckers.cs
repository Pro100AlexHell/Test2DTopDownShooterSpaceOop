using UnityEngine;

namespace GridForColliders.Core
{
    /// <summary>
    /// Functions for intersection \ collision checks of primitives
    /// </summary>
    public static class IntersectCheckers
    {
        public static bool CircleCircle(
            float centerX1, float centerY1, float radius1,
            float centerX2, float centerY2, float radius2)
        {
            var radiusSum = radius1 + radius2;
            var dx = centerX1 - centerX2;
            var dy = centerY1 - centerY2;
            return dx * dx + dy * dy <= radiusSum * radiusSum;
        }

        public static bool RectCircle(
            float rectCenterX, float rectCenterY, float rectWidth, float rectHeight,
            float circleCenterX, float circleCenterY, float circleRadius
            )
        {
            float rectHalfWidth = rectWidth / 2;
            float rectHalfHeight = rectHeight / 2;

            float circleDistanceX = Mathf.Abs(circleCenterX - rectCenterX);
            float circleDistanceY = Mathf.Abs(circleCenterY - rectCenterY);

            if (circleDistanceX > rectHalfWidth + circleRadius) return false;
            if (circleDistanceY > rectHalfHeight + circleRadius) return false;

            if (circleDistanceX <= rectHalfWidth) return true;
            if (circleDistanceY <= rectHalfHeight) return true;

            var nx = circleDistanceX - rectHalfWidth;
            var ny = circleDistanceY - rectHalfHeight;
            return nx * nx + ny * ny <= circleRadius * circleRadius;
        }

        /// <summary>
        /// NOTE: based on https://stackoverflow.com/questions/23016676/line-segment-and-circle-intersection
        /// and https://math.stackexchange.com/questions/275529/check-if-line-intersects-with-circles-perimeter
        /// with additional check (strict 'line segment'): 't1' \ 't2' must be in range [0--1] for including (1st, 2nd) intersection point
        /// </summary>
        /// <returns>hasIntersection</returns>
        public static bool CheckAndGetClosestToStartIntersectionPointOfLineSegmentAndCircle(Vector2 lineStart, Vector2 lineEnd,
            float circleCenterX, float circleCenterY, float circleRadius,
            out Vector2 closestToStartIntersectionPoint)
        {
            float dx = lineEnd.x - lineStart.x;
            float dy = lineEnd.y - lineStart.y;

            float ax = lineStart.x - circleCenterX;
            float ay = lineStart.y - circleCenterY;

            float A = dx * dx + dy * dy;
            float B = 2 * (dx * ax + dy * ay);
            float C = ax * ax + ay * ay - circleRadius * circleRadius;

            float det = B * B - 4 * A * C;

            if (A <= 0.0000001 || det < 0) // todo NOTE: magic const epsilon, may be better float.Epsilon ?
            {
                // no intersections
                closestToStartIntersectionPoint = Vector2.zero;
                return false;
            }
            else if (det == 0) // tangent line
            {
                // one intersection - if 't' in range [0--1], otherwise - no line segment intersection
                float t = -B / (2 * A);
                if (t > 0 && t < 1)
                {
                    closestToStartIntersectionPoint = new Vector2(lineStart.x + t * dx, lineStart.y + t * dy);
                    return true;
                }
                closestToStartIntersectionPoint = Vector2.zero;
                return false;
            }
            else
            {
                // max two intersections - if 't1' and 't2' in range [0--1],
                // otherwise - less then two (possibly no line segment intersection)
                float sqrtDet = Mathf.Sqrt(det);
                float t1 = (-B + sqrtDet) / (2 * A);
                float t2 = (-B - sqrtDet) / (2 * A);
                if (t1 > 0 && t1 < 1 &&
                    t2 > 0 && t2 < 1)
                {
                    Vector2 intersection1 = new Vector2(lineStart.x + t1 * dx, lineStart.y + t1 * dy);
                    Vector2 intersection2 = new Vector2(lineStart.x + t2 * dx, lineStart.y + t2 * dy);
                    float distSquared1 = (lineStart - intersection1).sqrMagnitude;
                    float distSquared2 = (lineStart - intersection2).sqrMagnitude;
                    closestToStartIntersectionPoint = (distSquared1 < distSquared2) ? intersection1 : intersection2;
                    return true;
                }
                else if (t1 > 0 && t1 < 1)
                {
                    closestToStartIntersectionPoint = new Vector2(lineStart.x + t1 * dx, lineStart.y + t1 * dy);
                    return true;
                }
                else if (t2 > 0 && t2 < 1)
                {
                    closestToStartIntersectionPoint = new Vector2(lineStart.x + t2 * dx, lineStart.y + t2 * dy);
                    return true;
                }
                closestToStartIntersectionPoint = Vector2.zero;
                return false;
            }
        }
    }
}