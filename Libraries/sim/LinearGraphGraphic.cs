//==============================================================================
// Filename: LinearGraphGraphic.cs
// Author: Aaron Thompson
// Date Created: 3/3/2026
// Last Updated: 3/4/2026
//
// Description:
//==============================================================================
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinearGraphGraphic : Graphic {
// VARIABLES
//------------------------------------------------------------------------------
    private List<int> vertexCount; //size n
    private List<List<List<float>>> lineWeight; //List of adjacency matrices, size n - 1

    public float vertexSize = 4.0f;
    public float vertexPadding = 1.0f;
    public Color vertexColor = Color.white;
    public float layerDistance = 16.0f;
    public float lineThickness = 1.0f;
    public Gradient lineColors;

    public void Start() {
        lineWeight = new List<List<List<float>>>();
        vertexCount = new List<int>();
    }

// DRAW FUNCTION(s)
//------------------------------------------------------------------------------
    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();

        Rect rectangle = rectTransform.rect;
        Vector2 center = rectangle.center;

        float xDist = vertexSize + vertexPadding + layerDistance;
        float yDist = vertexSize + vertexPadding;
        float startX = center.x - ((vertexCount.Count - 1) * xDist)/2;

        //Drawing lines
        float xa = startX;
        float xb = startX + xDist;
        for(int i = 0; i < vertexCount.Count - 1; i++) {
            float startYA = center.y + ((vertexCount[i] - 1) * yDist) / 2;
            float startYB = center.y + ((vertexCount[i+1] - 1) * yDist) / 2;
            Vector2 positionA = new Vector2(xa, startYA);
            Vector2 positionB = new Vector2(xb, startYB);

            for (int j = 0; j < vertexCount[i]; j++) {
                for(int k = 0; k < lineWeight[i][j].Count; k++) {
                    float t = 0.5f;
                    Color color = lineColors.Evaluate(t);
                    DrawHelper.DrawLine(positionA, positionB, vh, color, lineThickness);

                    positionB += Vector2.down * yDist;
                }

                positionA += Vector2.down * yDist;
                positionB = new Vector2(xb, startYB);
            }

            xa = xb;
            xb += xDist;
        }

        //Drawing vertices
        float x = startX;
        for (int i = 0; i < vertexCount.Count; i++) {
            float startY = center.y + ((vertexCount[i] - 1) * yDist) / 2;
            Vector2 position = new Vector2(x, startY);

            for(int j = 0; j < vertexCount[i]; j++) {
                DrawHelper.DrawSquare(position, vertexSize, vertexColor, vh);
                position += Vector2.down * yDist;
            }

            x += xDist;
        }
    }

// HELPER FUNCTION(s)
//------------------------------------------------------------------------------

    public void SetLayerDepth(int depth) {
        int n = vertexCount.Count;
        if(depth < n) {
            for(int i = n - 1; i >= depth; i--) {
                vertexCount.RemoveAt(i);
            }

            while(lineWeight.Count > Mathf.Max(0, vertexCount.Count - 1)) {
                lineWeight.RemoveAt(lineWeight.Count - 1);
            }
        }

        if(depth > n) {
            for(int i = n; i < depth; i++) {
                vertexCount.Add(0);
            }

            while(lineWeight.Count < vertexCount.Count - 1) {
                lineWeight.Add(new List<List<float>>());
            }
        }

        SetVerticesDirty();
    }

    public void SetVertexCount(int layer, int count) {
        //-1 on l and/or denotes layer does not exist. l -> left, r -> right
        int l = (layer > 0) ? vertexCount[layer - 1] : -1;
        int r = (layer + 1 < vertexCount.Count) ? vertexCount[layer + 1] : -1;
        int pCount = vertexCount[layer]; //Previous count
        int diff = count - pCount;
        vertexCount[layer] = count;

        if (l != -1) {
            if(diff < 0) {
                for (int i = 0; i < l; i++) {
                    for (int j = 0; j < -diff; j++) {
                        lineWeight[layer - 1][i].RemoveAt(lineWeight[layer - 1][i].Count - 1);
                    }
                }
            }

            if(diff > 0){
                for(int i = 0; i < l; i++) {
                    for (int j = 0; j < diff; j++) {
                        lineWeight[layer - 1][i].Add(0.0f);
                    }
                }
            }
        }

        if(r != -1) {
            if(diff < 0) {
                for(int i = 0; i < -diff; i++) {
                    lineWeight[layer].RemoveAt(lineWeight[layer].Count - 1);
                }
            }

            if(diff > 0) {
                for (int i = 0; i < diff; i++) {
                    lineWeight[layer].Add(new List<float>());
                    for(int j = 0; j < r; j++) {
                        lineWeight[layer][lineWeight[layer].Count - 1].Add(0.0f);
                    }
                }
            }
        }

        SetVerticesDirty();
    }

    public void SetLineWeight(int layer, int from, int to, float weight) {
        lineWeight[layer][from][to] = weight;
        SetVerticesDirty();
    }
}
//==============================================================================
//==============================================================================