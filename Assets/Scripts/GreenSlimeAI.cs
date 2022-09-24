using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenSlimeAI : MonoBehaviour
{
    private int movementDir = 1;

    void Update() {
        if (isNextToWall())
            movementDir *= -1;

        transform.position += new Vector3(1.0f, 0.0f, 0.0f) * Time.deltaTime * movementDir;    
    }

    bool isNextToWall() {
        int playerMask = ~(1 << 8);
        Vector2 origin = new Vector2(transform.position.x + movementDir * transform.localScale.x / 2.0f, transform.position.y);
        return Physics2D.Raycast(origin, Vector2.right * (float)movementDir, 0.001f, playerMask).collider != null;
    }
}
