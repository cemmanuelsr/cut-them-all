using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private CapsuleCollider2D capsuleCollider;
    private BoxCollider2D groundCollider;
    private Rigidbody2D rigidBody;
    private LineRenderer lineRenderer;
    private Animator animator;
    private Vector2 cutStartPoint;
    private Vector2 cutEndPoint;
    private float cutTraceWidth;
    private bool isCreatingCut;
    private bool isCutting;
    private List<string> animationStates;
    private bool hasUpgrade;
    private float initialHorizontalVelocity;
    private int health = 3;
    private int totalCuts = 0;
    private bool isCuttingBoss;
    private Vector3 respawnPoint;

    public float maxCutLength;
    public float cutSpeed;
    
    public void Start() {
        // Player collision box and rigid body
        capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
        groundCollider = gameObject.GetComponent<BoxCollider2D>();
        rigidBody = gameObject.GetComponent<Rigidbody2D>();

        animator = gameObject.GetComponent<Animator>();

        // Line renderer line width
        cutTraceWidth = 0.01f;
        maxCutLength = 6f;
        cutSpeed = 2.5f;

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
        isCuttingBoss = false;
        initialHorizontalVelocity = 0;

        // Upgrade
        hasUpgrade = false;

        // Respawn coords
        respawnPoint = transform.position;

        // Possible states for animation
        animationStates = new List<string>();
        animationStates.Add("IsPreparing");
        animationStates.Add("IsJumping");
        animationStates.Add("IsFalling");
        animationStates.Add("IsAttacking");
        animationStates.Add("IsHurt");
        animationStates.Add("IsDead");
    }
    
    public void Update() {
        // Get mouse click on player's collision box
        if (Input.GetMouseButtonDown(0) && ((hasUpgrade && totalCuts < 2) || isGrounded())) {
            // Mouse world position
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldMousePos.z = transform.position.z;
            if (capsuleCollider.bounds.Contains(worldMousePos)) {
                // Activate creating cut flag and store the cut starting point
                isCreatingCut = true;
                cutStartPoint = transform.TransformPoint(capsuleCollider.offset);
            }
        }

        // Get mouse up click
        if (Input.GetMouseButtonUp(0)) {
            if (isCreatingCut) {
                // Add one to total cuts
                totalCuts++;

                // Disable creating cut flag
                isCreatingCut = false;

                // Clear cut trace line
                lineRenderer.positionCount = 0;
                lineRenderer.positionCount = 2;

                // Mouse world position and store cut end point
                Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldMousePos.z = transform.position.z;
                cutEndPoint = worldMousePos;

                // Enable the cutting flag and collision mask
                isCutting = true;
                isCuttingBoss = true;
                Physics2D.IgnoreLayerCollision(7, 8, true);

                // Apply cutting force based on cutting parameters
                makeCut();
            }
        }
        
        if (isCreatingCut) {
            // Mouse world position and vector of points
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldMousePos.z = transform.position.z;

            // Update cut start point to the player's position
            cutStartPoint = transform.TransformPoint(capsuleCollider.offset);

            // Limit drawing point to max cut length
            Vector2 lineDir = Vector3.Normalize(new Vector2(worldMousePos.x, worldMousePos.y) - cutStartPoint);
            Vector3 lineEndPoint = limitVector(cutStartPoint, lineDir, worldMousePos, maxCutLength);
            lineEndPoint.z = transform.position.z;

            // Draw cut trace line
            Vector3[] points = new Vector3[2] {transform.InverseTransformPoint(cutStartPoint), transform.InverseTransformPoint(lineEndPoint)};
            lineRenderer.SetPositions(points);
        }

        if (isCutting) {
            if (Utils.isBetween(rigidBody.velocity.magnitude, initialHorizontalVelocity + 0.02f, initialHorizontalVelocity - 0.02f)) {
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
                totalCuts = 0;
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
        Animator animator = gameObject.GetComponent<Animator>();

        if (collision.gameObject.CompareTag("Enemy")) {
            health -= 1;
            if (health <= 0)
                animator.Play("Dead");
            else
                animator.Play("Hurt");
        }

        if (collision.gameObject.CompareTag("Upgrade")) {
            hasUpgrade = true;
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Boss") && isCuttingBoss) {
            animator.Play("Attack");
            BossSlime boss = collision.gameObject.GetComponent<BossSlime>();
            boss.takeDamage(transform.position);

            isCuttingBoss = false;
        } else if (collision.gameObject.CompareTag("Boss") && !isCuttingBoss) {
            health -= 1;
            if (health <= 0)
                animator.Play("Dead");
            else {
                animator.Play("Hurt");
                rigidBody.position = respawnPoint;
            }
        }

        isCuttingBoss = false;
    }

    void OnTriggerEnter2D(Collider2D otherCollider) {
        if (otherCollider.gameObject.name == "BossTrigger") {
            GameObject cam = GameObject.Find("Main Camera");
            cam.GetComponent<Camera>().orthographicSize = 11;
            CameraSmoothFollow smoothFollow = cam.GetComponent<CameraSmoothFollow>();
            smoothFollow.target = GameObject.Find("BossCameraAnchor");
            health = 3;

            GameObject boss = GameObject.Find("BossSlime");
            boss.GetComponent<BossSlime>().run = true;
        }

        if (otherCollider.gameObject.CompareTag("Checkpoint")) {
            Debug.Log(respawnPoint.x);
            Vector3 checkpointPosition = otherCollider.gameObject.transform.position;
            respawnPoint = new Vector3(checkpointPosition.x + 1.32f, checkpointPosition.y, checkpointPosition.z);
            Debug.Log(respawnPoint.x);
        }
    }

    void OnTriggerExit2D(Collider2D otherCollider) {
        if (otherCollider.gameObject.CompareTag("Cuttable") && isCutting && otherCollider.gameObject.GetComponent<Cuttable>().canCut) {
            Cuttable cuttableScript = (Cuttable)otherCollider.gameObject.GetComponent<Cuttable>();
            cuttableScript.cut(cutStartPoint, cutEndPoint);

            // Change animation to falling
            animator.Play("Attack");
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
        foreach (string animationState in animationStates)
            animator.SetBool(animationState, animationState == toSetTrue);
    }

    private void makeCut() {
        // Limit end point to max length
        Vector2 lineDir = Vector3.Normalize(cutEndPoint - cutStartPoint);
        cutEndPoint = limitVector(cutStartPoint, lineDir, cutEndPoint, maxCutLength);
        
        // Apply velocity in the direction of the cut
        rigidBody.velocity = cutSpeed * Vector3.Distance(cutStartPoint, cutEndPoint) * lineDir;
        initialHorizontalVelocity = Mathf.Abs(rigidBody.velocity.x);
    }
}
