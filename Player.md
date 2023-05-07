**Player.cs**

Player는 플레이어 고유 아이디를 저장하여 플레이어끼리 구분하기 위해 사용

```cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    // PlayerId를 통해 Player의 고유 아이디를 지정한다.
    public int PlayerId { get; set; }
}
```