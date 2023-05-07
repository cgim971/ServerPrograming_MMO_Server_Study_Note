using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;

namespace Server {
    class ClientSession : PacketSession {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        public float Angle { get; set; }

        public float DirX { get; set; }
        public float DirZ { get; set; }

        public int Health { get; set; } = 100;
        public int Life { get; set; } = 3;


        public override void OnConnected(EndPoint endPoint) {
            Console.WriteLine($"OnConnected : {endPoint}");

            Program.Room.Push(() => Program.Room.Enter(this));
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer) {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint) {
            SessionManager.Instance.Remove(this);
            if (Room != null) {
                GameRoom room = Room;
                room.Push(() => room.Leave(this));
                Room = null;
            }

            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes) {
            //Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
