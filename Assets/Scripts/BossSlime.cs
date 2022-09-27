using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSlime : MonoBehaviour
{
    private int movementDir = 1;
    private int health = 3;
    private Vector3 possiblePositionHealth2_right = new Vector3(208.624f, 3.107f, 0f);
    private Vector3 possiblePositionHealth2_left = new Vector3(176.94f, 3.107f, 0f);
    private CapsuleCollider2D boxCollider;
    private Rigidbody2D rigidBody;

    public bool run = false;

    void Start()
    {
        boxCollider = gameObject.GetComponent<CapsuleCollider2D>();
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update() {
        if (!run)
            return;

        if(health == 3) {
            if (isNextToWall())
                movementDir *= -1;
            transform.position += new Vector3(2f, 0.0f, 0.0f) * Time.deltaTime * movementDir; 
        } else if (health == 2) {
            if (isGrounded()) {
                rigidBody.velocity = new Vector3(movementDir * 11f, 14f ,0.0f);
                movementDir *= -1;
            }
        } else if (health == 1) {
            if (isGrounded()) {
                rigidBody.velocity = new Vector3(movementDir * 9.5073f, 13.2764f ,0.0f);
                movementDir *= -1;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collider) {
        if (collider.gameObject.CompareTag("Cuttable") && collider.gameObject.GetComponent<Rigidbody2D>().velocity.y < 0) {
            Destroy(gameObject);
        }
    }

    bool isNextToWall() {
        int playerMask = ~(1 << 8);
        Vector2 origin = new Vector2(transform.position.x + (float)movementDir * transform.localScale.x / 5.0f, transform.position.y);
        return Physics2D.Raycast(origin, Vector2.right * (float)movementDir, 0.001f, playerMask).collider != null;
    }

    private bool isGrounded() {
        return boxCollider.IsTouchingLayers(-1);
    }

    public void takeDamage(Vector3 playerPosition) {
        health -= 1;
        Animator animator = gameObject.GetComponent<Animator>();

        if (health <= 0) {
            animator.PlayInFixedTime("Death");
            Destroy(gameObject);
        } else
            animator.Play("Hurt");

        if (health <= 2) {
            float distanceToRight = Vector3.Distance(playerPosition, possiblePositionHealth2_right);
            float distanceToLeft = Vector3.Distance(playerPosition, possiblePositionHealth2_left);

            if (distanceToRight >= distanceToLeft) {
                transform.position = possiblePositionHealth2_right;
                movementDir = -1;
            } else {
                transform.position = possiblePositionHealth2_left;
                movementDir = 1;
            }
        }
    }
}
