using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSmoothFollow : MonoBehaviour
{
    public GameObject target;
    public float smoothLambda = 0.6f;

    private Vector3 followVelocity;

    void LateUpdate() {
        if (target) {
            Vector3 targetPos = Camera.main.WorldToViewportPoint(target.transform.position);
            Vector3 deltaPos = target.transform.position - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, targetPos.z));
            Vector3 destination = transform.position + deltaPos;
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref followVelocity, smoothLambda);
        }
    }
}
