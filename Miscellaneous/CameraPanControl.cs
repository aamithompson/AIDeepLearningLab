//==============================================================================
// Filename: FFEvoNeuralNetwork.cs
// Author: Aaron Thompson
// Date Created: 8/26/2021
// Last Updated: 9/2/2021
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraPanControl : MonoBehaviour {
//CONFIGURATION VARIABLE(S)
    public float minZoom;
    public float maxZoom;
    [Range(0.0f, 1.0f)]
    public float zoomLerpT;
    public float mouseScrollSensitivity;
    public Vector2 minPan;
    public Vector2 maxPan;
    public Vector2 panAcceleration;
    public Vector2 minPanVelocity;
    public Vector2 maxPanVelocity;
    [Range(0.0f, 1.0f)]
    public float panLerpT;

//VARIABLE(S)
    private Camera orthoCamera;
    private Vector3 origin;
    private float zoom;
    public Vector2 panInput;
    public Vector2 panVelocity;
    public Vector2 pan;
    public Vector2 minMouseBound;
    public Vector2 maxMouseBound;

    void Start() {
        orthoCamera = GetComponent<Camera>();
        origin = gameObject.transform.position;
        zoom = orthoCamera.orthographicSize;
    }

    void Update() {
        AdjustZoom();
        AdjustPosition();
    }

    private void AdjustZoom() {
        zoom += (Input.mouseScrollDelta.y * mouseScrollSensitivity * -1);
        zoom = (zoom < minZoom) ? minZoom : zoom;
        zoom = (zoom > maxZoom) ? maxZoom : zoom;

        orthoCamera.orthographicSize = Mathf.Lerp(orthoCamera.orthographicSize, zoom, zoomLerpT);
    }
    private void AdjustPosition() {
        panInput = Vector2.zero;

        //Camera Edge Detection
        float mouseX = (Screen.width - Input.mousePosition.x) / Screen.width;
        float mouseY = (Screen.height - Input.mousePosition.y) / Screen.height;
        Vector2 mouse = Vector2.one - new Vector2(mouseX, mouseY) * 2;

        //Binary Input
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || mouse.x < -maxMouseBound.x) {
            panInput.x = -1;
            panVelocity.x = (panVelocity.x > -minPanVelocity.x) ? -minPanVelocity.x : panVelocity.x;
        } else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || mouse.x > maxMouseBound.x) {
            panInput.x = 1;
            panVelocity.x = (panVelocity.x < minPanVelocity.x) ? minPanVelocity.x : panVelocity.x;
        }

        if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || mouse.y < -maxMouseBound.y) {
            panInput.y = -1;
            panVelocity.y = (panVelocity.y > -minPanVelocity.y) ? -minPanVelocity.y : panVelocity.y;
        } else if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || mouse.y > maxMouseBound.y) {
            panInput.y = 1;
            panVelocity.y = (panVelocity.y < minPanVelocity.y) ? minPanVelocity.y : panVelocity.y;
        }

        //Pan Velocity
        panVelocity += panInput * panAcceleration * Time.deltaTime;
        panVelocity.x = (panVelocity.x < -maxPanVelocity.x) ? -maxPanVelocity.x : panVelocity.x;
        panVelocity.x = (panVelocity.x > maxPanVelocity.x) ? maxPanVelocity.x : panVelocity.x;
        panVelocity.y = (panVelocity.y < -maxPanVelocity.y) ? -maxPanVelocity.y : panVelocity.y;
        panVelocity.y = (panVelocity.y > maxPanVelocity.y) ? maxPanVelocity.y : panVelocity.y;

        //Linear Input
        if(panInput.x == 0) {
            if(mouse.x < -minMouseBound.x) {
                panInput.x = (mouse.x + minMouseBound.x)/(maxMouseBound.x - minMouseBound.x);
                panVelocity.x = panInput.x * (maxPanVelocity.x - minPanVelocity.x) - minPanVelocity.x;
            } else if (mouse.x > minMouseBound.x) {
                panInput.x = (mouse.x - minMouseBound.x)/(maxMouseBound.x - minMouseBound.x);
                panVelocity.x = panInput.x * (maxPanVelocity.x - minPanVelocity.x) + minPanVelocity.x;
            }
        }

        if(panInput.y == 0) {
            if(mouse.y < -minMouseBound.y) {
                panInput.y = (mouse.y + minMouseBound.y)/(maxMouseBound.y - minMouseBound.y);
                panVelocity.y = panInput.y * (maxPanVelocity.y - minPanVelocity.y) - minPanVelocity.y;
            } else if (mouse.y > minMouseBound.y) {
                panInput.y = (mouse.y - minMouseBound.y) / (maxMouseBound.y - minMouseBound.y);
                panVelocity.y = panInput.y * (maxPanVelocity.y - minPanVelocity.y) + minPanVelocity.y;
            }
        }

        panVelocity.x = (panInput.x == 0) ? 0 : panVelocity.x;
        panVelocity.y = (panInput.y == 0) ? 0 : panVelocity.y;

        //Pan Positioning
        pan.x += panVelocity.x * Time.deltaTime;
        pan.x = (pan.x > maxPan.x) ? maxPan.x : pan.x;
        pan.x = (pan.x < minPan.x) ? minPan.x : pan.x;
        pan.y += panVelocity.y * Time.deltaTime;
        pan.y = (pan.y > maxPan.y) ? maxPan.y : pan.y;
        pan.y = (pan.y < minPan.y) ? minPan.y : pan.y;
        Vector3 x = transform.right * pan.x;
        Vector3 y = transform.up * pan.y;
        Vector3 v = origin + x + y;
        transform.position = Vector3.Lerp(transform.position, v, panLerpT);
    }
}
//==============================================================================
//==============================================================================
