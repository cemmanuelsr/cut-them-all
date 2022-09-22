using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour {
    public float scrollLambda = 1.0f;

    private Material textureMaterial;
    private GameObject backgroundParent;
    void Start() {
        backgroundParent = gameObject.transform.parent.gameObject;
        textureMaterial = gameObject.GetComponent<MeshRenderer>().material;
    }

    void Update() {

        float textureOffset = (-backgroundParent.transform.position.x * scrollLambda) / 10.0f;
        textureMaterial.SetTextureOffset("_MainTex", new Vector2(textureOffset, 0.0f));
    }
}
