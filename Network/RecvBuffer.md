**RecvBuffer.cs**

RecvBuffer는 클라이언트에서 서버로 데이터를 수신할 때 사용하는 버퍼 클래스이다.
데이터를 읽고 쓰는 위치를 기록하며, 읽을 수 있는 데이터의 크기와 사용 가능한 버퍼 공간 크기를 계산.

```cs
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore {
    public class RecvBuffer {
        // [r][][w][][][][][][][]
        ArraySegment<byte> _buffer; // Segment로 만들 경우, 대용량 바이트 대응 가능
        // _readPos는 현재 버퍼에서 읽어들일 위치를 나타낸다.
        int _readPos;
        // _writePos는 현재 버퍼에서 쓰기를 할 위치를 나타낸다.
        int _writePos;

        // 생성자에서는 새로운 크기의 버퍼를 생성해준다.
        public RecvBuffer(int bufferSize) {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        // 지금까지 스여진 위치에서 읽혀진 위치를 빼서 현재 버퍼에서 읽어야 하는 데이터의 크기를 계산한다.
        // 버퍼 사용 범위
        public int DataSize => _writePos - _readPos;
        // _buffer의 크기에서 쓰여진 위치를 빼서 남은 사이즈를 구한다.
        // 버퍼 남은 공간
        public int FreeSize => _buffer.Count - _writePos;

        // 현재 읽을 수 있는 데이터를 가져온다.
        // 버퍼에 쌓인 데이터의 실제 크기만큼
        public ArraySegment<byte> ReadSegment => new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); 
        // 현재 쓸 수 있는 위치부터 끝까지 남은 공간을 가져온다.
        // 사용가능한 버퍼의 빈공간
        public ArraySegment<byte> WriteSegment => new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize);

        // 현재 버퍼에 남아있는 데이터를 정리
        public void Clean() {
            // 읽어야 하는 데이터의 사이즈
            int dataSize = DataSize;
            if (dataSize == 0) {
                // 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
                _readPos = _writePos = 0;
            }
            else {
                // 남은 찌끄레기가 있으면 시작 위치로 복사
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }
        
        // 데이터를 읽을 수 있는지
        public bool OnRead(int numOfBytes) {
            // 읽을 데이터가 DataSize보다 크면 false
            if (numOfBytes > DataSize)
                return false;

            // 아니면 읽을 위치에 numOfBytes를 더하고
            _readPos += numOfBytes;
            return true;
        }

        // numOfBytes만큼의 크기를 버퍼에 쓸 수 있는지
        public bool OnWrite(int numOfBytes) {
            // 작성할 데이터가 버퍼의 남은 공간보다 크면 false
            if (numOfBytes > FreeSize)
                return false;

            // 아니면 쓸 위치에 쓸만큼을 더하고
            _writePos += numOfBytes;
            return true;
        }
    }
}

```