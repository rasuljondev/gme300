using System;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _speed = 5f;
        [SerializeField] private Animator _propAnim;
        [SerializeField] private CinemachineVirtualCamera _droneCam;
        [SerializeField] private InteractableZone _interactableZone;

        private bool _inFlightMode = false;
        private PlayerInputActions _input;
        private Vector2 _moveInput;
        private bool _thrustUp, _thrustDown, _rotateLeft, _rotateRight, _tiltFwd, _tiltBack;

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        private void Awake()
        {
            _input = new PlayerInputActions();
        }

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
            _input.Drone.Enable();

            _input.Drone.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _input.Drone.Move.canceled += _ => _moveInput = Vector2.zero;

            _input.Drone.ThrustUp.performed += _ => _thrustUp = true;
            _input.Drone.ThrustUp.canceled += _ => _thrustUp = false;

            _input.Drone.ThrustDown.performed += _ => _thrustDown = true;
            _input.Drone.ThrustDown.canceled += _ => _thrustDown = false;

            _input.Drone.RotateLeft.performed += _ => _rotateLeft = true;
            _input.Drone.RotateLeft.canceled += _ => _rotateLeft = false;

            _input.Drone.RotateRight.performed += _ => _rotateRight = true;
            _input.Drone.RotateRight.canceled += _ => _rotateRight = false;

            _input.Drone.TiltForward.performed += _ => _tiltFwd = true;
            _input.Drone.TiltForward.canceled += _ => _tiltFwd = false;

            _input.Drone.TiltBackward.performed += _ => _tiltBack = true;
            _input.Drone.TiltBackward.canceled += _ => _tiltBack = false;

            _input.Drone.ExitFlight.performed += _ =>
            {
                _inFlightMode = false;
                onExitFlightmode?.Invoke();
                ExitFlightMode();
            };
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
            _input.Drone.Disable();
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            if (!_inFlightMode && zone.GetZoneID() == 4)
            {
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);
        }

        private void Update()
        {
            if (_inFlightMode)
            {
                CalculateTilt();
                CalculateRotation();
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * 9.81f, ForceMode.Acceleration);

            if (_inFlightMode)
            {
                CalculateMovement();
            }
        }

        private void CalculateRotation()
        {
            if (_rotateLeft)
            {
                var rot = transform.localRotation.eulerAngles;
                rot.y -= _speed / 3;
                transform.localRotation = Quaternion.Euler(rot);
            }
            if (_rotateRight)
            {
                var rot = transform.localRotation.eulerAngles;
                rot.y += _speed / 3;
                transform.localRotation = Quaternion.Euler(rot);
            }
        }

        private void CalculateMovement()
        {
            if (_thrustUp)
            {
                _rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration);
            }

            if (_thrustDown)
            {
                _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);
            }
        }

        private void CalculateTilt()
        {
            if (_moveInput.x < 0)
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 30);
            else if (_moveInput.x > 0)
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, -30);
            else if (_moveInput.y > 0 || _tiltFwd)
                transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
            else if (_moveInput.y < 0 || _tiltBack)
                transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
            else
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
        }

        // ------------------------- LEGACY INPUT CODE (COMMENTED OUT) -------------------------
        // private float _xInput;
        // private float _zInput;
        // private bool _thrusting;
        // private bool _thrustDown;
        // private Tilt _tilting;
        // private float _rotation;

        // private void Update()
        // {
        //     if (_inFlightMode)
        //     {
        //         _xInput = Input.GetAxis("Horizontal");
        //         _zInput = Input.GetAxis("Vertical");
        //         _thrusting = Input.GetKey(KeyCode.Space);
        //         _thrustDown = Input.GetKey(KeyCode.V);
        //         _tilting = Input.GetKey(KeyCode.T) ? Tilt.Forward : Input.GetKey(KeyCode.G) ? Tilt.Back : Tilt.NoTilt;
        //         _rotation = Input.GetKey(KeyCode.Q) ? -1 : Input.GetKey(KeyCode.E) ? 1 : 0;
        //         if (Input.GetKeyDown(KeyCode.F))
        //         {
        //             _inFlightMode = false;
        //             onExitFlightmode?.Invoke();
        //             ExitFlightMode();
        //         }
        //     }
        // }

        // private void FixedUpdate()
        // {
        //     _rigidbody.AddForce(transform.up * 9.81f, ForceMode.Acceleration);
        //     if (!_inFlightMode) return;

        //     _rigidbody.AddForce(transform.forward * _zInput * _speed);
        //     _rigidbody.AddForce(transform.right * _xInput * _speed);
        //     if (_thrusting)
        //         _rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration);
        //     if (_thrustDown)
        //         _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);
        //     transform.Rotate(0, _rotation, 0);

        //     if (_tilting == Tilt.Forward)
        //         transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
        //     else if (_tilting == Tilt.Back)
        //         transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
        //     else
        //         transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
        // }
    }
}
