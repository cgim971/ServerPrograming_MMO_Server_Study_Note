using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MyPlayer : Player {
    private NetworkManager _networkManager;
    Transform _mainCamTs;
    private Vector3 _prevDir = Vector3.zero;
    float _prevAngle = 0f;

    protected override void Start() {
        base.Start();
        _networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

        _mainCamTs = Camera.main.transform;
        _mainCamTs.GetComponent<ThirdPersonCamera>().TargetTs = transform;
    }

    private void Update() {
        // 방향을 계산하고 이전 방향과 비교하여 변경되었다면 서버로 방향 정보를 보냅니다.
        Vector3 dir = CalculateDirection();
        if (dir != _prevDir) {
            _prevDir = dir;
            SendInputToServer(_prevDir);
        }

        Rot();

        if (Input.GetMouseButtonDown(0)) {
            C_Fire firePacket = new C_Fire();
            _networkManager.Send(firePacket.Write());
        }
    }


    private Vector3 CalculateDirection() {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(x, 0, z).normalized;

        dir = _mainCamTs.TransformDirection(dir);
        dir.y = 0f;

        return dir;
    }

    private void SendInputToServer(Vector3 dir) {
        C_Move movePacket = new C_Move();
        movePacket.posX = transform.position.x;
        movePacket.posY = 1;
        movePacket.posZ = transform.position.z;
        movePacket.dirX = dir.x;
        movePacket.dirZ = dir.z;

        _networkManager.Send(movePacket.Write());
    }

    private void Rot() {
        Vector3 cameraForward = _mainCamTs.forward;
        Vector3 playerForward = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
        Quaternion playerRotation = Quaternion.LookRotation(playerForward);
        float playerAngle = playerRotation.eulerAngles.y;

        float angle = playerAngle;
        if (angle != _prevAngle) {
            SendRotToServer(angle);
        }
    }

    private void SendRotToServer(float angle) {
        C_Rot packet = new C_Rot();
        packet.angle = angle;
        _networkManager.Send(packet.Write());

        _prevAngle = angle;
    }

    protected override void HealthSystem_OnDied(object sender, EventArgs e) {
        base.HealthSystem_OnDied(sender, e);
        
        if (Life == 0) {
            C_LeaveGame leaveGamePacket = new C_LeaveGame();
            _networkManager.Send(leaveGamePacket.Write());
        }
        else {
            C_Revive revivePacket = new C_Revive();
            _networkManager.Send(revivePacket.Write());
        }
    }
}
