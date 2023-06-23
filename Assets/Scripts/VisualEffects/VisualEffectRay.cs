using UnityEngine;
using UnityEngine.UI;

namespace VisualEffects
{
    public class VisualEffectRay : VisualEffect
    {
        private readonly GameObject _rayGameObject;

        private const float DurationSec = 0.3f;
        private readonly Color _lineColor = Color.red;
        private readonly float _lineWidth = 5;

        public VisualEffectRay(Transform parentForObjectViews, Vector2 startPos, Vector2 endPos)
            : base(DurationSec)
        {
            _rayGameObject = CreateLineForOverlayCanvas(parentForObjectViews, startPos, endPos);
        }

        /// <summary>
        /// Draws a 'line'
        /// todo NOTE: this works for Screen Space-Overlay Canvas
        /// todo NOTE: based on https://gamedev.stackexchange.com/questions/176366/how-to-render-line-renderer-or-trail-renderer-above-ui-canvas/189371#189371
        /// todo NOTE: and on https://stackoverflow.com/questions/69280558/how-to-draw-a-line-on-top-of-a-panel-use-screen-space-overlay-canvas/75824468#75824468
        /// </summary>
        private GameObject CreateLineForOverlayCanvas(Transform parentForObjectViews, Vector2 startPos, Vector2 endPos)
        {
            GameObject obj = new GameObject("CreateLineForOverlayCanvas");
            obj.transform.SetParent(parentForObjectViews, false);
            obj.AddComponent<Image>().color = _lineColor;

            float angleDeg = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * Mathf.Rad2Deg;
            obj.transform.Rotate(0, 0, angleDeg);

            RectTransform rectTransform = obj.GetComponent<RectTransform>();

            float dist = Vector2.Distance(endPos, startPos);
            rectTransform.sizeDelta = new Vector2(dist, _lineWidth);

            float midPointX = endPos.x + (startPos.x - endPos.x) / 2;
            float midPointY = endPos.y + (startPos.y - endPos.y) / 2;
            rectTransform.position = new Vector3(midPointX, midPointY, 0);

            return obj;
        }

        // todo !! this does not work for Screen Space-Overlay Canvas
        /*private GameObject CreateLineRendererGameObject(Vector2 startPos, Vector2 endPos)
        {
            GameObject obj = new GameObject("LineRendererGameObject");
            LineRenderer lineRenderer = obj.AddComponent<LineRenderer>();

            lineRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));

            lineRenderer.startColor = _lineColor;
            lineRenderer.endColor = _lineColor;

            lineRenderer.startWidth = _lineWidth;
            lineRenderer.endWidth = _lineWidth;

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);

            return obj;
        }*/

        public override void OnDestroy()
        {
            Object.Destroy(_rayGameObject);
        }
    }
}