**SendBuffer.cs**

SendBuffer는 서버에서 클라이언트로 데이터를 보낼 때 사용하는 버퍼 클래스이다.

```cs
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerCore {
    // SendBuffer를 도우는 SendBufferHelper 생성 
    // 효율적으로 관리하기 위해
    public class SendBufferHelper {
        // 쓰레드 내부에서만 전역 사용하기 위해 'ThreadLocal'을 통해 구현
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        // Chunk 쓰레드마다 가질 버퍼 최대 크기
        // 청크는 일정한 크기의 데이터 조각으로 분할하여 전송함으로써 데이터 손실을 방지하고, 데이터 전송 시간을 단축시키는 데 사용됩니다.
        public static int ChunkSize { get; set; } = 65535 * 100;

        // rserveSize만큼의 크기를 ArraySegment<byte> 형태의 바이트 배열을 반환 함수
        public static ArraySegment<byte> Open(int reserveSize) {
            // SendBuffer 객체를 CurrentBuffer.Value 속성에서 가져온다.
            // 널인 경우 버퍼 생성
            if (CurrentBuffer.Value == null)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            // 현재 버퍼 크기보다 더 많은 버퍼를 요구할 시 새로운 버퍼 생성
            if (CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            // 모두 통과하는 건 여유 버퍼가 있다는 의미로 Open()하여 반환
            return CurrentBuffer.Value.Open(reserveSize);
        }

        // 사용한 usedSize만큼의 크기를 Close() 하여 남은 부분을 반환
        public static ArraySegment<byte> Close(int usedSize) {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    // byte[] _buffer을 관리하는 클래스
    public class SendBuffer {
        byte[] _buffer; 
        int _usedSize = 0;

        // FreeSize는 버퍼의 크기에 사용한 버퍼의 크기만큼 뺀 사용가능한 _buffer의 크기
        public int FreeSize => _buffer.Length - _usedSize;

        // 생성자 chunkSize로 byte의 크기를 정의
        public SendBuffer(int chunkSize) {
            _buffer = new byte[chunkSize];
        }

        // _buffer의 사용한 _usedSize의 크기의 위치부터, reserveSize의 크기만큼 버퍼 반환
        public ArraySegment<byte> Open(int reserveSize) {
            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        // _buffer의 사용한 _usedSize의 크기의 위치부터 사용한 usedSize의 위치만큼 반환
        public ArraySegment<byte> Close(int usedSize) {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            // 이제 사용하지 못 한다 하기 위해 _usedSize에 usedSize를 더함
            _usedSize += usedSize;
            return segment;
        }
    }
}

```