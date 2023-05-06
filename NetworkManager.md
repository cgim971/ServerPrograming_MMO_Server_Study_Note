NetworkManager.cs

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
        List<IPacket> list = PacketQueue.Instance.PopAll();
        foreach (IPacket packet in list) {
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }

    // 데이터 보내기
    public void Send(ArraySegment<byte> sendBuff) => _session.Send(sendBuff);
}
```