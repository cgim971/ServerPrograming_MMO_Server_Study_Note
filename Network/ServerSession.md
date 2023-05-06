ServerSession.cs

```cs
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

namespace DummyClient {
    class ServerSession : PacketSession {
        // 재정의하여 연결되었을 때 출력
        public override void OnConnected(EndPoint endPoint) {
            Console.WriteLine($"OnConnected : {endPoint}");
        }

        // 재정의하여 연결이 종료되었을 때 출력
        public override void OnDisconnected(EndPoint endPoint) {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        // 재정의하여 패킷 수신시 호출되는 함수
        public override void OnRecvPacket(ArraySegment<byte> buffer) {
            // PacketManager에 패킷데이터를 보낸다. 
            // PacketQueue에 푸쉬
            PacketManager.Instance.OnRecvPacket(this, buffer, (s, p) => PacketQueue.Instance.Push(p));
        }

        // 재정의하여 데이터 전송이 완료 됬을 때 실행하는 함수
        public override void OnSend(int numOfBytes) {
            //Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
```
