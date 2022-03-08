using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KWUtils
{
    public static class InputSystemExtension
    {
        public static Vector2 GetMousePosition => Mouse.current.position.ReadValue();
        
        // SINGLE ACTIONS
        //==============================================================================================================
        
        //STARTED EVENT
        public static void EnableStartEvent(this InputAction inputAction, Action<InputAction.CallbackContext> start) => inputAction.started += start;
        public static void DisableStartEvent(this InputAction inputAction, Action<InputAction.CallbackContext> start) => inputAction.started -= start;
        //PERFORMED EVENT
        public static void EnablePerformEvent(this InputAction inputAction, Action<InputAction.CallbackContext> perform) => inputAction.performed += perform;
        public static void DisablePerformEvent(this InputAction inputAction, Action<InputAction.CallbackContext> perform) => inputAction.performed -= perform;
        //CANCELED EVENT
        public static void EnableCancelEvent(this InputAction inputAction, Action<InputAction.CallbackContext> cancel) => inputAction.canceled += cancel;
        public static void DisableCancelEvent(this InputAction inputAction, Action<InputAction.CallbackContext> cancel) => inputAction.canceled -= cancel;


        // 2 ACTIONS
        //==============================================================================================================
        
        //START-PERFORM
        public static void EnableStartPerformEvent(this InputAction inputAction,
            Action<InputAction.CallbackContext> start,
            Action<InputAction.CallbackContext> perform)
        {
            inputAction.started += start;
            inputAction.performed += perform;
        }
        
        public static void DisableStartPerformEvent(this InputAction inputAction,
            Action<InputAction.CallbackContext> start,
            Action<InputAction.CallbackContext> perform)
        {
            inputAction.started -= start;
            inputAction.performed -= perform;
        }
        
        //PERFORM-CANCEL
        public static void EnablePerformCancelEvent(this InputAction inputAction,
            Action<InputAction.CallbackContext> perform,
            Action<InputAction.CallbackContext> cancel)
        {
            inputAction.performed += perform;
            inputAction.canceled += cancel;
        }
        
        public static void DisablePerformCancelEvent(this InputAction inputAction,
            Action<InputAction.CallbackContext> perform,
            Action<InputAction.CallbackContext> cancel)
        {
            inputAction.performed -= perform;
            inputAction.canceled -= cancel;
        }
        
        //START-CANCEL
        public static void EnableStartCancelEvent(this InputAction inputAction,
            Action<InputAction.CallbackContext> start,
            Action<InputAction.CallbackContext> cancel)
        {
            inputAction.started += start;
            inputAction.canceled += cancel;
        }
        
        public static void DisableStartCancelEvent(this InputAction inputAction,
            Action<InputAction.CallbackContext> start,
            Action<InputAction.CallbackContext> cancel)
        {
            inputAction.started -= start;
            inputAction.canceled -= cancel;
        }
        
        // ALL ACTIONS
        //==============================================================================================================
        
        //STARTED-PERFORMED-CANCELED
        public static void EnableAllEvents(this InputAction inputAction,
            Action<InputAction.CallbackContext> start, 
            Action<InputAction.CallbackContext> performed, 
            Action<InputAction.CallbackContext> cancel)
        {
            inputAction.started += start;
            inputAction.performed += performed;
            inputAction.canceled += cancel;
        }
        
        public static void DisableAllEvents(this InputAction inputAction,
            Action<InputAction.CallbackContext> start, 
            Action<InputAction.CallbackContext> performed, 
            Action<InputAction.CallbackContext> cancel)
        {
            inputAction.started -= start;
            inputAction.performed -= performed;
            inputAction.canceled -= cancel;
        }
        
//======================================================================================================================
//TOGGLE EVENTS
//======================================================================================================================


        //SINGLE ACTION
        //==============================================================================================================
        
        //STARTED EVENT
        public static void ToggleStartEvent(this InputAction inputAction, Action<InputAction.CallbackContext> start, bool toggleState)
        {
            if(toggleState)
                inputAction.started += start;
            else
                inputAction.started -= start;
        }
        
        //PERFORMED EVENT
        public static void TogglePerformEvent(this InputAction inputAction, Action<InputAction.CallbackContext> perform, bool toggleState)
        {
            if(toggleState)
                inputAction.performed += perform;
            else
                inputAction.performed -= perform;
        }
        
        //CANCELED EVENT
        public static void ToggleCancelEvent(this InputAction inputAction, Action<InputAction.CallbackContext> cancel, bool toggleState)
        {
            if(toggleState)
                inputAction.canceled += cancel;
            else
                inputAction.canceled -= cancel;
        }
        
        // 2 ACTIONS
        //==============================================================================================================
        
        //START-PERFORM
        public static void ToggleStartPerformEvent(this InputAction inputAction,
            Action<InputAction.CallbackContext> start,
            Action<InputAction.CallbackContext> perform,
            bool toggleState)
        {
            if (toggleState)
            {
                inputAction.started += start;
                inputAction.performed += perform;
            }
            else
            {
                inputAction.started -= start;
                inputAction.performed -= perform;
            }
        }
        

        
        //PERFORM-CANCEL
        public static void TogglePerformCancelEvent(this InputAction inputAction,
            Action<InputAction.CallbackContext> perform,
            Action<InputAction.CallbackContext> cancel,
            bool toggleState)
        {
            if (toggleState)
            {
                inputAction.performed += perform;
                inputAction.canceled += cancel;
            }
            else
            {
                inputAction.performed -= perform;
                inputAction.canceled -= cancel;
            }
        }
        
        //START-CANCEL
        public static void ToggleStartCancelEvent(this InputAction inputAction,
            Action<InputAction.CallbackContext> start,
            Action<InputAction.CallbackContext> cancel,
            bool toggleState)
        {
            if (toggleState)
            {
                inputAction.started += start;
                inputAction.canceled += cancel;
            }
            else
            {
                inputAction.started -= start;
                inputAction.canceled -= cancel;
            }
        }
        
        // ALL ACTIONS
        //==============================================================================================================
        
        //STARTED-PERFORMED-CANCELED
        public static void ToggleAllEvents(this InputAction inputAction,
            Action<InputAction.CallbackContext> start, 
            Action<InputAction.CallbackContext> performed, 
            Action<InputAction.CallbackContext> cancel,
            bool toggleState)
        {
            if (toggleState)
            {
                inputAction.started += start;
                inputAction.performed += performed;
                inputAction.canceled += cancel;
            }
            else
            {
                inputAction.started -= start;
                inputAction.performed -= performed;
                inputAction.canceled -= cancel;
            }
        }
    }
}
