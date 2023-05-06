Session.cs

```cs
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore {
    public abstract class PacketSession : Session {
        public static readonly int HeaderSize = 2;

        // [size(2)][packetId(2)][ ... ][size(2)][packetId(2)][ ... ]
        public sealed override int OnRecv(ArraySegment<byte> buffer) {
            int processLen = 0;
            int packetCount = 0;

            while (true) {
                // 최소한 헤더는 파싱할 수 있는지 확인
                if (buffer.Count < HeaderSize)
                    break;

                // 패킷이 완전체로 도착했는지 확인
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                // 여기까지 왔으면 패킷 조립 가능
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
                packetCount++;

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            if (packetCount > 1)
                Console.WriteLine($"패킷 모아보내기 : {packetCount}");

            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session {
        Socket _socket;
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(65535);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        // _sendQueue, _pendingList 비우기
        void Clear() {
            lock (_lock) {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Start(Socket socket) {
            // socket할당
            _socket = socket;

            // 이벤트 핸들러 추가
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        // 데이터 보내기
        public void Send(List<ArraySegment<byte>> sendBuffList) {
            if (sendBuffList.Count == 0)
                return;

            lock (_lock) {
                foreach (ArraySegment<byte> sendBuff in sendBuffList)
                    _sendQueue.Enqueue(sendBuff);

                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        // 데이터 보내기 
        public void Send(ArraySegment<byte> sendBuff) {
            lock (_lock) {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        // 연결 끊기
        public void Disconnect() {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }

        #region 네트워크 통신
        // 데이터 보내기
        void RegisterSend() {
            // 연결이 끊겼는지
            if (_disconnected == 1)
                return;

            // 보낼 데이터가 잇는지
            while (_sendQueue.Count > 0) {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            }
            _sendArgs.BufferList = _pendingList;
            
            try {
                // 비동기로 실행하여 pending은 보내졌는지 
                bool pending = _socket.SendAsync(_sendArgs);
                // false면 데이터가 보내짐
                if (pending == false)
                    OnSendCompleted(null, _sendArgs);
            }
            catch (Exception e) {
                Console.WriteLine($"RegisterSend Failed {e}");
            }
        }

        // 전송이 완료됫을 때 실행되는 함수
        void OnSendCompleted(object sender, SocketAsyncEventArgs args) {
            lock (_lock) {
                // args.BytesTransferred는 데이터의 크기를 바이트 단위로 가져온다
                // args.SocektError 소켓 오류 여부 확인
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success) {
                    try {
                        // _pendingList 초기화
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        // 보낼 데이터가 있다면 RegisterSend 실행
                        if (_sendQueue.Count > 0)
                            RegisterSend();
                    }
                    catch (Exception e) {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else {
                    // 문제 생기면 연결 끊기
                    Disconnect();
                }
            }
        }

        // 데이터를 받기 위한 함수
        void RegisterRecv() {
            // 연결이 끊겨잇으면 리턴
            if (_disconnected == 1)
                return;

            _recvBuffer.Clean();
            // 쓰기 가능한 바이트 영역을 가져옴
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try {
                // 데이터를 받음
                bool pending = _socket.ReceiveAsync(_recvArgs);
                // 받았으면
                if (pending == false)
                    OnRecvCompleted(null, _recvArgs);
            }
            catch (Exception e) {
                Console.WriteLine($"RegisterRecv Failed {e}");
            }
        }

        // 성공적으로 받으면
        void OnRecvCompleted(object sender, SocketAsyncEventArgs args) {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success) {
                try {
                    // Write 커서 이동
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false) {
                        Disconnect();
                        return;
                    }

                    // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || _recvBuffer.DataSize < processLen) {
                        Disconnect();
                        return;
                    }

                    // Read 커서 이동
                    if (_recvBuffer.OnRead(processLen) == false) {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch (Exception e) {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else {
                Disconnect();
            }
        }
        #endregion
    }
}
```