using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server {
    class GameRoom : IJobQueue {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job) {
            _jobQueue.Push(job);
        }

        public void Flush() {
            // N ^ 2
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);

            // Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        public void Broadcast(ArraySegment<byte> segment) {
            _pendingList.Add(segment);
        }

        public void Enter(ClientSession session) {
            // 플레이어 추가하고
            _sessions.Add(session);
            session.Room = this;

            S_PlayerList players = new S_PlayerList();
            foreach (ClientSession s in _sessions) {
                s.PosY = 1;
                players.players.Add(new S_PlayerList.Player {
                    isSelf = (s == session),
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ,
                    healthAmount = s.Health,
                    Life = s.Life
                });
            }

            session.Send(players.Write());

            S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
            enter.playerId = session.SessionId;
            enter.posX = 0;
            enter.posY = 1;
            enter.posZ = 0;
            enter.healthAmount = session.Health;
            enter.Life = session.Life;
            Broadcast(enter.Write());
        }

        public void Leave(ClientSession session) {
            // 플레이어 제거
            _sessions.Remove(session);

            // 제거를 모두에게 알림  
            S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
            leave.playerId = session.SessionId;
            Broadcast(leave.Write());
        }

        public void Move(ClientSession session, C_Move packet) {
            // 좌표 바꿔주고
            session.PosX = packet.posX;
            session.PosY = packet.posY;
            session.PosZ = packet.posZ;
            session.DirX = packet.dirX;
            session.DirZ = packet.dirZ;

            // 모두에게 알림
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = session.SessionId;
            move.posX = session.PosX;
            move.posY = session.PosY;
            move.posZ = session.PosZ;
            move.dirX = session.DirX;
            move.dirZ = session.DirZ;
            Broadcast(move.Write());
        }

        public void Rot(ClientSession session, C_Rot packet) {
            session.Angle = packet.angle;

            S_BroadcastRot rot = new S_BroadcastRot();
            rot.playerId = session.SessionId;
            rot.angle = session.Angle;
            Broadcast(rot.Write());
        }

        public void Fire(ClientSession session, C_Fire packet) {
            S_BroadcastFire fire = new S_BroadcastFire();
            fire.playerId = session.SessionId;
            Broadcast(fire.Write());
        }

        public void Hit(ClientSession session, C_Hit packet) {
            ClientSession hitSession = SessionManager.Instance.Find(packet.hittedPlayerId);

            hitSession.Health -= packet.hitAmount;

            S_BroadcastHit hit = new S_BroadcastHit();
            hit.hittedPlayerId = packet.hittedPlayerId;
            hit.playerHealth = hitSession.Health;
            Broadcast(hit.Write());

            if (hitSession.Health <= 0) {
                Die(hitSession);
            }
        }

        public void Die(ClientSession session) {
            session.Life -= 1;

            S_BroadcastDie die = new S_BroadcastDie();
            die.playerId = session.SessionId;
            die.Life = session.Life;
            Broadcast(die.Write());
        }

        public void Revive(ClientSession session, C_Revive packet) {
            session.Health = 100;

            S_BroadcastRevive revive = new S_BroadcastRevive();
            revive.playerId = session.SessionId;
            revive.healthAmount = session.Health;
            Broadcast(revive.Write());
        }
    }
}
