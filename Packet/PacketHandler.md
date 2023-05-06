```cs
using DummyClient;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketHandler : MonoBehaviour {

    // 받은 패킷을 변환 후 실행
    public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet) {
        S_BroadcastEnterGame pkt = packet as S_BroadcastEnterGame;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.EnterGame(pkt);
    }

    // 받은 패킷을 변환 후 실행
    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet) {
        S_BroadcastLeaveGame pkt = packet as S_BroadcastLeaveGame;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.LeaveGame(pkt);
    }

    // 받은 패킷을 변환 후 실행
    public static void S_PlayerListHandler(PacketSession session, IPacket packet) {
        S_PlayerList pkt = packet as S_PlayerList;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.Add(pkt);
    }

    // 받은 패킷을 변환 후 실행
    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet) {
        S_BroadcastMove pkt = packet as S_BroadcastMove;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.Move(pkt);
    }

}
```
