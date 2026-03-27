//==============================================================================
// Filename: DrawHelper.cs
// Author: Aaron Thompson
// Date Created: 3/3/2026
// Last Updated: 3/4/2026
//
// Description: Helper class for UI mesh drawing.
//==============================================================================
using UnityEngine;
using UnityEngine.UI;

public class DrawHelper {
    public static void DrawSector(Vector2 center, float radius, float startAngle, float endAngle, int segments, Color color, VertexHelper vh) {
        float section = (endAngle - startAngle) / segments;

        int startIndex = vh.currentVertCount;
        UIVertex v = UIVertex.simpleVert;
        v.color = color;
        v.position = center;
        vh.AddVert(v);
        for(int i = 0; i <= segments; i++) {
            float t = ((float)i) / segments;
            float angle = Mathf.Lerp(endAngle, startAngle, t);

            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Vector2 position = center + offset;

            v.position = position;
            vh.AddVert(v);
        }

        for (int i = 1; i <= segments; i++) {
            vh.AddTriangle(startIndex, startIndex + i, startIndex + i + 1);
        }
    }

    public static void DrawLine(Vector2 a, Vector2 b, VertexHelper vh, Color color, float lineThickness = 1.0f) {
        Vector2 dir = (b - a).normalized;
        Vector2 norm = new Vector2(-dir.y, dir.x) * lineThickness * 0.5f;

        int index = vh.currentVertCount;
        UIVertex v = UIVertex.simpleVert;
        v.color = color;
        v.position = a - norm;
        vh.AddVert(v);
        v.position = a + norm;
        vh.AddVert(v);
        v.position = b + norm;
        vh.AddVert(v);
        v.position = b - norm;
        vh.AddVert(v);

        vh.AddTriangle(index + 0, index + 1, index + 2);
        vh.AddTriangle(index + 2, index + 3, index + 0);

    }

    public static void DrawSquare(Vector2 center, float length, Color color, VertexHelper vh) {
        Vector2[] directions = {new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(-1, -1)};

        int startIndex = vh.currentVertCount;
        UIVertex v = UIVertex.simpleVert;
        v.color = color;
        for(int i = 0; i < directions.Length; i++) {
            v.position = center + (directions[i] * (length/2));
            vh.AddVert(v);
        }

        vh.AddTriangle(startIndex + 0, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex + 1, startIndex + 2, startIndex + 3);
    }

    public static void DrawCircle(Vector2 center, float radius, int segments, float startAngle, float endAngle, VertexHelper vh, Color color, float lineThickness = 1.0f) {
        float section = (endAngle - startAngle) / segments;

        for(int i = 0; i < segments; i++) {
            float ax = Mathf.Cos((i * section) + startAngle);
            float ay = Mathf.Sin((i * section) + startAngle);
            float bx = Mathf.Cos(((i+1) * section) + startAngle);
            float by = Mathf.Sin(((i+1) * section) + startAngle);
            Vector2 a = center + new Vector2(ax, ay) * radius;
            Vector2 b = center + new Vector2(bx, by) * radius;

            DrawLine(a, b, vh, color, lineThickness);
        }
    }
}
