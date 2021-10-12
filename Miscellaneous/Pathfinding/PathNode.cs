//==============================================================================
// Filename: PathNode.cs
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
public class PathNode : IHeapElement<PathNode> {
// VARIABLES
//------------------------------------------------------------------------------
    //Data
    public PathNode parent;
    public bool hasParent { get { return parent != null; } }
    public Vector3 position;
    public bool isWalkable;
    public int gridX;
    public int gridY;
    private int index;

    //Costs
    public int gCost; //Current Travel Cost
    public int hCost; //Heuristic (Distance from Target) Cost
    public int fCost { get { return gCost + hCost; } } //Total Cost

// CONSTRUCTORS
//------------------------------------------------------------------------------
    public PathNode(PathNode node) {
        this.position = node.position;
        this.isWalkable = node.isWalkable;
        this.gridX = node.gridX;
        this.gridY = node.gridY;
    }

    public PathNode(Vector3 position, bool isWalkable, int gridX, int gridY) {
        this.position = position;
        this.isWalkable = isWalkable;
        this.gridX = gridX;
        this.gridY = gridY;
    }

// INHERITED FUNCTIONS
//------------------------------------------------------------------------------
    public int HeapIndex{
        get { return index; }
        set { index = value;  }
    }

    public int CompareTo(PathNode node) {
        int compare = fCost.CompareTo(node.fCost);
        if(compare == 0) {
            compare = hCost.CompareTo(node.hCost);
        }

        return -compare;
    }
}
//==============================================================================
//==============================================================================