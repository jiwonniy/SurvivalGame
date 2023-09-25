using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class HandController : MonoBehaviour
{
    //���� ������ hand �� Ÿ�� ����.
    [SerializeField]
    private Hand currentHand;

    //������
    private bool isAttack = false;
    private bool isSwing = false; //�����ֵθ��� �ִ����ƴ���

    private RaycastHit hitInfo; //������ ���� ���� ����


   

    // Update is called once per frame
    void Update()
    {
        TryAttack();
    }

    private void TryAttack()
    {
        if (Input.GetButton("Fire1")) //��Ŭ�� �Ѿ� �߻�
        {
            if(!isAttack)
            {
                //�ڷ�ƾ ����.
                StartCoroutine(AttackCoroutine());
            }
        }
    }
    
    IEnumerator AttackCoroutine()
    {
        isAttack=true;
        currentHand.anim.SetTrigger("Attack"); //currentHand�� �ִ� �ִϸ��̼��� ���º��� Ʈ���Ÿ� attack�� �ߵ�
        
        yield return new WaitForSeconds(currentHand.attackDelayA);
        isSwing = true;

        //���߿��θ� �Ǻ��ϴ� �ڷ�ƾ ����
        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(currentHand.attackDelayB);
        isSwing = false; //���� �������ϱ�

        yield return new WaitForSeconds(currentHand.attackDelay -  currentHand.attackDelayA - currentHand.attackDelayB);
        isAttack=false; //������� �̷���� �� �ֵ���
    }

    IEnumerator HitCoroutine()
    {
        while (isSwing)
        {
            if (CheckObject())
            {
                isSwing = false; //�ѹ� �����ϸ� �ڷ�ƾ ��.
                //�浹����.
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
