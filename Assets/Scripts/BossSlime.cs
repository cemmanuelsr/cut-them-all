using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSlime : MonoBehaviour
{
    private int movementDir = 1;
    private int health = 3;

    void Update() {
        if(health == 3) {
            if (isNextToWall())
                movementDir *= -1;
            transform.position += new Vector3(2f, 0.0f, 0.0f) * Time.deltaTime * movementDir; 
        } else if (health == 2) {
            
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

    public void takeDamage() {
        health -= 1;
        Animator animator = gameObject.GetComponent<Animator>();

        if (health <= 0) {
            animator.PlayInFixedTime("Death");
            Destroy(gameObject);
        } else
            animator.Play("Hurt");
    }
}
