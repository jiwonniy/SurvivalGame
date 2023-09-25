using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class PlayerController : MonoBehaviour
{
    //���ǵ� ���� ����
    [SerializeField] //�����̺��� �ø���������ʵ带 �־��ָ� ��ȣ������ �����Ǹ鼭 �ν����� â���� ������ �� ����. �׷��� ���.
    private float walkSpeed; //�� ��ũ��Ʈ �������� ����� �� �ְ� �����̺����� ����.
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;

    private float applySpeed;

    [SerializeField]
    private float jumpForce;

    //���� ����
    private bool isRun = false;
    private bool isCrouch = false;
    private bool isGround = true;

    //�ɾ��� �� �󸶳� ������ �����ϴ� ����.
    [SerializeField]
    private float crouchPosY;
    private float originPosY; //������� ���ƿ����� ���� ���� �־����.
    private float applyCrouchPosY;


    //�� ���� ����
    private CapsuleCollider capsuleCollider;


    //�ΰ���
    [SerializeField]
    private float lookSensitivity; //ī�޶��� �ΰ����� ������ �� �ֵ��� ����

    //ī�޶� �Ѱ�
    [SerializeField]
    private float cameraRotationLimit; //���� �鶧 ������������� �ö󰡰� ����������.
    private float currentCameraRotationX = 0;//������ �ٶ󺸵��� 0���� ����. ���� 45�̸� 45�� ������


    //�ʿ��� ������Ʈ
    [SerializeField]
    private Camera theCamera;

    private Rigidbody myRigid; //���� �÷��̾��� ��ü���� �ٵ�.
                               //ĸ���� �Ѹ� ���̱� ������ colider�� �浹 ������ �����ϰ�, rigidbody���� colider�� �������� ����.
    


    // Start is called before the first frame update
    //��ũ��Ʈ�� ó�� ����� �� ����
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();

        //theCamera = FindObjectOfType<Camera>(); //���� ��� ��ü�� ������ ī�޶� ������Ʈ�� �־���.

        myRigid = GetComponent<Rigidbody>(); //rigidbody������Ʈ�� myRigid ������ �ְڴٴ� ��.
        //�ø���������ʵ带 �־ �ǳ�, �ø���������ʵ带 �ִ´ٰ� ������ �ν����� â�� ������ ����. ���ܵ� ������.
        //����Ƽ������ �� ����� �� ��õ�� ������ ����.

        applySpeed = walkSpeed;

        //�ʱ�ȭ
        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY; //�⺻ �� �ִ� ����
    }

    // Update is called once per frame
    // 1�ʿ� �뷫 60�� ����, �������� �ǽð����� �̷���� �� �ֵ���
    void Update()
    {
        IsGround();
        TryJump();
        TryRun();//�ݵ�� move�Լ� ���� �־����.
        TryCrouch();
        Move();
        CameraLotation();
        CharacterRotation();
    }
    //�ɱ� �õ�
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }
    //�ɱ� ����
    private void Crouch(){
        isCrouch = !isCrouch;
        if (isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }
        //theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
        StartCoroutine(CrouchCoroutine());
    }
    //�ε巯�� ���� ����
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while (_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.1f); //���� �Լ�.
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15)
                break;
            yield return null;
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
        //yield return new WaitForSeconds(1f); //�ڷ�ƾ�� ����. ����ó���� ���� �����. ������ �Դٰ��� �ϴ� ������ ����ó���� ������.
        //�ε巴�� ī�޶� �̵��� ó����.
    }
    //���� üũ
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        //extents�� ĸ�� �ٿ����� ����. y���� ������ y����ŭ �Ÿ��� �ְ� �������� ��. ��� �밢�� ������ ����ؼ� 0.1f������
    }
    //���� �õ�
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
            Jump();
    }

    private void Jump()
    {
        //�������� �� �ɱ⸦ ĵ����Ű�� ������
        //���� ���¿��� ������ ���� ���� ����
        if (isCrouch)
        {
            Crouch();
        }
        myRigid.velocity = transform.up * jumpForce;
    }
    //�޸��� �õ�
    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.RightShift))
        {
            RunningCancel();
        }
    }
    //�޸��� ����
    private void Running()
    {
        if (isCrouch)
            Crouch();

        isRun = true;
        applySpeed = runSpeed;
    }
    //�޸��� ���
    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;

    }

    //������ ����
    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal"); //ad, �¿� �� 1�� -1�� ���ϵǸ鼭 ������ ��.
        float _moveDirZ = Input.GetAxisRaw("Vertical"); //Z�� ����� ��. Vertical.

        Vector3 _moveHorizontal = transform.right * _moveDirX; //transform�� �⺻ ������Ʈ�� ������ �ִ� �Ӽ����� right�� ���ڴ�.
        //(1, 0, 0) �� moveDirX�� ������. ������ ���� ���� ����.

        Vector3 _moveVertical = transform.forward * _moveDirZ; //(0,0,1)�� moveDirZ�� ������. �� �Ʒ��� ������ �� �ְ� ��.

        //�ΰ��� ���Ͱ��� �����ְڴ�.
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;
        //(1, 0, 0) + (0, 0, 1) = (1, 0, 1) = 2
        //���⼭ normalized�� �ϸ� (0.5, 0. 0.5)�� �Ǹ鼭 ���� 1�� ��. ���� �˰��������� ������. ����ϱ� ���ϰ� ���� 1��.
        //�󸶳����� �ӵ��� �������� walkspeed ������.

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime); //Time.deltaTime�� 1�ʵ��� _velocity��ŭ �����̰Բ��Ͽ� �����̵��ϴ°� �ƴ϶� �� �����̴� ���·�.

    }
    //�¿� ĳ���� ȸ�� ���� �¿�� �����̴�
    private void CharacterRotation()
    {
        
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY)); //���ʹϿ�
    }
    //���� ī�޶� ȸ��
    private void CameraLotation()
    {
        
        float _xRotation = Input.GetAxisRaw("Mouse Y"); //���콺�� 3������ �ƴ϶� 2������. ���Ʒ��� ���� ��� ��
        float _cameraRotationX = _xRotation * lookSensitivity; //���콺�� �÷ȴٰ� Ȯ �ö󰡸� �ȵǴϱ� sensitivity�� �̿��ؼ�
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);//currentCameraRotationX���� -45���� 45���̿� �����ǵ���
        //���� 60�̸� 45�� limit

        //Debug.Log(myRigid.rotation);
        //Debug.Log(myRigid.rotation.eulerAngles);
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f); //���콺�� ���Ʒ��� �����̴µ� �¿�� �����̸� �ȵǴϱ� 0���� ����

    }
}
