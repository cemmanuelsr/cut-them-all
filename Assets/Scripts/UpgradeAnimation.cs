using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeAnimation : MonoBehaviour
{
    private Vector3 startPosition;
    private int dir;

    void Start()
    {
        startPosition = transform.position;
        dir = 1;
    }

    void Update()
    {
        if ((transform.position.y > startPosition.y + 0.25f) && (dir == 1))
            dir = -1;
        else if ((transform.position.y < startPosition.y - 0.25f) && (dir == -1))
            dir = 1;

        transform.position += new Vector3(0.0f, 1f, 0.0f) * Time.deltaTime * dir;
    }
}
