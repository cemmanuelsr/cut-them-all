using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private CapsuleCollider2D capsuleCollider;
    private BoxCollider2D groundCollider;
    private Rigidbody2D rigidBody;
    private LineRenderer lineRenderer;
    private Vector2 cutStartPoint;
    private Vector2 cutEndPoint;
    private float cutTraceWidth;
    private bool isCreatingCut;
    private bool isCutting;
    private List<string> animationStates;

    public float maxCutLength;
    public float cutSpeed;
    
    public void Start() {
        // Player collision box and rigid body
        capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
        groundCollider = gameObject.GetComponent<BoxCollider2D>();
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

        // Possible states for animation
        animationStates = new List<string>();
        animationStates.Add("IsPreparing");
        animationStates.Add("IsJumping");
        animationStates.Add("IsFalling");
        animationStates.Add("IsAttacking");

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
            if (capsuleCollider.bounds.Contains(worldMousePos)) {
                // Activate creating cut flag and store the cut starting point
                isCreatingCut = true;
                cutStartPoint = transform.TransformPoint(capsuleCollider.offset);
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

        if (isCutting) {
            if (rigidBody.velocity.magnitude < cutSpeed / 2.0) {
                isCutting = false;
                Physics2D.IgnoreLayerCollision(7, 8, false);
            }
        }

        // Flip sprite according to mouse position
        Vector3 _worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (_worldMousePos.x < transform.position.x && transform.localScale.x > 0)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        else if (_worldMousePos.x > transform.position.x && transform.localScale.x < 0)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        if (rigidBody.velocity.y <= 0.0f) {
            if (isGrounded()) {
                if (Input.GetMouseButton(0)) {
                    // Change animation to prepare
                    setOneTrue("IsPreparing");
                } else {
                    setOneTrue("");
                }
            }
            else
                setOneTrue("IsFalling");
        } else
                setOneTrue("IsJumping");
    }

    void OnCollisionEnter2D(Collision2D collision) {
        isCutting = false;
        Physics2D.IgnoreLayerCollision(7, 8, false);
    }

    void OnTriggerExit2D(Collider2D otherCollider) {
        if (otherCollider.gameObject.CompareTag("Cuttable") && isCutting) {
            Cuttable cuttableScript = (Cuttable)otherCollider.gameObject.GetComponent<Cuttable>();
            cuttableScript.cut(cutStartPoint, cutEndPoint);

            // Change animation to falling
            setOneTrue("IsAttacking");
        }
    }

    private bool isGrounded() {
        return groundCollider.IsTouchingLayers(-1);
    }

    private Vector3 limitVector(Vector3 origin, Vector3 dir, Vector3 v, float limit) {
        // Limit vector v to limit t based on vector origin and direction
        if (Vector3.Distance(origin, v) > limit)
            return origin + dir * limit;
        return v;
    }

    private void setOneTrue(string toSetTrue) {
        Animator animator = gameObject.GetComponent<Animator>();
        foreach (string animationState in animationStates)
            animator.SetBool(animationState, animationState == toSetTrue);
    }

    private void makeCut() {
        // Limit end point to max length
        Vector2 lineDir = Vector3.Normalize(cutEndPoint - cutStartPoint);
        cutEndPoint = limitVector(cutStartPoint, lineDir, cutEndPoint, maxCutLength);
        
        // Apply force in the direction of the cut
        Vector2 cutForce = cutSpeed * Vector3.Distance(cutStartPoint, cutEndPoint) * lineDir;
        rigidBody.AddForce(cutForce, ForceMode2D.Impulse);
    }
}
