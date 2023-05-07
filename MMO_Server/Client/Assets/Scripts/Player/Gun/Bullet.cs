using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    private Rigidbody _rigidbody;
    public float Speed = 20f;

    private bool _isPlayer;

    public void Init(Vector3 pos, Vector3 dir, bool isPlayer = false) {
        transform.position = pos;
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.velocity = dir * Speed;
        _isPlayer = isPlayer;
        Destroy(gameObject, 10f);
    }

    void OnCollisionEnter(Collision collision) {
        if (_isPlayer) {
            Player player = collision.transform.GetComponent<Player>();
            if (player != null) {
                C_Hit hitPacket = new C_Hit();
                Debug.Log(player.PlayerId);
                hitPacket.hittedPlayerId = player.PlayerId;
                hitPacket.hitAmount = 10;
                NetworkManager.Instance.Send(hitPacket.Write());
            }
        }

        Destroy(gameObject);
    }
}
