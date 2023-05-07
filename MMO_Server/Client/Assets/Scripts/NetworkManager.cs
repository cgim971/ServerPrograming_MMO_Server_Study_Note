using DummyClient;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    public static NetworkManager Instance => _instance;
    private static NetworkManager _instance;

    ServerSession _session = new ServerSession();

    private void Awake() {
        _instance = this;
    }

    void Start() {
        // DNS (Domain Name System)
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); // IP주소, 포트번호 입력

        Connector connector = new Connector();
        connector.Connect(endPoint, () => _session, 1);
    }

    void Update() {
        List<IPacket> list = PacketQueue.Instance.PopAll();
        foreach (IPacket packet in list) {
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }

    public void Send(ArraySegment<byte> sendBuff) => _session.Send(sendBuff);
}
