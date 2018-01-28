using UnityEngine;

namespace InitialPrefabs.DANIEditor {
    /// <summary>
    /// A utility extension class for Rects
    /// </summary>
    internal static class RectUtility {
        internal static Rect ScaleAroundPivot (this Rect rect, float scale, Vector2 pivot) {
            var result = rect;
            result.x -= pivot.x;
            result.y -= pivot.y;

            result.xMin *= scale;
            result.xMax *= scale;
            result.yMin *= scale;
            result.xMax *= scale;

            result.x += pivot.x;
            result.y += pivot.y;

            return result;
        }

        internal static Vector2 UpperLeft (this Rect rect) {
            return new Vector2 (rect.xMin, rect.yMin);
        }
    }
}