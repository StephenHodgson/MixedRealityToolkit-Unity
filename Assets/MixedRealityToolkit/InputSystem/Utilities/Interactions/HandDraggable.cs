// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.MixedReality.Toolkit.Internal.EventDatum.Input;
using Microsoft.MixedReality.Toolkit.InputSystem.Focus;
using Microsoft.MixedReality.Toolkit.InputSystem.Sources;
using Microsoft.MixedReality.Toolkit.Internal.Definitions;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem.Handlers;
using Microsoft.MixedReality.Toolkit.Internal.Managers;
using Microsoft.MixedReality.Toolkit.Internal.Utilities;
using UnityEngine;

#if UNITY_WSA
using UnityEngine.XR.WSA.Input;
#endif

namespace Microsoft.MixedReality.Toolkit.InputSystem.Utilities.Interactions
{
    /// <summary>
    /// Component that allows dragging an object with your hand on HoloLens.
    /// Dragging is done by calculating the angular delta and z-delta between the current and previous hand positions,
    /// and then repositioning the object based on that.
    /// </summary>
    public class HandDraggable : FocusTarget, IInputHandler, ISourceStateHandler
    {
        /// <summary>
        /// Event triggered when dragging starts.
        /// </summary>
        public event Action StartedDragging;

        /// <summary>
        /// Event triggered when dragging stops.
        /// </summary>
        public event Action StoppedDragging;

        [Tooltip("Transform that will be dragged. Defaults to the object of the component.")]
        public Transform HostTransform;

        [Tooltip("Scale by which hand movement in z is multiplied to move the dragged object.")]
        public float DistanceScale = 2f;

        public enum RotationModeEnum
        {
            Default,
            LockObjectRotation,
            OrientTowardUser,
            OrientTowardUserAndKeepUpright
        }

        public RotationModeEnum RotationMode = RotationModeEnum.Default;

        [Tooltip("Controls the speed at which the object will interpolate toward the desired position")]
        [Range(0.01f, 1.0f)]
        public float PositionLerpSpeed = 0.2f;

        [Tooltip("Controls the speed at which the object will interpolate toward the desired rotation")]
        [Range(0.01f, 1.0f)]
        public float RotationLerpSpeed = 0.2f;

        public bool IsDraggingEnabled = true;

        private bool isDragging;
        private Vector3 objRefForward;
        private Vector3 objRefUp;
        private float objRefDistance;
        private Quaternion gazeAngularOffset;
        private float handRefDistance;
        private Vector3 objRefGrabPoint;

        private Vector3 draggingPosition;
        private Quaternion draggingRotation;

        private uint currentInputSourceId;
        private Rigidbody hostRigidbody;
        private bool hostRigidbodyWasKinematic;
        private IMixedRealityInputSystem inputSystem;

        private void Awake()
        {
            inputSystem = MixedRealityManager.Instance.GetManager<IMixedRealityInputSystem>();
        }

        private void Start()
        {
            if (HostTransform == null)
            {
                HostTransform = transform;
            }

            hostRigidbody = HostTransform.GetComponent<Rigidbody>();
        }

        private void OnDestroy()
        {
            if (isDragging)
            {
                StopDragging();
            }
        }

        private void Update()
        {
            if (IsDraggingEnabled && isDragging)
            {
                UpdateDragging();
            }
        }

        /// <summary>
        /// Starts dragging the object.
        /// </summary>
        public void StartDragging(Vector3 initialDraggingPosition)
        {
            if (!IsDraggingEnabled)
            {
                return;
            }

            if (isDragging)
            {
                return;
            }

            // TODO: robertes: Fix push/pop and single-handler model so that multiple HandDraggable components can be active at once.

            // Add self as a modal input handler, to get all inputs during the manipulation
            inputSystem.PushModalInputHandler(gameObject);

            isDragging = true;

            if (hostRigidbody != null)
            {
                hostRigidbodyWasKinematic = hostRigidbody.isKinematic;
                hostRigidbody.isKinematic = true;
            }

            Transform cameraTransform = CameraCache.Main.transform;

            Vector3 inputPosition = Vector3.zero;
#if UNITY_WSA
            InteractionSourceKind sourceKind;
            InteractionInputSources.TryGetSourceKind(currentInputSourceId, out sourceKind);

            switch (sourceKind)
            {
                case InteractionSourceKind.Hand:
                    InteractionInputSources.TryGetGripPosition(currentInputSourceId, out inputPosition);
                    break;
                case InteractionSourceKind.Controller:
                    InteractionInputSources.TryGetPointerPosition(currentInputSourceId, out inputPosition);
                    break;
            }
#else
            InteractionInputSources.TryGetPointerPosition(currentInputSourceId, out inputPosition);
#endif

            Vector3 pivotPosition = GetHandPivotPosition(cameraTransform);
            handRefDistance = Vector3.Magnitude(inputPosition - pivotPosition);
            objRefDistance = Vector3.Magnitude(initialDraggingPosition - pivotPosition);

            Vector3 objForward = HostTransform.forward;
            Vector3 objUp = HostTransform.up;
            // Store where the object was grabbed from
            objRefGrabPoint = cameraTransform.transform.InverseTransformDirection(HostTransform.position - initialDraggingPosition);

            Vector3 objDirection = Vector3.Normalize(initialDraggingPosition - pivotPosition);
            Vector3 handDirection = Vector3.Normalize(inputPosition - pivotPosition);

            objForward = cameraTransform.InverseTransformDirection(objForward);       // in camera space
            objUp = cameraTransform.InverseTransformDirection(objUp);                 // in camera space
            objDirection = cameraTransform.InverseTransformDirection(objDirection);   // in camera space
            handDirection = cameraTransform.InverseTransformDirection(handDirection); // in camera space

            objRefForward = objForward;
            objRefUp = objUp;

            // Store the initial offset between the hand and the object, so that we can consider it when dragging
            gazeAngularOffset = Quaternion.FromToRotation(handDirection, objDirection);
            draggingPosition = initialDraggingPosition;

            StartedDragging?.Invoke();
        }

        /// <summary>
        /// Gets the pivot position for the hand, which is approximated to the base of the neck.
        /// </summary>
        /// <returns>Pivot position for the hand.</returns>
        private Vector3 GetHandPivotPosition(Transform cameraTransform)
        {
            return cameraTransform.position + new Vector3(0, -0.2f, 0) - cameraTransform.forward * 0.2f; // a bit lower and behind
        }

        /// <summary>
        /// Enables or disables dragging.
        /// </summary>
        /// <param name="isEnabled">Indicates whether dragging should be enabled or disabled.</param>
        public void SetDragging(bool isEnabled)
        {
            if (IsDraggingEnabled == isEnabled)
            {
                return;
            }

            IsDraggingEnabled = isEnabled;

            if (isDragging)
            {
                StopDragging();
            }
        }

        /// <summary>
        /// Update the position of the object being dragged.
        /// </summary>
        private void UpdateDragging()
        {
            Transform cameraTransform = CameraCache.Main.transform;

            Vector3 inputPosition = Vector3.zero;
#if UNITY_WSA
            InteractionSourceKind sourceKind;
            InteractionInputSources.TryGetSourceKind(currentInputSourceId, out sourceKind);
            switch (sourceKind)
            {
                case InteractionSourceKind.Hand:
                    InteractionInputSources.TryGetGripPosition(currentInputSourceId, out inputPosition);
                    break;
                case InteractionSourceKind.Controller:
                    InteractionInputSources.TryGetPointerPosition(currentInputSourceId, out inputPosition);
                    break;
            }
#else
            InteractionInputSources.TryGetPointerPosition(currentInputSourceId, out inputPosition);
#endif

            Vector3 pivotPosition = GetHandPivotPosition(cameraTransform);

            Vector3 newHandDirection = Vector3.Normalize(inputPosition - pivotPosition);

            newHandDirection = cameraTransform.InverseTransformDirection(newHandDirection); // in camera space
            Vector3 targetDirection = Vector3.Normalize(gazeAngularOffset * newHandDirection);
            targetDirection = cameraTransform.TransformDirection(targetDirection); // back to world space

            float currentHandDistance = Vector3.Magnitude(inputPosition - pivotPosition);

            float distanceRatio = currentHandDistance / handRefDistance;
            float distanceOffset = distanceRatio > 0 ? (distanceRatio - 1f) * DistanceScale : 0;
            float targetDistance = objRefDistance + distanceOffset;

            draggingPosition = pivotPosition + (targetDirection * targetDistance);

            if (RotationMode == RotationModeEnum.OrientTowardUser || RotationMode == RotationModeEnum.OrientTowardUserAndKeepUpright)
            {
                draggingRotation = Quaternion.LookRotation(HostTransform.position - pivotPosition);
            }
            else if (RotationMode == RotationModeEnum.LockObjectRotation)
            {
                draggingRotation = HostTransform.rotation;
            }
            else // RotationModeEnum.Default
            {
                Vector3 objForward = cameraTransform.TransformDirection(objRefForward); // in world space
                Vector3 objUp = cameraTransform.TransformDirection(objRefUp);           // in world space
                draggingRotation = Quaternion.LookRotation(objForward, objUp);
            }

            Vector3 newPosition = Vector3.Lerp(HostTransform.position, draggingPosition + cameraTransform.TransformDirection(objRefGrabPoint), PositionLerpSpeed);
            // Apply Final Position
            if (hostRigidbody == null)
            {
                HostTransform.position = newPosition;
            }
            else
            {
                hostRigidbody.MovePosition(newPosition);
            }

            // Apply Final Rotation
            Quaternion newRotation = Quaternion.Lerp(HostTransform.rotation, draggingRotation, RotationLerpSpeed);
            if (hostRigidbody == null)
            {
                HostTransform.rotation = newRotation;
            }
            else
            {
                hostRigidbody.MoveRotation(newRotation);
            }

            if (RotationMode == RotationModeEnum.OrientTowardUserAndKeepUpright)
            {
                Quaternion upRotation = Quaternion.FromToRotation(HostTransform.up, Vector3.up);
                HostTransform.rotation = upRotation * HostTransform.rotation;
            }
        }

        /// <summary>
        /// Stops dragging the object.
        /// </summary>
        public void StopDragging()
        {
            if (!isDragging)
            {
                return;
            }

            // Remove self as a modal input handler
            inputSystem.PopModalInputHandler();

            isDragging = false;
            currentInputSourceId = 0;
            if (hostRigidbody != null)
            {
                hostRigidbody.isKinematic = hostRigidbodyWasKinematic;
            }
            StoppedDragging?.Invoke();
        }

        public override void OnFocusEnter(FocusEventData eventData)
        {
            base.OnFocusEnter(eventData);

            if (!IsDraggingEnabled)
            {
                return;
            }
        }

        public override void OnFocusExit(FocusEventData eventData)
        {
            base.OnFocusEnter(eventData);

            if (!IsDraggingEnabled)
            {
                return;
            }
        }

        void IInputHandler.OnInputUp(InputEventData eventData)
        {
            if (eventData.SourceId == currentInputSourceId)
            {
                eventData.Use(); // Mark the event as used, so it doesn't fall through to other handlers.

                StopDragging();
            }
        }

        void IInputHandler.OnInputDown(InputEventData eventData)
        {
            if (isDragging)
            {
                // We're already handling drag input, so we can't start a new drag operation.
                return;
            }

#if UNITY_WSA
            InteractionSourceKind sourceKind;
            InteractionInputSources.TryGetSourceKind(eventData.SourceId, out sourceKind);
            if (sourceKind != InteractionSourceKind.Hand)
            {
                if (!eventData.InputSource.SupportsInputCapability(new[] { InputType.GripPosition }))
                {
                    // The input source must provide grip positional data for this script to be usable
                    return;
                }
            }
#else
            if (!eventData.InputSource.SupportsInputCapability(new[] { InputType.PointerPosition }))
            {
                // The input source must provide positional data for this script to be usable
                return;
            }
#endif

            eventData.Use(); // Mark the event as used, so it doesn't fall through to other handlers.

            currentInputSourceId = eventData.SourceId;

            FocusDetails focusDetails;
            Vector3 initialDraggingPosition = inputSystem.FocusProvider.TryGetFocusDetails(eventData, out focusDetails)
                ? focusDetails.Point
                : HostTransform.position;

            StartDragging(initialDraggingPosition);
        }

        void IInputHandler.OnInputPressed(InputPressedEventData eventData) { }

        void IInputHandler.OnDualAxisInputChanged(InputDualAxisPositionEventData eventData) { }

        void ISourceStateHandler.OnSourceDetected(SourceStateEventData eventData) { }

        void ISourceStateHandler.OnSourceLost(SourceStateEventData eventData)
        {
            if (eventData.SourceId == currentInputSourceId)
            {
                StopDragging();
            }
        }

        void ISourceStateHandler.OnSourcePositionChanged(SourcePositionEventData eventData) { }

        void ISourceStateHandler.OnSourceRotationChanged(SourceRotationEventData eventData) { }
    }
}
