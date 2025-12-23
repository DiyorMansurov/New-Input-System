using System;
using System.Collections;
using System.Collections.Generic;
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

        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;
        
        private PlayerInputActions _input;
        

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;


        private void Awake() {
            _input = new PlayerInputActions();
        }
        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;

            _input.Drone.Enable();
            _input.Drone.Escape.performed += Escape_performed;
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                UIManager.Instance.DisplayDroneTutorial(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {            
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);      
            UIManager.Instance.DisplayDroneTutorial(false);
                  
        }

        private void Update()
        {
            if (_inFlightMode)
            {
                CalculateTilt();
                CalculateMovementUpdate();

            }
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }


        private void Escape_performed(InputAction.CallbackContext context)
        {
            if (_inFlightMode)
                {
                    _inFlightMode = false;
                    onExitFlightmode?.Invoke();
                    ExitFlightMode();
                }
        }
//---------------------------------------------------------
        // private void CalculateMovementUpdate()
        // {
        //     if (Input.GetKey(KeyCode.LeftArrow))
        //     {
        //         var tempRot = transform.localRotation.eulerAngles;
        //         tempRot.y -= _speed / 3;
        //         transform.localRotation = Quaternion.Euler(tempRot);
        //     }
        //     if (Input.GetKey(KeyCode.RightArrow))
        //     {
        //         var tempRot = transform.localRotation.eulerAngles;
        //         tempRot.y += _speed / 3;
        //         transform.localRotation = Quaternion.Euler(tempRot);
        //     }
        // }

        private void CalculateMovementUpdate()
        {
            float rotationInput = _input.Drone.Rotation.ReadValue<float>();
            var tempRot = transform.localRotation.eulerAngles;
            tempRot.y += rotationInput * (_speed / 3);
            transform.localRotation = Quaternion.Euler(tempRot);
        }
//---------------------------------------------------------

//---------------------------------------------------------
        // private void CalculateMovementFixedUpdate()
        // {
            
        //     if (Input.GetKey(KeyCode.Space))
        //     {
        //         _rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration);
        //     }
        //     if (Input.GetKey(KeyCode.V))
        //     {
        //         _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);
        //     }
        // }

        private void CalculateMovementFixedUpdate()
        {
            float heightInput = _input.Drone.Height.ReadValue<float>();

            _rigidbody.AddForce(transform.up * heightInput * _speed, ForceMode.Acceleration);
        }

//---------------------------------------------------------

//---------------------------------------------------------

        // private void CalculateTilt()
        // {
        //     if (Input.GetKey(KeyCode.A)) 
        //         transform.rotation = Quaternion.Euler(00, transform.localRotation.eulerAngles.y, 30);
        //     else if (Input.GetKey(KeyCode.D))
        //         transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, -30);
        //     else if (Input.GetKey(KeyCode.W))
        //         transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
        //     else if (Input.GetKey(KeyCode.S))
        //         transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
        //     else 
        //         transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
        // }

        private void CalculateTilt()
        {
            Vector2 movementInput = _input.Drone.Movement.ReadValue<Vector2>();

            transform.rotation = Quaternion.Euler(movementInput.y * 30, transform.localRotation.eulerAngles.y, -movementInput.x * 30);
        }

//---------------------------------------------------------

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;

            _input.Drone.Disable();
            _input.Drone.Escape.performed -= Escape_performed;
        }
    }
}
