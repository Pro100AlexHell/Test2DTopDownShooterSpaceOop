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
    }
}