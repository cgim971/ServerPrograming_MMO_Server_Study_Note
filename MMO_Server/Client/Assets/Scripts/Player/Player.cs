using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour {
    [SerializeField] private int playerId;
    public int PlayerId { get => playerId; set => playerId = value; }
    private Vector3 _prevDir = Vector3.zero;
    protected float _speed = 5f;

    public int Life = 3;

    CharacterController _characterController;
    public Gun Gun { get; set; }
    public HealthSystem HealthSystem { get; set; }

    Coroutine _moveCoroutine;
    Coroutine _hitCoroutine;
    Coroutine _reviveCoroutine;
    Material _material;

    private void Awake() {
        _characterController = GetComponent<CharacterController>();
        Gun = transform.Find("Gun").GetComponent<Gun>();
        HealthSystem = transform.Find("Canvas").Find("HealthSystem").GetComponent<HealthSystem>();
    }

    protected virtual void Start() {
        HealthSystem.OnDamaged += HealthSystem_OnDamaged;
        HealthSystem.OnDied += HealthSystem_OnDied;
        HealthSystem.OnRevive += HealthSystem_OnRevive;

        _material = GetComponent<Renderer>().material;
    }

    private void HealthSystem_OnRevive(object sender, System.EventArgs e) {
        if (_reviveCoroutine != null)
            StopCoroutine(_reviveCoroutine);

        _reviveCoroutine = StartCoroutine(ReviveEffect());
    }

    IEnumerator ReviveEffect() {
        _material.color = Color.green;
        yield return new WaitForSeconds(0.2f);
        _material.color = Color.white;
    }

    protected virtual void HealthSystem_OnDied(object sender, System.EventArgs e) => Debug.Log("Á×À½");

    private void HealthSystem_OnDamaged(object sender, System.EventArgs e) {
        if (_hitCoroutine != null)
            StopCoroutine(_hitCoroutine);

        _hitCoroutine = StartCoroutine(HitEffect());
    }

    IEnumerator HitEffect() {
        _material.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        _material.color = Color.white;
    }

    public void Move(Vector3 currentPos, Vector3 dir) {
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);

        transform.position = currentPos;
        _prevDir = dir;
        _moveCoroutine = StartCoroutine(CoMove());
    }

    IEnumerator CoMove() {
        while (true) {
            yield return new WaitUntil(() => _prevDir != Vector3.zero);
            _characterController.Move(_prevDir * _speed * Time.deltaTime);
        }
    }

    public void Rot(float angle) {
        float receivedAngle = angle;
        Quaternion playerRotation = Quaternion.Euler(0f, receivedAngle, 0f);
        transform.rotation = playerRotation;
    }
}
