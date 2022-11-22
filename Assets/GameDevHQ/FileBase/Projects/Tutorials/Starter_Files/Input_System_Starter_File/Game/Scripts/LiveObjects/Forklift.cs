using System;
using UnityEngine;
using Cinemachine;

namespace Game.Scripts.LiveObjects
{
    public class Forklift : MonoBehaviour
    {
        private int _liftDirection = 0;

        private InputActions _input;

        //private BoxCollider _collider;
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

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterDriveMode;
        }
        private void Start()
        {
            _input = new InputActions();
            _input.ForkLift.Enable();
            _input.ForkLift.Lift.started += Lift_started;
            _input.ForkLift.Lift.canceled += canceled;
            _input.ForkLift.Lower.started += Lower_started;
            _input.ForkLift.Lower.canceled += canceled;
            _input.ForkLift.Leave.performed += Leave_performed;

           
           
        }

        private void Leave_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ExitDriveMode();
        }

        private void canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            _liftDirection = 0;
        }

        private void Lower_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            _liftDirection = -1;
        }

        private void Lift_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            _liftDirection = 1;
        }

        private void EnterDriveMode(InteractableZone zone)
        {
            if (_inDriveMode !=true && zone.GetZoneID() == 5) //Enter ForkLift
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
                //if (Input.GetKeyDown(KeyCode.Escape))
                //    ExitDriveMode();
            }

        }

        private void CalcutateMovement()
        {
            

            //float h = Input.GetAxisRaw("Horizontal");
            //float v = Input.GetAxisRaw("Vertical");
           
            

            var move = _input.ForkLift.Drive.ReadValue<float>();
            transform.Translate(Vector3.forward * move * _speed * Time.deltaTime);

            
            var rotateDirection = _input.ForkLift.Rotate.ReadValue<float>();
            transform.Rotate(Vector3.up * Time.deltaTime * rotateDirection * _speed * 5);

            

            //transform.Translate(velocity * Time.deltaTime);

            //if (Mathf.Abs(v) > 0)
            //{
            //   var tempRot = transform.rotation.eulerAngles;
            //   tempRot.y += h * _speed / 2;
            //   transform.rotation = Quaternion.Euler(tempRot);
            //}
        }

        private void LiftControls()
        {



            if (_liftDirection > 0)
            {
                LiftUpRoutine();
            }
            else if (_liftDirection < 0)
            {
                LiftDownRoutine();
            }
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

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterDriveMode;
        }

    }
}