using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public string handName; //맨손이나 너클을 구분.
    public float range; //공격범위
    public int damage; //공격력
    public float workSpeed; //작업속도.
    public float attackDelay; //클릭시에 공격 딜레이
    public float attackDelayA; //공격 활성화 시점.
    public float attackDelayB; //공격 비활성화 시점.


    public Animator anim; //애니메이터 컨트롤러를 넣어줄 것임.

}
