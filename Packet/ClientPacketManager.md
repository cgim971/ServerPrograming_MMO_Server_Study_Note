```cs
using ServerCore;
using System;
using System.Collections.Generic;

public class PacketManager {
    #region Singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance { get { return _instance; } }
    #endregion

    PacketManager() {
        Register();
    }

    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register() {
        _makeFunc.Add((ushort)PacketID.S_BroadcastEnterGame, MakePacket<S_BroadcastEnterGame>);
        _handler.Add((ushort)PacketID.S_BroadcastEnterGame, PacketHandler.S_BroadcastEnterGameHandler);
        _makeFunc.Add((ushort)PacketID.S_BroadcastLeaveGame, MakePacket<S_BroadcastLeaveGame>);
        _handler.Add((ushort)PacketID.S_BroadcastLeaveGame, PacketHandler.S_BroadcastLeaveGameHandler);
        _makeFunc.Add((ushort)PacketID.S_PlayerList, MakePacket<S_PlayerList>);
        _handler.Add((ushort)PacketID.S_PlayerList, PacketHandler.S_PlayerListHandler);
        _makeFunc.Add((ushort)PacketID.S_BroadcastMove, MakePacket<S_BroadcastMove>);
        _handler.Add((ushort)PacketID.S_BroadcastMove, PacketHandler.S_BroadcastMoveHandler);

    }

    // 패킷을 생성하고 핸들링하는 함수
    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null) {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if (_makeFunc.TryGetValue(id, out func)) {
            IPacket packet = func.Invoke(session, buffer);
            if (onRecvCallback != null)
                onRecvCallback.Invoke(session, packet);
            else
                HandlePacket(session, packet);
        }
    }

    // 제네릭 함수로 T는 IPacket인터페이스를 구현하는 함수
    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() {
        // 세션과 버퍼를 매개변수로 받아서 T타입의 패킷을 생성하고
        T pkt = new T();
        // 버퍼에서 읽은 데이터를 저장
        pkt.Read(buffer);
        return pkt;
    }

    // 패킷 ID에 해당하는 핸들러를 찾아서 실행
    public void HandlePacket(PacketSession session, IPacket packet) {
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);
    }
}
```