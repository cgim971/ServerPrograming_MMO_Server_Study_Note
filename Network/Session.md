**Session.cs**

Session은 서버와 클라이언트 사이 상호 작용하기 위해 생성하는 인스턴스이다.

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

    // Session
    public abstract class Session {
        // 세션에 연결된 소켓
        Socket _socket;
        // 1이면 연결 끊김
        int _disconnected = 0;

        // 수신 데이터 버퍼
        RecvBuffer _recvBuffer = new RecvBuffer(65535);

        object _lock = new object();
        // 전송 대기열
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        // 전송중인 데이터 리스트
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        // 비동기 소켓 전송 객체
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        // 비동기 소켓 수신 객체
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        // 세션이 연결되었을 때 호출되는 함수
        public abstract void OnConnected(EndPoint endPoint);
        // 데이터 수신 시 호출되는 함수
        public abstract int OnRecv(ArraySegment<byte> buffer);
        // 데이터 전송 시 호출되는 함수
        public abstract void OnSend(int numOfBytes);
        // 연결이 종료될 때 호출되는 함수
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
            // 보낼 데이터가 없으면 반환
            if (sendBuffList.Count == 0)
                return;

            lock (_lock) {
                // 보낼 데이터 덩이가 있다면 _sendQueue에 추가
                foreach (ArraySegment<byte> sendBuff in sendBuffList)
                    _sendQueue.Enqueue(sendBuff);

                // 전송중인 데이터가 없으면 전송
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        // 데이터 보내기 
        public void Send(ArraySegment<byte> sendBuff) {
            lock (_lock) {
                // 보낼 데이터가 잇따면 _sendQueue에 추가
                _sendQueue.Enqueue(sendBuff);
                
                // 전송중인 데이터가 없으면 전송
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        // 연결 끊기
        public void Disconnect() {
            // _disconnected 변수의 값이 1이면 메서드를 더 이상 진행하지 않고 반환
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            // 연결 종료 함수 호출
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
                // 오류 메세지 출력
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
                    // 오류 메세지 출력
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