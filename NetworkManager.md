**NetworkManager.cs**

유니티에서 작업을 할 때 주의사항
- Span과 BitConverter.TryWriteBytes를 사용할 수가 없다.
- Unity의 Main 쓰레드 외 백그라운드 쓰레드에서 유니티 객체에 접근하려고 하면 Crashing이 발생한다. ⇒ 게임 로직은 Main 쓰레드에서만 작동하도록 해야 한다


```cs
using DummyClient;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    ServerSession _session = new ServerSession();

    void Start() {
        // DNS (Domain Name System)
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); // IP주소, 포트번호 입력

        Connector connector = new Connector();
        connector.Connect(endPoint, () => _session, 1);
    }

    // 만약 실행할 것이 있다면 
    void Update() {
        // 이렇게 두면 매 프레임 당 하나의 패킷만 처리
        // List<IPacket> list = PacketQueue.Instance.Pop();
        // 프레임 안에 최대한 처리할 수 있을만큼의 패킷을 처리하면 좋다.
        List<IPacket> list = PacketQueue.Instance.PopAll();
        foreach (IPacket packet in list) {
            // 유니티 메인 쓰레드라서 해당 패킷에 대한 작업 진행
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }

    // 데이터 보내기
    public void Send(ArraySegment<byte> sendBuff) => _session.Send(sendBuff);
}
```