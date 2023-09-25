using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class PlayerController : MonoBehaviour
{
    //스피드 조정 변수
    [SerializeField] //프라이빗에 시리얼라이즈필드를 넣어주면 보호수준은 유지되면서 인스펙터 창에서 수정할 수 있음. 그래서 사용.
    private float walkSpeed; //이 스크립트 내에서만 사용할 수 있게 프라이빗으로 설정.
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;

    private float applySpeed;

    [SerializeField]
    private float jumpForce;

    //상태 변수
    private bool isRun = false;
    private bool isCrouch = false;
    private bool isGround = true;

    //앉았을 때 얼마나 앉을지 결정하는 변수.
    [SerializeField]
    private float crouchPosY;
    private float originPosY; //원래대로 돌아오려면 원래 값이 있어야함.
    private float applyCrouchPosY;


    //땅 착지 여부
    private CapsuleCollider capsuleCollider;


    //민감도
    [SerializeField]
    private float lookSensitivity; //카메라의 민감도를 조절할 수 있도록 선언

    //카메라 한계
    [SerializeField]
    private float cameraRotationLimit; //고개를 들때 어느각도까지만 올라가고 내려가도록.
    private float currentCameraRotationX = 0;//정면을 바라보도록 0으로 설정. 만약 45이면 45도 각도로


    //필요한 컴포넌트
    [SerializeField]
    private Camera theCamera;

    private Rigidbody myRigid; //실제 플레이어의 육체적인 바디.
                               //캡슐이 겉만 보이기 때문에 colider로 충돌 영역을 설정하고, rigidbody에는 colider에 물리학을 입힘.
    


    // Start is called before the first frame update
    //스크립트가 처음 실행될 때 실행
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();

        //theCamera = FindObjectOfType<Camera>(); //상위 모든 객체를 뒤져서 카메라 컴포넌트를 넣어줌.

        myRigid = GetComponent<Rigidbody>(); //rigidbody컴포넌트를 myRigid 변수에 넣겠다는 것.
        //시리얼라이즈필드를 넣어도 되나, 시리얼라이즈필드를 넣는다고 무조건 인스펙터 창에 뜨지는 않음. 예외도 존재함.
        //유니티에서는 이 방식을 더 추천함 빠르기 때문.

        applySpeed = walkSpeed;

        //초기화
        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY; //기본 서 있는 상태
    }

    // Update is called once per frame
    // 1초에 대략 60번 실행, 움직임이 실시간으로 이루어질 수 있도록
    void Update()
    {
        IsGround();
        TryJump();
        TryRun();//반드시 move함수 위에 있어야함.
        TryCrouch();
        Move();
        CameraLotation();
        CharacterRotation();
    }
    //앉기 시도
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }
    //앉기 동작
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
    //부드러운 동작 실행
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while (_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.1f); //보간 함수.
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15)
                break;
            yield return null;
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
        //yield return new WaitForSeconds(1f); //코루틴의 장점. 병렬처리를 위해 사용함. 빠르게 왔다갔다 하는 식으로 병렬처리를 지원함.
        //부드럽게 카메라 이동을 처리함.
    }
    //지면 체크
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        //extents는 캡슐 바운즈의 반절. y값의 반절임 y값만큼 거리를 주고 레이저를 쏨. 계단 대각선 오차를 고려해서 0.1f더해줌
    }
    //점프 시도
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
            Jump();
    }

    private void Jump()
    {
        //점프했을 떄 앉기를 캔슬시키고 싶으면
        //앉은 상태에서 점프시 앉은 상태 해제
        if (isCrouch)
        {
            Crouch();
        }
        myRigid.velocity = transform.up * jumpForce;
    }
    //달리기 시도
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
    //달리기 실행
    private void Running()
    {
        if (isCrouch)
            Crouch();

        isRun = true;
        applySpeed = runSpeed;
    }
    //달리기 취소
    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;

    }

    //움직임 실행
    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal"); //ad, 좌우 등 1과 -1이 리턴되면서 변수에 들어감.
        float _moveDirZ = Input.GetAxisRaw("Vertical"); //Z가 정면과 뒤. Vertical.

        Vector3 _moveHorizontal = transform.right * _moveDirX; //transform은 기본 컴포넌트가 가지고 있는 속성값의 right를 쓰겠다.
        //(1, 0, 0) 에 moveDirX를 곱해줌. 오른쪽 왼쪽 구분 가능.

        Vector3 _moveVertical = transform.forward * _moveDirZ; //(0,0,1)에 moveDirZ를 곱해줌. 위 아래를 구분할 수 있게 됨.

        //두개의 벡터값을 합쳐주겠다.
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;
        //(1, 0, 0) + (0, 0, 1) = (1, 0, 1) = 2
        //여기서 normalized를 하면 (0.5, 0. 0.5)가 되면서 합이 1이 됨. 내부 알고리즘적으로 빨라짐. 계산하기 편하게 값을 1로.
        //얼마나빠른 속도로 갈것인지 walkspeed 곱해줌.

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime); //Time.deltaTime은 1초동안 _velocity만큼 움직이게끔하여 순간이동하는게 아니라 쭉 움직이는 형태로.

    }
    //좌우 캐릭터 회전 직접 좌우로 움직이는
    private void CharacterRotation()
    {
        
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY)); //쿼터니온
    }
    //상하 카메라 회전
    private void CameraLotation()
    {
        
        float _xRotation = Input.GetAxisRaw("Mouse Y"); //마우스는 3차원이 아니라 2차원임. 위아래로 고개를 드는 것
        float _cameraRotationX = _xRotation * lookSensitivity; //마우스를 올렸다고 확 올라가면 안되니까 sensitivity를 이용해서
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);//currentCameraRotationX값이 -45도와 45사이에 고정되도록
        //만약 60이면 45로 limit

        //Debug.Log(myRigid.rotation);
        //Debug.Log(myRigid.rotation.eulerAngles);
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f); //마우스가 위아래로 움직이는데 좌우로 움직이면 안되니까 0으로 고정

    }
}
