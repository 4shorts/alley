 using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private Vector2 _tiltDirection;
        private int _liftDirection = 0;
        private InputActions _input;

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
        

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        private void Start()
        {
            _input = new InputActions();
            _input.Drone.Enable();
            _input.Drone.Lift.performed += Lift_performed;
            _input.Drone.Lift.canceled += canceled;
            _input.Drone.Lower.performed += Lower_performed;
            _input.Drone.Lower.canceled += canceled;
            _input.Drone.Leave.performed += Leave_performed;
            
           
        }

        private void Leave_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            _inFlightMode = false;
            onExitFlightmode?.Invoke();
            ExitFlightMode();
        }

        private void Lower_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            _liftDirection = -1;
        }

        private void canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            _liftDirection = 0;
        }

        private void Lift_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            _liftDirection = 1;
        }


        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
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
                CalculateMovementUpdate();

                //if (Input.GetKeyDown(KeyCode.Escape))
                //{
                //    _inFlightMode = false;
                //    onExitFlightmode?.Invoke();
                //    ExitFlightMode();
                //}
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {
            var rotateDirection = _input.Drone.Rotate.ReadValue<float>();
            transform.Rotate(Vector3.up * Time.deltaTime * rotateDirection * _speed*10);

           

            

            //if (Input.GetKey(KeyCode.LeftArrow))
            //{
            //    var tempRot = transform.localRotation.eulerAngles;
            //    tempRot.y -= _speed / 3;
            //    transform.localRotation = Quaternion.Euler(tempRot);
            //}
            //if (Input.GetKey(KeyCode.RightArrow))
            //{
            //    var tempRot = transform.localRotation.eulerAngles;
            //    tempRot.y += _speed / 3;
            //    transform.localRotation = Quaternion.Euler(tempRot);
            //}
        }


        private void CalculateMovementFixedUpdate()
        {


            _rigidbody.AddForce(transform.up * _speed * _liftDirection, ForceMode.Acceleration);
           
        }
            
           


private void CalculateTilt()
        {
            _tiltDirection = _input.Drone.Tilt.ReadValue<Vector2>();



            if (_tiltDirection == Vector2.left)
                transform.rotation = Quaternion.Euler(00, transform.localRotation.eulerAngles.y, 30);
            else if (_tiltDirection == Vector2.right)
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, -30);
            else if (_tiltDirection == Vector2.up)
                transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
            else if (_tiltDirection == Vector2.down)
                transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
            else
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
        }
    }
}
