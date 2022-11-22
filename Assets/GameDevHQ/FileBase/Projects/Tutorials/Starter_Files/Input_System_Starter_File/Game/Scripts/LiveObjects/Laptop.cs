using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

namespace Game.Scripts.LiveObjects
{
    public class Laptop : MonoBehaviour
    {
        private InputActions _input;
        [SerializeField]
        private Slider _progressBar;
        [SerializeField]
        private int _hackTime = 5;
        private bool _hacked = false;
        [SerializeField]
        private CinemachineVirtualCamera[] _cameras;
        private int _activeCamera = 0;
        [SerializeField]
        private InteractableZone _interactableZone;

        public static event Action onHackComplete;
        public static event Action onHackEnded;

        private void OnEnable()
        {
            _input = new InputActions();
            
            InteractableZone.onHoldStarted += InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded += InteractableZone_onHoldEnded;
        }

        private void CameraSwitch_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

            var previous = _activeCamera;
            _activeCamera++;


            if (_activeCamera >= _cameras.Length)
                _activeCamera = 0;


            _cameras[_activeCamera].Priority = 11;
            _cameras[previous].Priority = 9;
        }

        //private void Update()
        //{
        //    if (_hacked == true)
        //    {
        //        if (Input.GetKeyDown(KeyCode.E))
        //        {
        //            var previous = _activeCamera;
        //            _activeCamera++;


        //            if (_activeCamera >= _cameras.Length)
        //                _activeCamera = 0;


        //            _cameras[_activeCamera].Priority = 11;
        //            _cameras[previous].Priority = 9;
        //        }

        //        if (Input.GetKeyDown(KeyCode.Escape))
        //        {
        //            _hacked = false;
        //            onHackEnded?.Invoke();
        //            ResetCameras();
        //        }
        //    }
        //}

        void ResetCameras()
        {
            foreach (var cam in _cameras)
            {
                cam.Priority = 9;
            }
        }

        private void InteractableZone_onHoldStarted(int zoneID)
        {
            if (zoneID == 3 && _hacked == false) //Hacking terminal
            {
                _progressBar.gameObject.SetActive(true);
                StartCoroutine(HackingRoutine());
                onHackComplete?.Invoke();
            }
        }

        private void InteractableZone_onHoldEnded(int zoneID)
        {
            if (zoneID == 3) //Hacking terminal
            {
                if (_hacked == true)
                    return;

                StopAllCoroutines();
                _progressBar.gameObject.SetActive(false);
                _progressBar.value = 0;
                onHackEnded?.Invoke();
            }
        }

        
        IEnumerator HackingRoutine()
        {
            while (_progressBar.value < 1)
            {
                _progressBar.value += Time.deltaTime / _hackTime;
                yield return new WaitForEndOfFrame();
            }

            //successfully hacked
            _hacked = true;
            _interactableZone.CompleteTask(3);
            _input.Laptop.Enable();
            _input.Laptop.CameraSwitch.performed += CameraSwitch_performed;
            _input.Laptop.Leave.performed += Leave_performed;

            //hide progress bar
            _progressBar.gameObject.SetActive(false);

            //enable Vcam1
            _cameras[0].Priority = 11;
        }

        private void Leave_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            _hacked = false;
            onHackEnded?.Invoke();
            ResetCameras();
        }

        private void OnDisable()
        {
            InteractableZone.onHoldStarted -= InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded -= InteractableZone_onHoldEnded;
        }
    }

}

