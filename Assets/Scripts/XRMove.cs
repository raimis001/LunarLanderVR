using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class XRMove : MonoBehaviour
{

    public float moveSpeed = 5f;
    public float rotateAngle = 15;
    public float rotateDelay = 0.05f;

    public InputAction rightJoystick;
    public InputAction leftJoystick;

    public Transform bodyCollider;


    Rigidbody body;
    Vector3 moveDelta = Vector3.zero;
    Vector2 rotateDelta;
    Transform mainCamera;
    bool isRotating;

    private void OnEnable()
    {
        rightJoystick.Enable();
        leftJoystick.Enable();
    }

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        mainCamera = Camera.main.transform;
    }

    private void Update()
    {

        if (LunarModule.moduleStatus != ModuleStatus.normal)
            return;

        moveDelta.x = leftJoystick.ReadValue<Vector2>().x;
        moveDelta.z = leftJoystick.ReadValue<Vector2>().y;

        transform.Translate(Quaternion.AngleAxis(mainCamera.eulerAngles.y, Vector3.up) * moveDelta * moveSpeed * Time.deltaTime, Space.World);

        //TODO rotate
        if (isRotating)
            return;

        rotateDelta = rightJoystick.ReadValue<Vector2>();
        if (Mathf.Abs(rotateDelta.x) < 0.3f)
            return;

        StartCoroutine(IRodate());
    }

    private void FixedUpdate()
    {
        //body.velocity = Quaternion.AngleAxis(mainCamera.eulerAngles.y, Vector3.up) * moveDelta * moveSpeed;
    }

    private void LateUpdate()
    {
        bodyCollider.position = new Vector3(mainCamera.position.x, bodyCollider.position.y, mainCamera.position.z);
    }

    IEnumerator IRodate()
    {
        isRotating = true;
        
        transform.Rotate(0, rotateAngle * Mathf.Sign(rotateDelta.x), 0, Space.Self);
        
        yield return new WaitForSeconds(rotateDelay);
        isRotating = false;
    }

}
