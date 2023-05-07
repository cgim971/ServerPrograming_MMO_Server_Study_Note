**PacketQueue.cs**

PacketQueue는 게임을 하면서 받아지는 Packet들을 순차적으로 받는다.
이것을 선입선출 형태의 Queue를 이용하여 받아진 순서대로 처리한다.

```cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketQueue {
    public static PacketQueue Instance { get; } = new PacketQueue();

    // Queue로 IPacket을 Enqueue하여 모든 값을 순차적으로 넣을 용도
    Queue<IPacket> _packetQueue = new Queue<IPacket>();
    object _lock = new object();

    // _packetQueue에 넣기
    public void Push(IPacket packet) {
        lock (_lock) {
            _packetQueue.Enqueue(packet);
        }
    }

    // _packetQueue에서 가장 먼저 넣은 값 빼기
    public IPacket Pop() {
        lock (_lock) {
            if (_packetQueue.Count == 0)
                return null;

            return _packetQueue.Dequeue();
        }
    }

    // _packetQueue의 값들을 리스트로 이동
    public List<IPacket> PopAll() {
        List<IPacket> list = new List<IPacket>();
        lock (_lock) {
            while (_packetQueue.Count > 0)
                list.Add(_packetQueue.Dequeue());
        }

        return list;
    }
}
```