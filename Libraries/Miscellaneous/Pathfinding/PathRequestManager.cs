//==============================================================================
// Filename: PathRequest.cs
// Author: Aaron Thompson
// Date Created: 10/9/2021
// Last Updated: 10/9/2021
//
// Description:
//==============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//------------------------------------------------------------------------------
[RequireComponent(typeof(Pathfinding))]
public class PathRequestManager : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
    private Queue<PathRequest> requestQueue = new Queue<PathRequest>();
    private PathRequest currentRequest;
    private bool isProcessing;

    private Pathfinding pathfinding;

    private static PathRequestManager instance;

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
    void Awake() {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

// REQUEST FUNCTIONS
//------------------------------------------------------------------------------
    public static void RequestPath(Vector3 start, Vector3 end, float[] maxHeightDelta, Action<Vector3[], bool> callback) {
        PathRequest request = new PathRequest(start, end, maxHeightDelta, callback);
        instance.requestQueue.Enqueue(request);
        instance.TryProcessNext();
    }

    public static void RequestPath(Vector3 start, Vector3 end, float maxHeightDelta, Action<Vector3[], bool> callback) {
        float[] maxHeightDeltaArray = { maxHeightDelta };
        RequestPath(start, end, maxHeightDeltaArray, callback);
    }

    private void TryProcessNext() {
        if(!isProcessing && requestQueue.Count > 0) {
            currentRequest = requestQueue.Dequeue();
            isProcessing = true;
            pathfinding.StartFindPath(currentRequest.start, currentRequest.end, currentRequest.maxHeightDelta);
        }
    }

    public void FinishProcessingPath(Vector3[] path, bool success) {
        currentRequest.callback(path, success);
        isProcessing = false;
        TryProcessNext();
    }

    struct PathRequest {
        public Vector3 start;
        public Vector3 end;
        public float[] maxHeightDelta;
        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 start, Vector3 end, float[] maxHeightDelta, Action<Vector3[], bool> callback) {
            this.start = start;
            this.end = end;
            this.maxHeightDelta = maxHeightDelta;
            this.callback = callback;
        }
    }
}
//==============================================================================
//==============================================================================
