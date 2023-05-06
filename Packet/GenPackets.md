```cs
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID {
    S_BroadcastEnterGame = 1,
    C_LeaveGame = 2,
    S_BroadcastLeaveGame = 3,
    S_PlayerList = 4,
    C_Move = 5,
    S_BroadcastMove = 6,

}

// IPacket을 인터페이스로 생성
public interface IPacket {
    ushort Protocol { get; }
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}


// Server 에서 게임룸에 들어오게 되면 플레이어들의 정보를 받아 Client에 보낸다
public class S_BroadcastEnterGame : IPacket {
    public int playerId;
    public float posX;
    public float posY;
    public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastEnterGame; } }

    public void Read(ArraySegment<byte> segment) {
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);
        // playerId에 int의 길이만큼 넣고 count에 int크기만큼 더한다. 
        this.playerId = BitConverter.ToInt32(segment.Array, segment.Offset + count);
        count += sizeof(int);
        // posX에 float의 길이만큼 넣고 count에 float크기만큼 더한다. 
        this.posX = BitConverter.ToSingle(segment.Array, segment.Offset + count);
        count += sizeof(float);
        // posY에 float의 길이만큼 넣고 count에 float크기만큼 더한다. 
        this.posY = BitConverter.ToSingle(segment.Array, segment.Offset + count);
        count += sizeof(float);
        // posZ에 float의 길이만큼 넣고 count에 float크기만큼 더한다. 
        this.posZ = BitConverter.ToSingle(segment.Array, segment.Offset + count);
        count += sizeof(float);
    }

    public ArraySegment<byte> Write() {
        // 4096사이즈의 ArraySegment<byte>의 영역만큼 
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        // PacketID를 넣고 ushort만큼 count증가
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_BroadcastEnterGame), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        // playerId를 넣고 int만큼 count증가
        Array.Copy(BitConverter.GetBytes(this.playerId), 0, segment.Array, segment.Offset + count, sizeof(int));
        count += sizeof(int);
        // posX 넣고 float만큼 count증가
        Array.Copy(BitConverter.GetBytes(this.posX), 0, segment.Array, segment.Offset + count, sizeof(float));
        count += sizeof(float);
        // posY 넣고 float만큼 count증가
        Array.Copy(BitConverter.GetBytes(this.posY), 0, segment.Array, segment.Offset + count, sizeof(float));
        count += sizeof(float);
        // posZ 넣고 float만큼 count증가
        Array.Copy(BitConverter.GetBytes(this.posZ), 0, segment.Array, segment.Offset + count, sizeof(float));
        count += sizeof(float);

        // 
        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

public class C_LeaveGame : IPacket {


    public ushort Protocol { get { return (ushort)PacketID.C_LeaveGame; } }

    public void Read(ArraySegment<byte> segment) {
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);

    }

    public ArraySegment<byte> Write() {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.C_LeaveGame), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);


        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

public class S_BroadcastLeaveGame : IPacket {
    public int playerId;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastLeaveGame; } }

    public void Read(ArraySegment<byte> segment) {
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.playerId = BitConverter.ToInt32(segment.Array, segment.Offset + count);
        count += sizeof(int);
    }

    public ArraySegment<byte> Write() {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_BroadcastLeaveGame), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes(this.playerId), 0, segment.Array, segment.Offset + count, sizeof(int));
        count += sizeof(int);

        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

public class S_PlayerList : IPacket {
    public class Player {
        public bool isSelf;
        public int playerId;
        public float posX;
        public float posY;
        public float posZ;

        public void Read(ArraySegment<byte> segment, ref ushort count) {
            this.isSelf = BitConverter.ToBoolean(segment.Array, segment.Offset + count);
            count += sizeof(bool);
            this.playerId = BitConverter.ToInt32(segment.Array, segment.Offset + count);
            count += sizeof(int);
            this.posX = BitConverter.ToSingle(segment.Array, segment.Offset + count);
            count += sizeof(float);
            this.posY = BitConverter.ToSingle(segment.Array, segment.Offset + count);
            count += sizeof(float);
            this.posZ = BitConverter.ToSingle(segment.Array, segment.Offset + count);
            count += sizeof(float);
        }

        public bool Write(ArraySegment<byte> segment, ref ushort count) {
            bool success = true;
            Array.Copy(BitConverter.GetBytes(this.isSelf), 0, segment.Array, segment.Offset + count, sizeof(bool));
            count += sizeof(bool);
            Array.Copy(BitConverter.GetBytes(this.playerId), 0, segment.Array, segment.Offset + count, sizeof(int));
            count += sizeof(int);
            Array.Copy(BitConverter.GetBytes(this.posX), 0, segment.Array, segment.Offset + count, sizeof(float));
            count += sizeof(float);
            Array.Copy(BitConverter.GetBytes(this.posY), 0, segment.Array, segment.Offset + count, sizeof(float));
            count += sizeof(float);
            Array.Copy(BitConverter.GetBytes(this.posZ), 0, segment.Array, segment.Offset + count, sizeof(float));
            count += sizeof(float);
            return success;
        }
    }
    public List<Player> players = new List<Player>();

    public ushort Protocol { get { return (ushort)PacketID.S_PlayerList; } }

    public void Read(ArraySegment<byte> segment) {
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.players.Clear();
        ushort playerLen = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
        count += sizeof(ushort);
        for (int i = 0; i < playerLen; i++) {
            Player player = new Player();
            player.Read(segment, ref count);
            players.Add(player);
        }
    }

    public ArraySegment<byte> Write() {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_PlayerList), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort)this.players.Count), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        foreach (Player player in this.players)
            player.Write(segment, ref count);

        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

public class C_Move : IPacket {
    public float posX;
    public float posY;
    public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.C_Move; } }

    public void Read(ArraySegment<byte> segment) {
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.posX = BitConverter.ToSingle(segment.Array, segment.Offset + count);
        count += sizeof(float);
        this.posY = BitConverter.ToSingle(segment.Array, segment.Offset + count);
        count += sizeof(float);
        this.posZ = BitConverter.ToSingle(segment.Array, segment.Offset + count);
        count += sizeof(float);
    }

    public ArraySegment<byte> Write() {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.C_Move), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes(this.posX), 0, segment.Array, segment.Offset + count, sizeof(float));
        count += sizeof(float);
        Array.Copy(BitConverter.GetBytes(this.posY), 0, segment.Array, segment.Offset + count, sizeof(float));
        count += sizeof(float);
        Array.Copy(BitConverter.GetBytes(this.posZ), 0, segment.Array, segment.Offset + count, sizeof(float));
        count += sizeof(float);

        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

public class S_BroadcastMove : IPacket {
    public int playerId;
    public float posX;
    public float posY;
    public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastMove; } }

    public void Read(ArraySegment<byte> segment) {
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.playerId = BitConverter.ToInt32(segment.Array, segment.Offset + count);
        count += sizeof(int);
        this.posX = BitConverter.ToSingle(segment.Array, segment.Offset + count);
        count += sizeof(float);
        this.posY = BitConverter.ToSingle(segment.Array, segment.Offset + count);
        count += sizeof(float);
        this.posZ = BitConverter.ToSingle(segment.Array, segment.Offset + count);
        count += sizeof(float);
    }

    public ArraySegment<byte> Write() {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_BroadcastMove), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes(this.playerId), 0, segment.Array, segment.Offset + count, sizeof(int));
        count += sizeof(int);
        Array.Copy(BitConverter.GetBytes(this.posX), 0, segment.Array, segment.Offset + count, sizeof(float));
        count += sizeof(float);
        Array.Copy(BitConverter.GetBytes(this.posY), 0, segment.Array, segment.Offset + count, sizeof(float));
        count += sizeof(float);
        Array.Copy(BitConverter.GetBytes(this.posZ), 0, segment.Array, segment.Offset + count, sizeof(float));
        count += sizeof(float);

        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

```