**Connector.cs**

Connector는 서버와 클라이언트를 연결하기 위한 기능을 제공하는 클래스 

```cs
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore {
    public class Connector {
        // Func 대리자 선언 
        Func<Session> _sessionFactory;

        // 연결 함수
        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1) {
            for (int i = 0; i < count; i++) {
                // 휴대폰 설정
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);    // 소켓을 생성 후
                _sessionFactory = sessionFactory;

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnConnectCompleted;
                args.RemoteEndPoint = endPoint;
                // UserToken: 생성한 Socket을 잠시 저장했다가, 다른 함수에서 사용할 때 꺼내쓸 수 있음.
                args.UserToken = socket;

                RegisterConnect(args);
            }
        }

        // Socket객체를 가져와서 비동기적 연결 시도
        // 성공시 OnConnectCompleted 이벤트 핸들러 호출
        void RegisterConnect(SocketAsyncEventArgs args) {
            Socket socket = args.UserToken as Socket;
            if (socket == null)
                return;

            bool pending = socket.ConnectAsync(args);
            if (pending == false)
                OnConnectCompleted(null, args);
        }

        // 이벤트 핸들러에서는 연결이 성공했을 때 새로운 세션을 생성하고, 연결된 소켓을 세션에 할당
        void OnConnectCompleted(object sender, SocketAsyncEventArgs args) {
            if (args.SocketError == SocketError.Success) {
                Session session = _sessionFactory.Invoke();
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else {
                Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
            }
        }
    }
}
```