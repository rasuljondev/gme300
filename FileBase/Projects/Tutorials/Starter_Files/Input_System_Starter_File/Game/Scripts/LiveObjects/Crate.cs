using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions; // <-- Required for HoldInteraction

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;

        private bool _isReadyToBreak = false;
        private List<Rigidbody> _brakeOff = new List<Rigidbody>();

        private PlayerInputActions _inputActions;

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;

            // Setup input system
            if (_inputActions == null)
            {
                _inputActions = new PlayerInputActions();
                _inputActions.Push.Punch.performed += OnPushPerformed;
                _inputActions.Enable();
            }
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;

            if (_inputActions != null)
            {
                _inputActions.Push.Punch.performed -= OnPushPerformed;
                _inputActions.Disable();
            }
        }

        private void Start()
        {
            _brakeOff.AddRange(_pieces);
        }

        private void OnPushPerformed(InputAction.CallbackContext context)
        {
            if (!_isReadyToBreak || _interactableZone.GetZoneID() != 6)
                return;

            // Check if this is a hold interaction
            bool isHold = context.interaction is HoldInteraction;

            if (_brakeOff.Count > 0)
            {
                BreakPart(isHold);
                StartCoroutine(PunchDelay());
            }
            else
            {
                _isReadyToBreak = false;
                _crateCollider.enabled = false;
                _interactableZone.CompleteTask(6);
                Debug.Log("Completely Busted");
            }
        }

        public void BreakPart(bool strong)
        {
            int rng = Random.Range(0, _brakeOff.Count);
            _brakeOff[rng].constraints = RigidbodyConstraints.None;

            Vector3 force = strong ? new Vector3(3f, 3f, 3f) : new Vector3(1f, 1f, 1f);
            _brakeOff[rng].AddForce(force, ForceMode.Force);

            _brakeOff.Remove(_brakeOff[rng]);
        }

        IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(6);
        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            if (_isReadyToBreak == false && _brakeOff.Count > 0)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }

            // Action happens via new input now
        }

        /*
        // ---------------- OLD CODE ----------------
        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            if (_isReadyToBreak == false && _brakeOff.Count > 0)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }

            if (_isReadyToBreak && zone.GetZoneID() == 6) //Crate zone
            {
                if (_brakeOff.Count > 0)
                {
                    BreakPart();
                    StartCoroutine(PunchDelay());
                }
                else if (_brakeOff.Count == 0)
                {
                    _isReadyToBreak = false;
                    _crateCollider.enabled = false;
                    _interactableZone.CompleteTask(6);
                    Debug.Log("Completely Busted");
                }
            }
        }

        public void BreakPart()
        {
            int rng = Random.Range(0, _brakeOff.Count);
            _brakeOff[rng].constraints = RigidbodyConstraints.None;
            _brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
            _brakeOff.Remove(_brakeOff[rng]);
        }
        */
    }
}
