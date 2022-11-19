﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.UI;


namespace Game.Scripts.LiveObjects
{
    public class InteractableZone : MonoBehaviour
    {
        
        private InputActions _input;

        private enum ZoneType
        {
            Collectable,
            Action,
            HoldAction
        }

        private enum KeyState
        {
            Press,
            PressHold
        }

        [SerializeField]
        private ZoneType _zoneType;
        [SerializeField]
        private int _zoneID;
        [SerializeField]
        private int _requiredID;
        [SerializeField]
        [Tooltip("Press the (---) Key to .....")]
        private string _displayMessage;
        [SerializeField]
        private GameObject[] _zoneItems;
        private bool _inZone = false;
        private bool _itemsCollected = false;
        private bool _actionPerformed = false;
        [SerializeField]
        private Sprite _inventoryIcon;
        //[SerializeField]
        //private KeyCode _zoneKeyInput;
        //[SerializeField]
        //private KeyState _keyState;
        [SerializeField]
        private GameObject _marker;

        private bool _inHoldState = false;

        private static int _currentZoneID = 0;

        private void Start()
        {
            _input = new InputActions();
            _input.Player.Enable();
            _input.Player.Interactions.performed += Interactions_performed;
            _input.Player.HoldInteraction.performed += HoldInteraction_performed;

        }

        private void HoldInteraction_performed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            Debug.Log("holdperformed");
            if (_inZone == true)
            {
                PerformHoldAction();

            }
        }

        private void Interactions_performed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            Debug.Log("performed");
            if (_inZone == true)
            {
                switch (_zoneType)
                {
                    case ZoneType.Collectable:
                        if (_itemsCollected == false)
                        {
                            CollectItems();
                            _itemsCollected = true;
                            UIManager.Instance.DisplayInteractableZoneMessage(false);
                        }
                        break;

                    case ZoneType.Action:
                        if (_actionPerformed == false)
                        {
                            PerformAction();
                            _actionPerformed = true;
                            UIManager.Instance.DisplayInteractableZoneMessage(false);
                        }
                        break;
                }
                //CollectItems();
                // PerformAction();
                //PerformHoldAction();

            }
        }

        public static int CurrentZoneID
        { 
            get 
            { 
               return _currentZoneID; 
            }
            set
            {
                _currentZoneID = value; 
                         
            }
        }


        public static event Action<InteractableZone> onZoneInteractionComplete;
        public static event Action<int> onHoldStarted;
        public static event Action<int> onHoldEnded;

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += SetMarker;

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && _currentZoneID > _requiredID)
            {
                switch (_zoneType)
                {
                    case ZoneType.Collectable:
                        if (_itemsCollected == false)
                        {
                            _inZone = true;
                            if (_displayMessage != null)
                            {
                                string message = $"Press the action key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the action key to collect");
                        }
                        break;

                    case ZoneType.Action:
                        if (_actionPerformed == false)
                        {
                            _inZone = true;
                            if (_displayMessage != null)
                            {
                                string message = $"Press the action key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the action key to perform action");
                        }
                        break;

                    case ZoneType.HoldAction:
                        _inZone = true;
                        if (_displayMessage != null)
                        {
                            string message = $"Press the alt action key to {_displayMessage}.";
                            UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                        }
                        else
                            UIManager.Instance.DisplayInteractableZoneMessage(true, $"Hold the action key to perform action");
                        break;
                }
            }
        }

        //private void Update()
        //{
        //    if (_inZone == true)
        //    {

        //        if (Input.GetKeyDown(_zoneKeyInput) && _keyState != KeyState.PressHold)
        //        {
        //            //press
        //            switch (_zoneType)
        //            {
        //                case ZoneType.Collectable:
        //                    if (_itemsCollected == false)
        //                    {
        //                        CollectItems();
        //                        _itemsCollected = true;
        //                        UIManager.Instance.DisplayInteractableZoneMessage(false);
        //                    }
        //                    break;

        //                case ZoneType.Action:
        //                    if (_actionPerformed == false)
        //                    {
        //                        PerformAction();
        //                        _actionPerformed = true;
        //                        UIManager.Instance.DisplayInteractableZoneMessage(false);
        //                    }
        //                    break;
        //            }
        //        }
        //        else if (Input.GetKey(_zoneKeyInput) && _keyState == KeyState.PressHold && _inHoldState == false)
        //        {
        //            _inHoldState = true;

                   

        //            switch (_zoneType)
        //            {                      
        //                case ZoneType.HoldAction:
        //                    PerformHoldAction();
        //                    break;           
        //            }
        //        }

        //        if (Input.GetKeyUp(_zoneKeyInput) && _keyState == KeyState.PressHold)
        //        {
        //            _inHoldState = false;
        //            onHoldEnded?.Invoke(_zoneID);
        //        }

               
        //    }
        //}
       
        private void CollectItems()
        {
            foreach (var item in _zoneItems)
            {
                item.SetActive(false);
            }

            UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            CompleteTask(_zoneID);

            onZoneInteractionComplete?.Invoke(this);

        }

        private void PerformAction()
        {
            
            foreach (var item in _zoneItems)
            {
                item.SetActive(true);
            }

            if (_inventoryIcon != null)
                UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            onZoneInteractionComplete?.Invoke(this);
        }

        private void PerformHoldAction()
        {
            UIManager.Instance.DisplayInteractableZoneMessage(false);
            onHoldStarted?.Invoke(_zoneID);
        }

        public GameObject[] GetItems()
        {
            return _zoneItems;
        }

        public int GetZoneID()
        {
            return _zoneID;
        }

        public void CompleteTask(int zoneID)
        {
            if (zoneID == _zoneID)
            {
                _currentZoneID++;
                Debug.Log(_currentZoneID);
                onZoneInteractionComplete?.Invoke(this);
            }
        }

        public void ResetAction(int zoneID)
        {
            if (zoneID == _zoneID)
                _actionPerformed = false;
        }

        public void SetMarker(InteractableZone zone)
        {
            if (_zoneID == _currentZoneID)
                _marker.SetActive(true);
            else
                _marker.SetActive(false);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _inZone = false;
                UIManager.Instance.DisplayInteractableZoneMessage(false);
            }
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= SetMarker;
        }       
        
    }
}


