using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler {
    public static void C_LeaveGameHandler(PacketSession session, IPacket packet) {
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
            return;
        GameRoom room = clientSession.Room;
        room.Push(() => room.Leave(clientSession));
    }

    public static void C_MoveHandler(PacketSession session, IPacket packet) {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
            return;
        //Console.WriteLine($"{movePacket.posX}, {movePacket.posY}, {movePacket.posZ}");

        GameRoom room = clientSession.Room;
        room.Push(() => room.Move(clientSession, movePacket));
    }
    public static void C_RotHandler(PacketSession session, IPacket packet) {
        C_Rot rotPacket = packet as C_Rot;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
            return;

        GameRoom room = clientSession.Room;
        room.Push(() => room.Rot(clientSession, rotPacket));
    }

    public static void C_FireHandler(PacketSession session, IPacket packet) {
        C_Fire firePacket = packet as C_Fire;
        ClientSession clientSession = session as ClientSession;
        if (clientSession.Room == null)
            return;

        GameRoom room = clientSession.Room;
        room.Push(() => room.Fire(clientSession, firePacket));
    }

    public static void C_HitHandler(PacketSession session, IPacket packet) {
        C_Hit hitPacket = packet as C_Hit;
        ClientSession clientSession = session as ClientSession;
        if (clientSession.Room == null)
            return;

        GameRoom room = clientSession.Room;
        room.Push(() => room.Hit(clientSession, hitPacket));
    }

    public static void C_ReviveHandler(PacketSession session, IPacket packet) {
        C_Revive revivePacket = packet as C_Revive;
        ClientSession clientSession = session as ClientSession;
        if (clientSession.Room == null)
            return;

        GameRoom room = clientSession.Room;
        room.Push(() => room.Revive(clientSession, revivePacket));
    }
}
