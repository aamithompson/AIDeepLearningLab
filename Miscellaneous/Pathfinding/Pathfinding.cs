//==============================================================================
// Filename: Pathfinding.cs
// Author: Aaron Thompson
// Date Created: 10/8/2021
// Last Updated: 10/9/2021
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
//------------------------------------------------------------------------------
[RequireComponent (typeof(PathGrid))]
public class Pathfinding : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
    //SETTING(s)
    public float maxHeightPenaltyScale;

    //COMPONENTS
    private PathGrid grid;
    private PathRequestManager manager;

    // MONOBEHAVIOR METHODS
    //------------------------------------------------------------------------------
    void Awake() {
        grid = GetComponent<PathGrid>();
        manager = GetComponent<PathRequestManager>();
    }

// PATH FUNCTIONS
//------------------------------------------------------------------------------
    public void StartFindPath(Vector3 start, Vector3 target, float[] maxHeightDelta) {
        StartCoroutine(FindPath(start, target, maxHeightDelta));
    }
    public void StartFindPath(Vector3 start, Vector3 target, float maxHeightDelta=-1) {
        float[] maxHeightDeltaArray = { maxHeightDelta };
        StartFindPath(start, target, maxHeightDeltaArray);
    }

    private IEnumerator FindPath(Vector3 start, Vector3 target, float[] maxHeightDelta) {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] path = new Vector3[0];
        bool success = false;
        PathNode startNode = grid.GetNode(start);
        PathNode targetNode = grid.GetNode(target);

        if(startNode.isWalkable && targetNode.isWalkable) {
            Heap<PathNode> open = new Heap<PathNode>(grid.length);
            HashSet<PathNode> closed = new HashSet<PathNode>();

            open.Push(startNode);
            while(open.Count > 0) {
                PathNode currentNode = open.Pop();
                /*for(int i = 1; i < open.Count; i++) {
                    if(open[i].fCost < currentNode.fCost || (open[i].fCost == currentNode.fCost && open[i].hCost < currentNode.hCost)) {
                        currentNode = open[i];
                    }
                }

                open.Remove(currentNode);*/
                closed.Add(currentNode);

                if(currentNode == targetNode) {
                    sw.Stop();
                    print("[Pathfinding]: " + sw.ElapsedMilliseconds + "ms");
                    success = true;
                    break;
                }

                foreach(PathNode neighbor in grid.GetNeighbors(currentNode)) {
                    if(closed.Contains(neighbor) || !IsStepWalkable(currentNode, neighbor, maxHeightDelta)) {
                        continue;
                    }

                    int costToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if(maxHeightDelta.Length > 0) {
                        if(maxHeightDelta[0] != -1) {
                            costToNeighbor *= (int)Mathf.Round(1 + (Mathf.Abs(currentNode.position.y - neighbor.position.y) / maxHeightDelta[0]) * maxHeightPenaltyScale);
                        }
                    }

                    if(costToNeighbor < neighbor.gCost || !open.Contains(neighbor)) {
                        neighbor.gCost = costToNeighbor;
                        if (!open.Contains(neighbor)) {
                            neighbor.hCost = GetDistance(neighbor, targetNode);
                            open.Push(neighbor);
                        } else {
                            open.UpdateItem(neighbor);
                        }

                        neighbor.parent = currentNode;
                    }
                }
            }
        }

        if(success) {
            path = RetracePath(startNode, targetNode);
        }

        manager.FinishProcessingPath(path, success);
        yield return null;
    }

    private Vector3[] RetracePath(PathNode startNode, PathNode targetNode) {
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = targetNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        System.Array.Reverse(waypoints);

        return waypoints;
    }

    private Vector3[] SimplifyPath(List<PathNode> path) {
        List<Vector3> waypoints = new List<Vector3>();

        Vector2 directionOld = Vector2.zero;
        for(int i = 1; i < path.Count; i++) {
            float x = path[i-1].gridX - path[i].gridX;
            float y = path[i-1].gridY - path[i].gridY;
            Vector2 directionNew = new Vector2(x, y);

            if(directionNew != directionOld) {
                waypoints.Add(path[i].position);
                directionOld = directionNew;
            }
        }

        return waypoints.ToArray();
    }

    private int GetDistance(PathNode nodeA, PathNode nodeB) {
        int xDist = Mathf.Abs(nodeB.gridX - nodeA.gridX);
        int yDist = Mathf.Abs(nodeB.gridY - nodeA.gridY);
        int diag = Mathf.Min(xDist, yDist);
        int adj = Mathf.Max(xDist, yDist) - diag;

        return (14 * diag) + (10 * adj);
    }

// UTILITY FUNCTIONS
//------------------------------------------------------------------------------
    private bool IsStepWalkable(PathNode startNode, PathNode targetNode, float[] maxHeightDelta) {
        if(!startNode.isWalkable || !targetNode.isWalkable) {
            return false;
        }

        PathNode testNode = startNode;
        for(int i = 0; i < maxHeightDelta.Length; i++) {
            if(maxHeightDelta[i] < 0) {
                break;
            }

            if(Mathf.Abs(targetNode.position.y - testNode.position.y) > maxHeightDelta[i]) {
                return false;
            }

            if (!testNode.hasParent) {
                break;
            }

            testNode = testNode.parent;
        }

        return true;
    }
}
//==============================================================================
//==============================================================================
