﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.InputSystem.Cursors;
using Microsoft.MixedReality.Toolkit.InputSystem.EventData;
using Microsoft.MixedReality.Toolkit.InputSystem.Focus;
using Microsoft.MixedReality.Toolkit.InputSystem.Gaze;
using Microsoft.MixedReality.Toolkit.InputSystem.InputHandlers;
using Microsoft.MixedReality.Toolkit.InputSystem.Sources;
using Microsoft.MixedReality.Toolkit.InputSystem.Utilities;
using Microsoft.MixedReality.Toolkit.Internal.Definitions;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces;
using Microsoft.MixedReality.Toolkit.Internal.Managers;
using System.Collections;
using UnityEngine;

#if UNITY_WSA
#endif

namespace Microsoft.MixedReality.Toolkit.InputSystem.Pointers
{
    /// <summary>
    /// Base Pointer class for pointers that exist in the scene as GameObjects.
    /// </summary>
    public abstract class BaseControllerPointer : AttachToController, IInputHandler, IPointer
    {
        private IMixedRealityInputSystem inputSystem = null;
        public IMixedRealityInputSystem InputSystem => inputSystem ?? (inputSystem = MixedRealityManager.Instance.GetManager<IMixedRealityInputSystem>());

        [Header("Cursor")]
        [SerializeField]
        protected GameObject CursorPrefab;

        [Header("Interaction")]
        [SerializeField]
        private bool interactionEnabled = true;

        [SerializeField]
        [Range(0f, 360f)]
        protected float CurrentPointerOrientation;

        [SerializeField]
        [Range(0.5f, 50f)]
        private float pointerExtent = 2f;

        [SerializeField]
        [Tooltip("Source transform for raycast origin - leave null to use default transform")]
        protected Transform RaycastOrigin;

        [SerializeField]
        private KeyCode interactionEnabledKeyCode = KeyCode.None;

        [SerializeField]
        private KeyCode activeHoldKeyCode = KeyCode.None;

        [SerializeField]
        private InputType activeHoldPressType = InputType.Select;

        [SerializeField]
        private InputType interactionEnabledPressType = InputType.Select;

        [SerializeField]
        private bool interactionRequiresHold = false;

        /// <summary>
        /// True if select is pressed right now
        /// </summary>
        protected bool SelectPressed = false;

        /// <summary>
        /// True if select has been pressed once since startup
        /// </summary>
        protected bool SelectPressedOnce = false;

        private bool delayPointerRegistration = true;

        /// <summary>
        /// The Y orientation of the pointer target - used for touchpad rotation and navigation
        /// </summary>
        public virtual float PointerOrientation
        {
            get
            {
                return CurrentPointerOrientation + (RaycastOrigin != null ? RaycastOrigin.eulerAngles.y : transform.eulerAngles.y);
            }
            set
            {
                CurrentPointerOrientation = value;
            }
        }

        /// <summary>
        /// The forward direction of the targeting ray
        /// </summary>
        public virtual Vector3 PointerDirection => RaycastOrigin != null ? RaycastOrigin.forward : transform.forward;

        #region Monobehaviour Implementation

        protected override void OnEnable()
        {
            base.OnEnable();
            SelectPressed = false;

            if (BaseCursor != null)
            {
                BaseCursor.enabled = true;
            }

            if (!delayPointerRegistration)
            {
                InputSystem.FocusProvider.RegisterPointer(this);
            }
        }

        protected virtual void Start()
        {
             Debug.Assert(InputSourceParent != null, "This Pointer must have a Input Source Assigned");

            InputSystem.FocusProvider.RegisterPointer(this);
            delayPointerRegistration = false;

            SetCursor();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SelectPressed = false;

            if (BaseCursor != null)
            {
                BaseCursor.enabled = false;
            }

            InputSystem.FocusProvider.UnregisterPointer(this);
        }

        protected override void OnDestroy()
        {
            if (BaseCursor != null)
            {
                Destroy(BaseCursor.gameObject);
            }

            base.OnDestroy();
        }

        #endregion  Monobehaviour Implementation

        protected override void OnAttachToController()
        {
            // Subscribe to interaction events
            inputSystem.Register(gameObject);
        }

        protected override void OnDetachFromController()
        {
            // Unsubscribe from interaction events
            inputSystem.Unregister(gameObject);
        }

        public void SetCursor(GameObject newCursor = null)
        {
            CursorPrefab = newCursor == null ? CursorPrefab : newCursor;

            if (CursorPrefab != null)
            {
                var cursorObj = Instantiate(CursorPrefab, transform);
                cursorObj.name = $"{name}_Cursor";
                BaseCursor = cursorObj.GetComponent<BaseCursor>();
                 Debug.Assert(BaseCursor != null, "Failed to load cursor");
                BaseCursor.Pointer = this;
                 Debug.Assert(BaseCursor.Pointer != null, "Failed to assign cursor!");
            }
        }

        /// <summary>
        /// Call to initiate a select action for this pointer
        /// </summary>
        public virtual void OnSelectPressed()
        {
            SelectPressed = true;
            SelectPressedOnce = true;
        }

        public virtual void OnSelectReleased()
        {
            SelectPressed = false;
        }

        #region IPointer Implementation

        private uint pointerId;

        public uint PointerId
        {
            get
            {
                if (pointerId == 0)
                {
                    pointerId = InputSystem.FocusProvider.GenerateNewPointerId();
                }

                return pointerId;
            }
        }

        public string PointerName
        {
            get { return gameObject.name; }
            set { gameObject.name = value; }
        }

        public IInputSource InputSourceParent { get; set; }

        public BaseCursor BaseCursor { get; set; }

        public ICursorModifier CursorModifier { get; set; }

        public ITeleportTarget TeleportTarget { get; set; }

        public virtual bool InteractionEnabled
        {
            get { return interactionEnabled; }
            set { interactionEnabled = value; }
        }

        public bool FocusLocked { get; set; }

        public float? PointerExtent
        {
            get { return pointerExtent; }
            set { pointerExtent = value ?? InputSystem.FocusProvider.GlobalPointingExtent; }
        }

        public RayStep[] Rays { get; protected set; }

        public LayerMask[] PrioritizedLayerMasksOverride { get; set; }

        public IFocusHandler FocusTarget { get; set; }

        public PointerResult Result { get; set; }

        public BaseRayStabilizer RayStabilizer { get; set; }

        public virtual void OnPreRaycast() { }

        public virtual void OnPostRaycast() { }

        /// <summary>
        /// The world origin of the targeting ray
        /// </summary>
        public virtual bool TryGetPointerPosition(out Vector3 position)
        {
            position = RaycastOrigin != null ? RaycastOrigin.position : transform.position;
            return true;
        }

        public bool TryGetPointingRay(out Ray pointingRay)
        {
            Vector3 pointerPosition;
            TryGetPointerPosition(out pointerPosition);
            pointingRay = new Ray(pointerPosition, PointerDirection);
            return true;
        }

        public bool TryGetPointerRotation(out Quaternion rotation)
        {
            Vector3 pointerRotation = RaycastOrigin != null ? RaycastOrigin.eulerAngles : transform.eulerAngles;
            rotation = Quaternion.Euler(pointerRotation.x, PointerOrientation, pointerRotation.z);
            return true;
        }

        #region IEquality Implementation

        public static bool Equals(IPointer left, IPointer right)
        {
            return left.Equals(right);
        }

        bool IEqualityComparer.Equals(object left, object right)
        {
            return left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            if (obj.GetType() != GetType()) { return false; }

            return Equals((IPointer)obj);
        }

        private bool Equals(IPointer other)
        {
            return other != null && PointerId == other.PointerId && string.Equals(PointerName, other.PointerName);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 0;
                hashCode = (hashCode * 397) ^ (int)PointerId;
                hashCode = (hashCode * 397) ^ (PointerName != null ? PointerName.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion IEquality Implementation

        #endregion IPointer Implementation

        #region IInputHandler Implementation

        public virtual void OnInputUp(InputEventData eventData)
        {
            if (eventData.SourceId == InputSourceParent.SourceId)
            {
                bool interactionPressed = false;
#if UNITY_WSA
                interactionPressed = eventData.InputType == activeHoldPressType;
#endif
                if (interactionRequiresHold && (eventData.KeyCode == activeHoldKeyCode || interactionPressed))
                {
                    InteractionEnabled = false;
                }
#if UNITY_WSA
                interactionPressed = eventData.InputType == interactionEnabledPressType;
#endif
                if (eventData.KeyCode == interactionEnabledKeyCode || interactionPressed)
                {
                    OnSelectReleased();
                }
            }
        }

        public virtual void OnInputDown(InputEventData eventData)
        {
            if (eventData.SourceId == InputSourceParent.SourceId)
            {
                bool interactionPressed = false;
#if UNITY_WSA
                interactionPressed = eventData.InputType == activeHoldPressType;
#endif
                if (interactionRequiresHold && (eventData.KeyCode == activeHoldKeyCode || interactionPressed))
                {
                    InteractionEnabled = true;
                }

#if UNITY_WSA
                interactionPressed = eventData.InputType == interactionEnabledPressType;
#endif
                if (eventData.KeyCode == interactionEnabledKeyCode || interactionPressed)
                {
                    OnSelectPressed();
                }
            }
        }

        public virtual void OnInputPressed(InputPressedEventData eventData) { }

        /// <summary>
        /// Updates target point orientation via thumbstick
        /// </summary>
        public virtual void OnDualAxisInputChanged(InputDualAxisPositionEventData eventData) { }

        #endregion  IInputHandler Implementation
    }
}
