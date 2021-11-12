//==============================================================================
// Filename: PathGrid.cs
// Author: Aaron Thompson
// Date Created: 10/8/2021
// Last Updated: 10/8/2021
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//------------------------------------------------------------------------------
public class PathGrid : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
    //SETTING(s)
    public LayerMask unwalkableMask;
    public Vector3 gridWorldSize;
    public float nodeRadius=0.5f;
    public float nodeHeight=0.2f;

    //GRID
    private PathNode[,] grid;
    private int gridM, gridN, gridH; //M = rows, N = columns, H = height steps
    private float nodeDiameter;
    [HideInInspector]
    public int length { get { return gridM * gridN; } }

    //DEBUG
    public bool debugEnabled = false;
    public bool debugDrawGrid = false;
    public float debugGridGizmoHeight=0.1f;
    public float debugGridGizmoPadding=0.1f;

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
    void Awake() {
        CalculateParameters();
        CreateGrid();
    }

//INITIALIZATION FUNCTIONS
//------------------------------------------------------------------------------
    private void CalculateParameters() {
        nodeDiameter = nodeRadius * 2;
        gridM = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridN = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);
        gridH = Mathf.RoundToInt(gridWorldSize.y / nodeHeight);
    }

    private void CreateGrid() {
        grid = new PathNode[gridM, gridN];
        //X Start = Left (MAX)
        //Y Start = Top (MIN)
        //Z Start = Back (MIN)
        Vector3 worldCorner = transform.position - (Vector3.right * gridWorldSize.x / 2) + (Vector3.up * gridWorldSize.y / 2) - (Vector3.forward * gridWorldSize.z / 2);
        for(int x = 0; x < gridM; x++) {
            for(int y = 0;  y < gridN; y++) {
                Vector3 xLocal = Vector3.right * ((x * nodeDiameter) + nodeRadius);
                Vector3 yLocal = Vector3.zero;
                Vector3 zLocal = Vector3.forward * ((y * nodeDiameter) + nodeRadius);
                Vector3 worldPosition = worldCorner + xLocal + yLocal + zLocal;
                //bool isWalkable = !(Physics.CheckSphere(worldPosition, nodeRadius, unwalkableMask));
                bool isWalkable = true;
                for (int i = 0; i < gridH; i++) {
                    if(Physics.CheckSphere(worldPosition, nodeRadius, unwalkableMask)) {
                        isWalkable = false;
                        break;
                    }

                    if (Physics.CheckSphere(worldPosition, nodeHeight)) {
                        break;
                    }

                    worldPosition -= Vector3.up * nodeHeight;
                }
                grid[x, y] = new PathNode(worldPosition, isWalkable, x, y);
            }
        }
    }

//UTILITY FUNCTIONS
//------------------------------------------------------------------------------
    public PathNode GetNode(int x, int y){
        return grid[x,y];
    }

    public PathNode GetNode(Vector3 position) {
        float percentX = Mathf.Clamp01((position.x + gridWorldSize.x / 2) / gridWorldSize.x);
        float percentY = Mathf.Clamp01((position.z + gridWorldSize.z / 2) / gridWorldSize.z);
        int x = Mathf.RoundToInt((gridM - 1) * percentX);
        int y = Mathf.RoundToInt((gridN - 1) * percentY);

        return grid[x,y];
    }

    public List<PathNode> GetNeighbors(PathNode node) {
        List<PathNode> neighbors = new List<PathNode>();

        for(int i = -1; i < 2; i++) {
            int x = node.gridX + i;
            if(x < 0 || x >= gridM) {
                continue;
            }

            for(int j = -1; j < 2; j++) {
                if(i==0 && j == 0) {
                    continue;
                }

                int y = node.gridY + j;
                if(y < 0 || y >= gridN) {
                    continue;
                }

                neighbors.Add(grid[x, y]);
            }
        }

        return neighbors;
    }

// DEBUG FUNCTIONS
//------------------------------------------------------------------------------
    private void OnDrawGizmos() {
        if (debugEnabled) {
            if (debugDrawGrid) {
                Gizmos.DrawWireCube(transform.position, gridWorldSize);
                if (grid != null) {
                    foreach (PathNode node in grid) {
                        Gizmos.color = (node.isWalkable) ? Color.white : Color.red;
                        Gizmos.DrawWireCube(node.position, new Vector3(nodeDiameter-debugGridGizmoPadding, debugGridGizmoHeight, nodeDiameter - debugGridGizmoPadding));
                    }
                }
            }
        }
    }
}
//==============================================================================
//==============================================================================