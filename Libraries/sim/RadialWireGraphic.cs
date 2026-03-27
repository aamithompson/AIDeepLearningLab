//==============================================================================
// Filename: RadialWireGraphic.cs
// Author: Aaron Thompson
// Date Created: 12/1/2025
// Last Updated: 3/4/2026
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialWireGraphic : Graphic {
// VARIABLES
//------------------------------------------------------------------------------
    public int columns = 1; //Radial Lines
    public int rows = 1; //Rings
    public int segments = 64; //Number of Lines to Draw a Circle
    [Range(0.0f, 180.0f)]
    public float leftAngle = 45.0f;
    [Range(0.0f, 180.0f)]
    public float rightAngle = 45.0f;
    [Range(0.0f, 1.0f)]
    public float startRadius = 0.0f;

    public float lineThickness = 1f;
    public Color lineColor = Color.white;
    public bool hasFilling = false;
    public Gradient fillingGradient;
    public List<float> sectort;

// DRAW FUNCTION(s)
//------------------------------------------------------------------------------
    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();

        Rect rectangle = rectTransform.rect;
        Vector2 center = rectangle.center;
        float maxRadius = Mathf.Min(rectangle.width, rectangle.height) * 0.5f;
        float minRadius = maxRadius * startRadius;
        float startAngle = (leftAngle + 90.0f) * Mathf.Deg2Rad;
        float endAngle = -(rightAngle - 90.0f) * Mathf.Deg2Rad;

        //Sector Fillings
        if (hasFilling) {
            float deltaAngle = -(startAngle - endAngle)/columns;
            float a = startAngle;
            float b = a + deltaAngle;
            for(int i = 0; i < columns; i++) {
               float t = (float)i / columns;
                if (sectort != null && sectort.Count == columns) {
                    t = sectort[i];
                }
                t = Mathf.Clamp01(t);

                Color color = fillingGradient.Evaluate(t);
                DrawHelper.DrawSector(center, maxRadius, a, b, segments / columns, color, vh);
                a = b;
                b += deltaAngle;
            }
        }

        //Rings
        float step = 1.0f / rows;
        for(int i = 0; i <= rows; i++) {
            if(i == 0 && startRadius == 0.0f) {
                continue;
            }

            float radius = ((maxRadius - minRadius) * i * step) + minRadius;

            DrawHelper.DrawCircle(center, radius, segments, startAngle, endAngle, vh, color, lineThickness);
        }

        //Radial Lines
        step = ((endAngle - startAngle) / columns);
        for(int i = 0; i <= columns; i++) {
            float angle = (i * step) + startAngle;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 startpoint = center + (dir * minRadius);
            Vector2 endpoint = center + (dir * maxRadius);

            DrawHelper.DrawLine(startpoint, endpoint, vh, lineColor, lineThickness);
        }
    }

// HELPER FUNCTION(s)
//------------------------------------------------------------------------------

}
//==============================================================================
//==============================================================================
