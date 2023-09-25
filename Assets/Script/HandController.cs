using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class HandController : MonoBehaviour
{
    //현재 장착된 hand 형 타입 무기.
    [SerializeField]
    private Hand currentHand;

    //공격중
    private bool isAttack = false;
    private bool isSwing = false; //팔을휘두르고 있는지아닌지

    private RaycastHit hitInfo; //레이저 닿은 것의 정보


   

    // Update is called once per frame
    void Update()
    {
        TryAttack();
    }

    private void TryAttack()
    {
        if (Input.GetButton("Fire1")) //좌클릭 총알 발사
        {
            if(!isAttack)
            {
                //코루틴 실행.
                StartCoroutine(AttackCoroutine());
            }
        }
    }
    
    IEnumerator AttackCoroutine()
    {
        isAttack=true;
        currentHand.anim.SetTrigger("Attack"); //currentHand에 있는 애니메이션의 상태변수 트리거를 attack을 발동
        
        yield return new WaitForSeconds(currentHand.attackDelayA);
        isSwing = true;

        //적중여부를 판별하는 코루틴 시작
        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(currentHand.attackDelayB);
        isSwing = false; //팔을 접었으니까

        yield return new WaitForSeconds(currentHand.attackDelay -  currentHand.attackDelayA - currentHand.attackDelayB);
        isAttack=false; //재공격이 이루어질 수 있도록
    }

    IEnumerator HitCoroutine()
    {
        while (isSwing)
        {
            if (CheckObject())
            {
                isSwing = false; //한번 적중하면 코루틴 끝.
                //충돌했음.
                Debug.Log(hitInfo.transform.name);
            }
            yield return null;
 
        }
    }

    private bool CheckObject()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, currentHand.range)) {
            // transform.forward == transform.TransformDirection(Vector3)
            return true;
        }
        return false;
    }
}
