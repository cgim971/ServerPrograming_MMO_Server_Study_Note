using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LookRotation : MonoBehaviour {
    private Transform _target;
    private void Start() {
        _target = Camera.main.transform;
    }

    void Update() {
        transform.rotation = Quaternion.LookRotation(transform.position - _target.position);
    }

}
