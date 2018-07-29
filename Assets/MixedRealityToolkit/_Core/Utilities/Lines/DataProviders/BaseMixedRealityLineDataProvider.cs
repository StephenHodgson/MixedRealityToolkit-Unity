﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Definitions.Lines;
using Microsoft.MixedReality.Toolkit.Internal.Utilities.Physics.Distorters;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Internal.Utilities.Lines.DataProviders
{
    /// <summary>
    /// Base class that provides data about a line.
    /// </summary>
    /// <remarks>Data to be consumed by other classes like the <see cref="Renderers.BaseMixedRealityLineRenderer"/></remarks>
    [DisallowMultipleComponent]
    public abstract class BaseMixedRealityLineDataProvider : MonoBehaviour
    {
        private const float MinRotationMagnitude = 0.0001f;

        public float UnClampedWorldLength => GetUnClampedWorldLengthInternal();

        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("Clamps the line's normalized start point. This setting will affect line renderers.")]
        private float lineStartClamp = 0f;

        /// <summary>
        /// Clamps the line's normalized start point. This setting will affect line renderers.
        /// </summary>
        public float LineStartClamp
        {
            get { return lineStartClamp; }
            set { lineStartClamp = Mathf.Clamp01(value); }
        }

        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("Clamps the line's normalized end point. This setting will affect line renderers.")]
        private float lineEndClamp = 1f;

        /// <summary>
        /// Clamps the line's normalized end point. This setting will affect line renderers.
        /// </summary>
        public float LineEndClamp
        {
            get { return lineEndClamp; }
            set { lineEndClamp = Mathf.Clamp01(value); }
        }

        [SerializeField]
        [Tooltip("Transform to use when translating points from local to world space. If null, this object's transform is used.")]
        private Transform customLineTransform;

        /// <summary>
        /// Transform to use when translating points from local to world space. If null, this object's transform is used.
        /// </summary>
        public Transform LineTransform
        {
            get { return customLineTransform != null ? customLineTransform : transform; }
            set { customLineTransform = value; }
        }

        [SerializeField]
        [Tooltip("Controls whether this line loops \nNote: some classes override this setting")]
        private bool loops = false;

        /// <summary>
        /// Controls whether this line loops
        /// </summary>
        /// <remarks>Some classes override this setting.</remarks>
        public virtual bool Loops
        {
            get { return loops; }
            set { loops = value; }
        }

        [SerializeField]
        [Tooltip("The rotation mode used in the GetRotation function. You can visualize rotations by checking Draw Rotations under Editor Settings.")]
        private LineRotationType rotationType = LineRotationType.Velocity;

        /// <summary>
        /// The rotation mode used in the GetRotation function. You can visualize rotations by checking Draw Rotations under Editor Settings.
        /// </summary>
        public LineRotationType RotationType
        {
            get { return rotationType; }
            set { rotationType = value; }
        }

        [SerializeField]
        [Tooltip("Reverses up vector when determining rotation along line")]
        private bool flipUpVector = false;

        /// <summary>
        /// Reverses up vector when determining rotation along line
        /// </summary>
        public bool FlipUpVector
        {
            get { return flipUpVector; }
            set { flipUpVector = value; }
        }

        [SerializeField]
        [Tooltip("Local space offset to transform position. Used to determine rotation along line in RelativeToOrigin rotation mode")]
        private Vector3 originOffset = Vector3.zero;

        /// <summary>
        /// Local space offset to transform position. Used to determine rotation along line in RelativeToOrigin rotation mode
        /// </summary>
        public Vector3 OriginOffset
        {
            get { return originOffset; }
            set { originOffset = value; }
        }

        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("The weight of manual up vectors in Velocity rotation mode")]
        private float manualUpVectorBlend = 0f;

        /// <summary>
        /// The weight of manual up vectors in Velocity rotation mode
        /// </summary>
        public float ManualUpVectorBlend
        {
            get { return manualUpVectorBlend; }
            set { manualUpVectorBlend = Mathf.Clamp01(value); }
        }

        [SerializeField]
        [Tooltip("These vectors are used with ManualUpVectorBlend to determine rotation along the line in Velocity rotation mode. Vectors are distributed along the normalized length of the line.")]
        private Vector3[] manualUpVectors = { Vector3.up, Vector3.up, Vector3.up };

        /// <summary>
        /// These vectors are used with ManualUpVectorBlend to determine rotation along the line in Velocity rotation mode. Vectors are distributed along the normalized length of the line.
        /// </summary>
        public Vector3[] ManualUpVectors
        {
            get { return manualUpVectors; }
            set { manualUpVectors = value; }
        }

        [SerializeField]
        [Range(0.0001f, 0.1f)]
        [Tooltip("Used in Velocity rotation mode. Smaller values are more accurate but more expensive")]
        private float velocitySearchRange = 0.02f;

        /// <summary>
        /// Used in Velocity rotation mode. 
        /// </summary>
        /// <remarks>
        /// Smaller values are more accurate but more expensive
        /// </remarks>
        public float VelocitySearchRange
        {
            get { return velocitySearchRange; }
            set { velocitySearchRange = Mathf.Clamp(value, 0.001f, 0.1f); }
        }

        [SerializeField]
        private List<Distorter> distorters = new List<Distorter>();

        /// <summary>
        /// A list of distorters that apply to this line
        /// </summary>
        public List<Distorter> Distorters
        {
            get
            {
                if (distorters.Count == 0)
                {
                    var newDistorters = GetComponents<Distorter>();

                    for (int i = 0; i < newDistorters.Length; i++)
                    {
                        distorters.Add(newDistorters[i]);
                    }
                }

                distorters.Sort();
                return distorters;
            }
        }

        [SerializeField]
        [Tooltip("NormalizedLength mode uses the DistortionStrength curve for distortion strength, Uniform uses UniformDistortionStrength along entire line")]
        private DistortionType distortionType = DistortionType.NormalizedLength;

        /// <summary>
        /// NormalizedLength mode uses the DistortionStrength curve for distortion strength, Uniform uses UniformDistortionStrength along entire line
        /// </summary>
        public DistortionType DistortionType
        {
            get { return distortionType; }
            set { distortionType = value; }
        }

        [SerializeField]
        private AnimationCurve distortionStrength = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        public AnimationCurve DistortionStrength
        {
            get { return distortionStrength; }
            set { distortionStrength = value; }
        }

        [Range(0f, 1f)]
        [SerializeField]
        private float uniformDistortionStrength = 1f;

        public float UniformDistortionStrength
        {
            get { return uniformDistortionStrength; }
            set { uniformDistortionStrength = Mathf.Clamp01(value); }
        }

        public Vector3 FirstPoint
        {
            get { return GetPoint(0); }
            set { SetPoint(0, value); }
        }

        public Vector3 LastPoint
        {
            get { return GetPoint(PointCount - 1); }
            set { SetPoint(PointCount - 1, value); }
        }

        #region BaseMixedRealityLineDataProvider Abstract Declarations

        /// <summary>
        /// The number of points this line has.
        /// </summary>
        public abstract int PointCount { get; }

        /// <summary>
        /// Sets the point at index.
        /// </summary>
        /// <param name="pointIndex"></param>
        /// <param name="point"></param>
        protected abstract void SetPointInternal(int pointIndex, Vector3 point);

        /// <summary>
        /// Get a point based on normalized distance along line
        /// Normalized distance will be pre-clamped
        /// </summary>
        /// <param name="normalizedLength"></param>
        /// <returns></returns>
        protected abstract Vector3 GetPointInternal(float normalizedLength);

        /// <summary>
        /// Get a point based on point index
        /// Point index will be pre-clamped
        /// </summary>
        /// <param name="pointIndex"></param>
        /// <returns></returns>
        protected abstract Vector3 GetPointInternal(int pointIndex);

        /// <summary>
        /// Gets the up vector at a normalized length along line (used for rotation)
        /// </summary>
        /// <param name="normalizedLength"></param>
        /// <returns></returns>
        protected virtual Vector3 GetUpVectorInternal(float normalizedLength)
        {
            return LineTransform.forward;
        }

        /// <summary>
        /// Get the UnClamped world length of the line
        /// </summary>
        /// <returns></returns>
        protected abstract float GetUnClampedWorldLengthInternal();

        #endregion BaseMixedRealityLineDataProvider Abstract Declarations

        #region Monobehavior Implementation

        protected virtual void OnValidate()
        {
            distorters.Sort();
        }

        protected virtual void OnEnable()
        {
            distorters.Sort();
        }

        #endregion Monobehavior Implementation

        /// <summary>
        /// Returns a normalized length corresponding to a world length
        /// Useful for determining LineStartClamp / LineEndClamp values
        /// </summary>
        /// <param name="worldLength"></param>
        /// <param name="searchResolution"></param>
        /// <returns></returns>
        public float GetNormalizedLengthFromWorldLength(float worldLength, int searchResolution = 10)
        {
            Vector3 lastPoint = GetUnClampedPoint(0f);
            float normalizedLength = 0f;
            float distanceSoFar = 0f;

            for (int i = 1; i < searchResolution; i++)
            {
                // Get the normalized length of this position along the line
                normalizedLength = (1f / searchResolution) * i;
                Vector3 currentPoint = GetUnClampedPoint(normalizedLength);
                distanceSoFar += Vector3.Distance(lastPoint, currentPoint);
                lastPoint = currentPoint;

                if (distanceSoFar >= worldLength)
                {
                    // We've reached the world length
                    break;
                }
            }

            return Mathf.Clamp01(normalizedLength);
        }

        /// <summary>
        /// Gets the velocity along the line
        /// </summary>
        /// <param name="normalizedLength"></param>
        /// <returns></returns>
        public Vector3 GetVelocity(float normalizedLength)
        {
            Vector3 velocity;

            if (normalizedLength < velocitySearchRange)
            {
                Vector3 currentPos = GetPoint(normalizedLength);
                Vector3 nextPos = GetPoint(normalizedLength + velocitySearchRange);
                velocity = (nextPos - currentPos).normalized;
            }
            else
            {
                Vector3 currentPos = GetPoint(normalizedLength);
                Vector3 prevPos = GetPoint(normalizedLength - velocitySearchRange);
                velocity = (currentPos - prevPos).normalized;
            }

            return velocity;
        }

        /// <summary>
        /// Gets the rotation of a point along the line at the specified length
        /// </summary>
        /// <param name="normalizedLength"></param>
        /// <param name="lineRotationType"></param>
        /// <returns></returns>
        public Quaternion GetRotation(float normalizedLength, LineRotationType lineRotationType = LineRotationType.None)
        {
            lineRotationType = (lineRotationType != LineRotationType.None) ? lineRotationType : rotationType;
            Vector3 rotationVector = Vector3.zero;

            switch (lineRotationType)
            {
                case LineRotationType.Velocity:
                    rotationVector = GetVelocity(normalizedLength);
                    break;
                case LineRotationType.RelativeToOrigin:
                    Vector3 point = GetPoint(normalizedLength);
                    Vector3 origin = LineTransform.TransformPoint(originOffset);
                    rotationVector = (point - origin).normalized;
                    break;
                case LineRotationType.None:
                    break;
            }

            if (rotationVector.magnitude < MinRotationMagnitude)
            {
                return LineTransform.rotation;
            }

            Vector3 upVector = GetUpVectorInternal(normalizedLength);

            if (manualUpVectorBlend > 0f)
            {
                Vector3 manualUpVector = LineUtility.GetVectorCollectionBlend(manualUpVectors, normalizedLength, Loops);
                upVector = Vector3.Lerp(upVector, manualUpVector, manualUpVector.magnitude);
            }

            if (flipUpVector)
            {
                upVector = -upVector;
            }

            return Quaternion.LookRotation(rotationVector, upVector);
        }

        /// <summary>
        /// Gets the rotation of a point along the line at the specified index
        /// </summary>
        /// <param name="pointIndex"></param>
        /// <param name="lineRotationType"></param>
        /// <returns></returns>
        public Quaternion GetRotation(int pointIndex, LineRotationType lineRotationType = LineRotationType.None)
        {
            return GetRotation((float)pointIndex / PointCount, lineRotationType != LineRotationType.None ? lineRotationType : rotationType);
        }

        /// <summary>
        /// Gets a point along the line at the specified normalized length.
        /// </summary>
        /// <param name="normalizedLength"></param>
        /// <returns></returns>
        public Vector3 GetPoint(float normalizedLength)
        {
            normalizedLength = ClampedLength(normalizedLength);
            return DistortPoint(LineTransform.TransformPoint(GetPointInternal(normalizedLength)), normalizedLength);
        }

        /// <summary>
        /// Gets a point along the line at the specified length without using LineStartClamp or LineEndClamp
        /// </summary>
        /// <param name="normalizedLength"></param>
        /// <returns></returns>
        public Vector3 GetUnClampedPoint(float normalizedLength)
        {
            normalizedLength = Mathf.Clamp01(normalizedLength);
            return DistortPoint(LineTransform.TransformPoint(GetPointInternal(normalizedLength)), normalizedLength);
        }

        /// <summary>
        /// Gets a point along the line at the specified index
        /// </summary>
        /// <param name="pointIndex"></param>
        /// <returns></returns>
        public Vector3 GetPoint(int pointIndex)
        {
            if (pointIndex < 0 || pointIndex >= PointCount)
            {
                Debug.LogError("Invalid point index");
                return Vector3.zero;
            }

            return LineTransform.TransformPoint(GetPointInternal(pointIndex));
        }

        /// <summary>
        /// Sets a point in the line
        /// This function is not guaranteed to have an effect
        /// </summary>
        /// <param name="pointIndex"></param>
        /// <param name="point"></param>
        public void SetPoint(int pointIndex, Vector3 point)
        {
            if (pointIndex < 0 || pointIndex >= PointCount)
            {
                Debug.LogError("Invalid point index");
                return;
            }

            SetPointInternal(pointIndex, LineTransform.InverseTransformPoint(point));
        }

        private Vector3 DistortPoint(Vector3 point, float normalizedLength)
        {
            float strength = uniformDistortionStrength;

            if (distortionType == DistortionType.NormalizedLength)
            {
                strength = distortionStrength.Evaluate(normalizedLength);
            }

            for (int i = 0; i < distorters.Count; i++)
            {
                // Components may be added or removed
                if (distorters[i] != null)
                {
                    point = distorters[i].DistortPoint(point, strength);
                }
            }

            return point;
        }

        private float ClampedLength(float normalizedLength)
        {
            return Mathf.Lerp(Mathf.Max(lineStartClamp, 0.0001f), Mathf.Min(lineEndClamp, 0.9999f), Mathf.Clamp01(normalizedLength));
        }
    }
}