using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundFollower : MonoBehaviour {
    public GameObject followTarget;

    void Update() {
        transform.position = new Vector3(followTarget.transform.position.x, followTarget.transform.position.y, transform.position.z);
    }
}
