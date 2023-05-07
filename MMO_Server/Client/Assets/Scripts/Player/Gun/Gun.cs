using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {

    public Transform FireTs;
    private Bullet _bullet;

    private void Start() {
        _bullet = Resources.Load<Bullet>("Bullet");
    }

    public void Fire(bool isPlayer = false) {
        Bullet newBullet = Instantiate(_bullet, null);
        newBullet.Init(FireTs.position, transform.forward, isPlayer);
    }
}
