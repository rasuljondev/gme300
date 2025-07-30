using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.LiveObjects;
using Cinemachine;
using UnityEngine.InputSystem;

namespace Game.Scripts.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        private CharacterController _controller;
        private Animator _anim;

        [SerializeField] private float _speed = 5.0f;
        private bool _playerGrounded;

        [SerializeField] private Detonator _detonator;
        private bool _canMove = true;

        [SerializeField] private CinemachineVirtualCamera _followCam;
        [SerializeField] private GameObject _model;

        private PlayerInputActions _inputActions;
        private Vector2 _moveInput;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
            _inputActions.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Move.canceled += ctx => _moveInput = Vector2.zero;

            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
            Laptop.onHackComplete += ReleasePlayerControl;
            Laptop.onHackEnded += ReturnPlayerControl;
            Forklift.onDriveModeEntered += ReleasePlayerControl;
            Forklift.onDriveModeExited += ReturnPlayerControl;
            Forklift.onDriveModeEntered += HidePlayer;
            Drone.OnEnterFlightMode += ReleasePlayerControl;
            Drone.onExitFlightmode += ReturnPlayerControl;
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            if (_controller == null)
                Debug.LogError("No Character Controller Present");

            _anim = GetComponentInChildren<Animator>();
            if (_anim == null)
                Debug.Log("Failed to connect the Animator");
        }

        private void Update()
        {
            if (_canMove)
                CalculateMovement();
        }

        private void CalculateMovement()
        {
            _playerGrounded = _controller.isGrounded;

            // Legacy input (disabled)
            // float h = Input.GetAxisRaw("Horizontal");
            // float v = Input.GetAxisRaw("Vertical");

            float h = _moveInput.x;
            float v = _moveInput.y;

            transform.Rotate(transform.up, h);

            Vector3 direction = transform.forward * v;
            Vector3 velocity = direction * _speed;

            _anim.SetFloat("Speed", Mathf.Abs(velocity.magnitude));

            if (_playerGrounded)
                velocity.y = 0f;
            else
                velocity.y += -20f * Time.deltaTime;

            _controller.Move(velocity * Time.deltaTime);
        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            switch (zone.GetZoneID())
            {
                case 1:
                    _detonator.Show(); break;
                case 2:
                    TriggerExplosive(); break;
            }
        }

        private void ReleasePlayerControl()
        {
            // _canMove = false;
            // _followCam.Priority = 9;

            _inputActions.Player.Disable(); // Disables input map
            _canMove = false; // Disables movement logic
            _followCam.Priority = 9; // Lower priority camera
        }

        private void ReturnPlayerControl()
        {
            _model.SetActive(true);

            // _canMove = true;
            // _followCam.Priority = 10;

            _inputActions.Player.Enable(); // Re-enables input map
            _canMove = true;
            _followCam.Priority = 10;
        }

        private void HidePlayer()
        {
            _model.SetActive(false);
        }

        private void TriggerExplosive()
        {
            _detonator.TriggerExplosion();
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();

            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;
            Laptop.onHackComplete -= ReleasePlayerControl;
            Laptop.onHackEnded -= ReturnPlayerControl;
            Forklift.onDriveModeEntered -= ReleasePlayerControl;
            Forklift.onDriveModeExited -= ReturnPlayerControl;
            Forklift.onDriveModeEntered -= HidePlayer;
            Drone.OnEnterFlightMode -= ReleasePlayerControl;
            Drone.onExitFlightmode -= ReturnPlayerControl;
        }
    }
}
