using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private BoxCollider2D boxCollider;
    private Rigidbody2D rigidBody;
    private LineRenderer lineRenderer;
    private Vector2 cutStartPoint;
    private Vector2 cutEndPoint;
    private float cutTraceWidth;
    private bool isCreatingCut;
    public bool isCutting;

    public float maxCutLength;
    public float cutSpeed;
    
    public void Start() {
        // Player collision box and rigid body
        boxCollider = gameObject.GetComponent<BoxCollider2D>();
        rigidBody = gameObject.GetComponent<Rigidbody2D>();

        // Line renderer line width
        cutTraceWidth = 0.01f;

        // Line renderer
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = cutTraceWidth;
        lineRenderer.endWidth = cutTraceWidth;
        lineRenderer.startColor = new Color(0.0f, 200.0f, 0.0f);
        lineRenderer.endColor = new Color(0.0f, 200.0f, 0.0f);
        lineRenderer.positionCount = 2;

        // Flag that controls the creation of cuts
        isCreatingCut = false;

        // Flag that controls when the player is cutting
        isCutting = false;

        // Cut parameters
        maxCutLength = 8.0f;
        cutSpeed = 4.0f;
    }
    
    public void Update() {
        // Get mouse click on player's collision box
        if (Input.GetMouseButtonDown(0)) {
            // Mouse world position
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldMousePos.z = 0.0f;
            if (boxCollider.bounds.Contains(worldMousePos)) {
                // Activate creating cut flag and store the cut starting point
                isCreatingCut = true;
                cutStartPoint = transform.TransformPoint(boxCollider.offset);
            }
        }

        // Get mouse up click
        if (Input.GetMouseButtonUp(0)) {
            if (isCreatingCut) {
                // Disable creating cut flag
                isCreatingCut = false;

                // Clear cut trace line
                lineRenderer.positionCount = 0;
                lineRenderer.positionCount = 2;

                // Mouse world position and store cut end point
                Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldMousePos.z = 0.0f;
                cutEndPoint = worldMousePos;

                // Enable the cutting flag and collision mask
                isCutting = true;
                Physics2D.IgnoreLayerCollision(7, 8, true);

                // Apply cutting force based on cutting parameters
                makeCut();
            }
        }
        
        if (isCreatingCut) {
            // Mouse world position and vector of points
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldMousePos.z = 0.0f;

            // Limit drawing point to max cut length
            Vector2 lineDir = Vector3.Normalize(new Vector2(worldMousePos.x, worldMousePos.y) - cutStartPoint);
            Vector3 lineEndPoint = limitVector(cutStartPoint, lineDir, worldMousePos, maxCutLength);

            // Draw cut trace line
            Vector3[] points = new Vector3[2] {cutStartPoint, lineEndPoint};
            lineRenderer.SetPositions(points);
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        isCutting = false;
        Physics2D.IgnoreLayerCollision(7, 8, false);
    }

    private Vector3 limitVector(Vector3 origin, Vector3 dir, Vector3 v, float limit) {
        // Limit vector v to limit t based on vector origin and direction
        if (Vector3.Distance(origin, v) > limit)
            return origin + dir * limit;
        return v;
    }

    private void makeCut() {
        // Limit end point to max length
        Vector2 lineDir = Vector3.Normalize(cutEndPoint - cutStartPoint);
        cutEndPoint = limitVector(cutStartPoint, lineDir, cutEndPoint, maxCutLength);
        
        // Apply force in the direction of the cut
        Vector2 cutForce = lineDir * Vector3.Distance(cutStartPoint, cutEndPoint) * cutSpeed;
        rigidBody.AddForce(cutForce, ForceMode2D.Impulse);
    }
}
