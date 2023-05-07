**MyPlayer.cs**

MyPlayer는 Player를 상속 받아서 기본적으로 모든 플레이어가 고유 아이디를 가질 수 있게 작성

```cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayer : Player {
    private NetworkManager _networkManager;

    private void Start() {
        _networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        // 코루틴으로 0.25f마다 데이터를 보낸다.
        StartCoroutine("CoSendPacket");
    }

    IEnumerator CoSendPacket() {
        while (true) {
            yield return new WaitForSeconds(0.25f);

            // C_Move 형태로 데이터를 보내기 때문에 값을 지정해준다.
            C_Move movePacket = new C_Move();
            movePacket.posX = UnityEngine.Random.Range(-50, 50);
            movePacket.posY = 1;
            movePacket.posZ = UnityEngine.Random.Range(-50, 50);

            // 데이터 보내기
            _networkManager.Send(movePacket.Write());
        }
    }

}
```