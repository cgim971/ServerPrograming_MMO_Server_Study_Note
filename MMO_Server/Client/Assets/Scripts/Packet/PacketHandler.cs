using DummyClient;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketHandler : MonoBehaviour {

    public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet) {
        S_BroadcastEnterGame pkt = packet as S_BroadcastEnterGame;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.EnterGame(pkt);
    }

    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet) {
        S_BroadcastLeaveGame pkt = packet as S_BroadcastLeaveGame;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.LeaveGame(pkt);
    }

    public static void S_PlayerListHandler(PacketSession session, IPacket packet) {
        S_PlayerList pkt = packet as S_PlayerList;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.Add(pkt);
    }

    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet) {
        S_BroadcastMove pkt = packet as S_BroadcastMove;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.Move(pkt);
    }

    public static void S_BroadcastRotHandler(PacketSession session, IPacket packet) {
        S_BroadcastRot pkt = packet as S_BroadcastRot;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.Rot(pkt);
    }

    public static void S_BroadcastFireHandler(PacketSession session, IPacket packet) {
        S_BroadcastFire pkt = packet as S_BroadcastFire;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.Fire(pkt);
    }
    public static void S_BroadcastHitHandler(PacketSession session, IPacket packet) {
        S_BroadcastHit pkt = packet as S_BroadcastHit;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.Hit(pkt);
    }
    public static void S_BroadcastDieHandler(PacketSession session, IPacket packet) {
        S_BroadcastDie pkt = packet as S_BroadcastDie;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.Die(pkt);
    }
    public static void S_BroadcastReviveHandler(PacketSession session, IPacket packet) {
        S_BroadcastRevive pkt = packet as S_BroadcastRevive;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.Revive(pkt);
    }
}
