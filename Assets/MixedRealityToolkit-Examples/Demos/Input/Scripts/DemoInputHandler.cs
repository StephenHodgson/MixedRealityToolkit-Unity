using Microsoft.MixedReality.Toolkit.Core.Definitions.InputSystem;
using Microsoft.MixedReality.Toolkit.Core.EventDatum.Input;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.InputSystem.Handlers;
using Microsoft.MixedReality.Toolkit.Core.Services;
using Microsoft.MixedReality.Toolkit.SDK.Input.Handlers;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.Input
{
    public class DemoInputHandler : BaseInputHandler,
            IMixedRealitySourceStateHandler,
            IMixedRealityInputHandler<float>,
            IMixedRealityInputHandler<Vector2>
    {
        [SerializeField]
        [Tooltip("The action that will be used for selecting objects.")]
        private MixedRealityInputAction selectAction = MixedRealityInputAction.None;

        [SerializeField]
        [Tooltip("The action that will move the camera forward, back, left, and right.")]
        private MixedRealityInputAction movementAction = MixedRealityInputAction.None;

        [SerializeField]
        [Tooltip("The action that will pivot the camera on it's axis.")]
        private MixedRealityInputAction rotateAction = MixedRealityInputAction.None;

        [SerializeField]
        [Tooltip("The action that will move the camera up or down vertically.")]
        private MixedRealityInputAction heightAction = MixedRealityInputAction.None;

        private Vector3 newPosition = Vector3.zero;

        private Vector3 newRotation = Vector3.zero;

        #region Monobehaviour Implementation

        private void Awake()
        {
            Debug.Log($"[Awake] Is MRTK initialized? {MixedRealityToolkit.Instance != null}");
            Debug.Log($"[Awake] Is Input System initialized? {MixedRealityToolkit.InputSystem != null}");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Debug.Log($"[OnEnable] Is MRTK initialized? {MixedRealityToolkit.Instance != null}");
            Debug.Log($"[OnEnable] Is Input System initialized? {MixedRealityToolkit.InputSystem != null}");
        }

        protected override void Start()
        {
            base.Start();

            Debug.Log($"[Start] Is MRTK initialized? {MixedRealityToolkit.Instance != null}");
            Debug.Log($"[Start] Is Input System initialized? {MixedRealityToolkit.InputSystem != null}");

            if (MixedRealityToolkit.InputSystem != null)
            {
                foreach (var inputSource in MixedRealityToolkit.InputSystem.DetectedInputSources)
                {
                    Debug.Log($"OnSourceDetected {inputSource.SourceName}");
                }
            }
        }

        #endregion Monobehaviour Implementation

        #region IMixedRealityInputHandler Implementation

        public void OnInputUp(InputEventData eventData)
        {
            if (eventData.MixedRealityInputAction == selectAction)
            {
                Debug.Log($"[OnInputUp] {eventData.MixedRealityInputAction.Description}");
            }
        }

        public void OnInputDown(InputEventData eventData)
        {
            if (eventData.MixedRealityInputAction == selectAction)
            {
                Debug.Log($"[OnInputDown] {eventData.MixedRealityInputAction.Description}");
            }
        }

        public void OnInputChanged(InputEventData<float> eventData)
        {
            if (eventData.MixedRealityInputAction == heightAction)
            {
                Debug.Log($"[OnInputPressed] {eventData.MixedRealityInputAction.Description} | Value: {eventData.InputData}");
                newPosition.x = 0f;
                newPosition.y = eventData.InputData;
                newPosition.z = 0f;
                gameObject.transform.position += newPosition;
            }
        }

        public void OnInputChanged(InputEventData<Vector2> eventData)
        {
            if (eventData.MixedRealityInputAction == movementAction)
            {
                Debug.Log($"[OnInputChanged] {eventData.MixedRealityInputAction.Description} | Value: {eventData.InputData}");
                newPosition.x = eventData.InputData.x;
                newPosition.y = 0f;
                newPosition.z = eventData.InputData.y;
                gameObject.transform.position += newPosition;
            }
            else if (eventData.MixedRealityInputAction == rotateAction)
            {
                Debug.Log($"[OnInputChanged] {eventData.MixedRealityInputAction.Description} | Value: {eventData.InputData}");
                newRotation.x = eventData.InputData.x;
                newRotation.y = eventData.InputData.y;
            }
        }

        #endregion IMixedRealityInputHandler Implementation

        #region IMixedRealitySourceStateHandler Implementation

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            Debug.Log($"[OnSourceDetected] {eventData.InputSource.SourceName}");
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            Debug.Log($"[OnSourceLost] {eventData.InputSource.SourceName}");
        }

        #endregion IMixedRealitySourceStateHandler Implementation
    }
}