**PacketHandler.cs**

PacketHandler는 Packet을 받았을 때 그에 맞는 실행될 함수를 작성한다.
PacketHandler를 통해 받아온 정보를 형태를 as 를 이용해 형변환해준다.

```cs
using DummyClient;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketHandler : MonoBehaviour {

    // 받은 패킷을 변환 후 실행
    // 이미 입장을 한 상태에서 다른 플레이어가 입장을 한다면 여기서 추가
    public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet) {
        S_BroadcastEnterGame pkt = packet as S_BroadcastEnterGame;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.EnterGame(pkt);
    }

    // 받은 패킷을 변환 후 실행
    // 누군가 나가면
    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet) {
        S_BroadcastLeaveGame pkt = packet as S_BroadcastLeaveGame;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.LeaveGame(pkt);
    }

    // 받은 패킷을 변환 후 실행
    // GameRoom에 접속한 플레이어 리스트 
    public static void S_PlayerListHandler(PacketSession session, IPacket packet) {
        S_PlayerList pkt = packet as S_PlayerList;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.Add(pkt);
    }

    // 받은 패킷을 변환 후 실행
    // 누군가 이동을 하면 데이터를 받는다
    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet) {
        S_BroadcastMove pkt = packet as S_BroadcastMove;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.Move(pkt);
    }
}
```
