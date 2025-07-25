using System;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem; // <-- New Input System

namespace Game.Scripts.LiveObjects
{
    public class Forklift : MonoBehaviour
    {
        [SerializeField]
        private GameObject _lift, _steeringWheel, _leftWheel, _rightWheel, _rearWheels;
        [SerializeField]
        private Vector3 _liftLowerLimit, _liftUpperLimit;
        [SerializeField]
        private float _speed = 5f, _liftSpeed = 1f;
        [SerializeField]
        private CinemachineVirtualCamera _forkliftCam;
        [SerializeField]
        private GameObject _driverModel;
        private bool _inDriveMode = false;
        [SerializeField]
        private InteractableZone _interactableZone;

        public static event Action onDriveModeEntered;
        public static event Action onDriveModeExited;

        // New Input System
        private PlayerInputActions inputActions;
        private Vector2 moveInput;
        private bool liftUpPressed;
        private bool liftDownPressed;

        private void Awake()
        {
            inputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            inputActions.Forklift.Enable();
            inputActions.Forklift.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            inputActions.Forklift.Move.canceled += ctx => moveInput = Vector2.zero;
            inputActions.Forklift.LiftUp.performed += ctx => liftUpPressed = true;
            inputActions.Forklift.LiftUp.canceled += ctx => liftUpPressed = false;
            inputActions.Forklift.LiftDown.performed += ctx => liftDownPressed = true;
            inputActions.Forklift.LiftDown.canceled += ctx => liftDownPressed = false;

            InteractableZone.onZoneInteractionComplete += EnterDriveMode;
        }

        private void OnDisable()
        {
            inputActions.Forklift.Move.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
            inputActions.Forklift.Move.canceled -= ctx => moveInput = Vector2.zero;
            inputActions.Forklift.LiftUp.performed -= ctx => liftUpPressed = true;
            inputActions.Forklift.LiftUp.canceled -= ctx => liftUpPressed = false;
            inputActions.Forklift.LiftDown.performed -= ctx => liftDownPressed = true;
            inputActions.Forklift.LiftDown.canceled -= ctx => liftDownPressed = false;

            inputActions.Forklift.Disable();
            InteractableZone.onZoneInteractionComplete -= EnterDriveMode;
        }

        private void EnterDriveMode(InteractableZone zone)
        {
            if (_inDriveMode != true && zone.GetZoneID() == 5) //Enter ForkLift
            {
                _inDriveMode = true;
                _forkliftCam.Priority = 11;
                onDriveModeEntered?.Invoke();
                _driverModel.SetActive(true);
                _interactableZone.CompleteTask(5);
            }
        }

        private void ExitDriveMode()
        {
            _inDriveMode = false;
            _forkliftCam.Priority = 9;
            _driverModel.SetActive(false);
            onDriveModeExited?.Invoke();
        }

        private void Update()
        {
            if (_inDriveMode == true)
            {
                LiftControls();
                CalcutateMovement();
                // if (Input.GetKeyDown(KeyCode.Escape))
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                    ExitDriveMode();
            }
        }

        private void CalcutateMovement()
        {
            // float h = Input.GetAxisRaw("Horizontal");
            // float v = Input.GetAxisRaw("Vertical");
            float h = moveInput.x;
            float v = moveInput.y;

            var direction = new Vector3(0, 0, v);
            var velocity = direction * _speed;

            transform.Translate(velocity * Time.deltaTime);

            if (Mathf.Abs(v) > 0)
            {
                var tempRot = transform.rotation.eulerAngles;
                tempRot.y += h * _speed / 2;
                transform.rotation = Quaternion.Euler(tempRot);
            }
        }

        private void LiftControls()
        {
            // if (Input.GetKey(KeyCode.R))
            if (liftUpPressed)
                LiftUpRoutine();
            // else if (Input.GetKey(KeyCode.T))
            else if (liftDownPressed)
                LiftDownRoutine();
        }

        private void LiftUpRoutine()
        {
            if (_lift.transform.localPosition.y < _liftUpperLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y += Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y >= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftUpperLimit;
        }

        private void LiftDownRoutine()
        {
            if (_lift.transform.localPosition.y > _liftLowerLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y -= Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y <= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftLowerLimit;
        }
    }
}
