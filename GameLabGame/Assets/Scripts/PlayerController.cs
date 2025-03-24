using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        PAUSED, PLAYING, PHOTO
    }

    public PlayerState State;
    public static PlayerController Instance;
    [Header("Player Settings")] 
    public float height = 2f;
    public float crouchHeight = 1f;
    public float radius = .3f;
    [Range(0,1)]
    public float underpass = .5f;
    [Range(0, .25f)]
    public float groundGrip = .15f;
    public float verticalAdjustStr = 175f;
    [Range(0, 1)] 
    public float verticalAdjustImmediate = .5f;
    [Header("Movement Settings")]
    public float moveSpeed = 5;
    [Range(1, 2)] public float runSpeedMult = 1.5f;
    [Range(0, 1)] public float crouchSpeedMult = .5f;
    public float gravity = 9.8f;
    public float jumpForce = 10f;
    [Range(0, 1)] public float groundControl= .95f;
    [Range(0, 1)] public float airControl= .25f;

    [Header("Camera Settings")]
    public float lookSpeed = 1;
    public float lookDirectionalClamp = 180;

    [Header("Layermasks")] 
    public LayerMask groundMask;
    [Header("Components")]
    public CinemachineVirtualCamera _camera;
    public Transform _camerashiftpoint;
    private Animator _animator;
    private CapsuleCollider _collider;
    private Rigidbody _rigidbody;

    private Vector2 _moveInputs;
    private Vector2 _lookInputs;
    private Boolean _jumpPressed;
    private Boolean _sprintHeld;
    private Boolean _crouchHeld;
    
    private float _xRotation = 0f;

    private bool _grounded;
    private bool _crouching;

    private float _timeFalling = 0f;
    // Start is called before the first frame update
    void Start() { Setup(); }
    private void OnValidate() { Setup(); }
    private void Setup()
    {
        if(Instance != null && Instance != this)
            Destroy(this);
        Instance = this;
        
        Cursor.lockState = CursorLockMode.Locked;
        
        _collider = GetComponent<CapsuleCollider>();
        _collider.radius = radius;
        _collider.height = crouchHeight * (1-underpass);
        _collider.center = new Vector3(0f, crouchHeight * underpass * .5f, 0f); 
        
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

    }

    // Update only gets inputs- everything else should be done in fixedupdate
    void Update()
    {

        if (State == PlayerState.PAUSED) return;
        
        _moveInputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
        _lookInputs = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        
        if (Input.GetKeyDown(KeyCode.Space)) { _jumpPressed = true; }

        _sprintHeld = Input.GetKey(KeyCode.LeftShift);
        _crouchHeld = Input.GetKey(KeyCode.LeftControl);

        State = Input.GetMouseButton(1) ? PlayerState.PHOTO : PlayerState.PLAYING;
        
        _animator.SetBool("InCamera", State == PlayerState.PHOTO);
        float speed = _moveInputs.magnitude * (_sprintHeld ? 2 : 1) * (_crouchHeld ? .5f : 1); 
        _animator.SetFloat("Speed", speed);

    }
    private void FixedUpdate()
    {
        MoveCamera();
        
        CrouchLogic();

        Vector3 playerMovements = GetPlayerMovements();

        Vector3 verticalVelocity = GetVerticalVelocity();

        _rigidbody.velocity = new Vector3(playerMovements.x, _rigidbody.velocity.y, playerMovements.z);
        _rigidbody.velocity += verticalVelocity;
    }
    
    private void MoveCamera()
    {
        //Camera Control
        float mouseX = _lookInputs.x * lookSpeed * Time.deltaTime;
        float mouseY = _lookInputs.y * lookSpeed * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -lookDirectionalClamp, lookDirectionalClamp);

        _camerashiftpoint.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        this.transform.Rotate(Vector3.up * mouseX);

    }
    
    private Vector3 GetPlayerMovements()
    {
        Vector3 relativeInputs = transform.forward * _moveInputs.y + transform.right * _moveInputs.x;
        relativeInputs = relativeInputs.normalized * (Mathf.Clamp(relativeInputs.magnitude, 0, 1) * EvaluatePlayerSpeed());

        Vector3 oldInputs = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);

        return Vector3.Lerp(oldInputs, relativeInputs, _grounded ? groundControl : airControl);
    }
    
    private Vector3 GetVerticalVelocity()
    {
        float verticalForces;
        
        Ray groundCheckRay = new Ray(transform.position, Vector3.down);

        if (Physics.SphereCast(groundCheckRay,groundGrip,  out RaycastHit hit, EvaluatePlayerHeight() * .5f, groundMask))
        {
            _grounded = true;
            _timeFalling = 0;
            verticalForces = -(_rigidbody.velocity.y * verticalAdjustImmediate) + EvaluateFloorGripStr(hit.point.y, transform.position.y - EvaluatePlayerHeight() * .5f);
        }
        else
        {
            _grounded = false;
            _timeFalling += Time.deltaTime;
            verticalForces = -gravity * _timeFalling;
        }

        if (_jumpPressed) verticalForces += JumpLogic();
      
        
        return Vector3.up * verticalForces;
    }
    
    private float JumpLogic()
    {
        _jumpPressed = false;
        
        if (!_grounded) return 0;
        this.transform.position += Vector3.up * groundGrip;
        return jumpForce;
    }

    private void CrouchLogic()
    {
        if (_crouchHeld)
        {
            if (_crouching == false)
            {
                _collider.height = crouchHeight * (1-underpass);
                _collider.center = new Vector3(0f, crouchHeight * underpass * .5f, 0f); 
            }
            _crouching = true;
        }
        else
        {
            if (_crouching)
            {
                //Check above the player's head to make sure uncrouching doesn't mess them up.
                Ray crouchRay = new Ray(this.transform.position + Vector3.up * crouchHeight * .5f, Vector3.up);
                if (!Physics.SphereCast(crouchRay, radius, (height - crouchHeight - radius),  groundMask))
                {
                    _collider.height = height * (1-underpass);
                    _collider.center = new Vector3(0f, height * underpass * .5f, 0f); 
                    _crouching = false;
                }
            }
        }
    }
    
    private float EvaluatePlayerSpeed()
    {
        float speed = moveSpeed;
        if (_sprintHeld) speed *= runSpeedMult;
        if (_crouching) speed *= crouchSpeedMult;
        
        return speed;
    }
    
    private float EvaluatePlayerHeight()
    {
        if (_crouching) return crouchHeight;
        return height;
    }
    
    private float EvaluateFloorGripStr(float hitPoint, float destPoint)
    {
        float offsetCurve = Mathf.Clamp(hitPoint - destPoint, -groundGrip, groundGrip) * groundGrip;
        return offsetCurve * verticalAdjustStr;
    }
    
    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position - Vector3.up * height * (.5f - underpass), transform.position - Vector3.up * EvaluatePlayerHeight() * .5f);
        Debug.DrawLine(transform.position - Vector3.up * height * .5f, transform.position - Vector3.up * (EvaluatePlayerHeight() * .5f + groundGrip), Color.magenta);
    }
}
