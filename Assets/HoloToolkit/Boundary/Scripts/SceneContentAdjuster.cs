﻿// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.Boundary;
using System.Collections;
using UnityEngine;

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#endif

namespace HoloToolkit.Unity
{
    public class SceneContentAdjuster : MonoBehaviour
    {
        private float contentHeightOffset = 1f;
        private int frameWaitHack = 0;

        private void Awake()
        {
#if UNITY_2017_2_OR_NEWER
            // A Stationary TrackingSpaceType doesn't need any changes for an object at height 0.
            if (XRDevice.GetTrackingSpaceType() == TrackingSpaceType.Stationary || !XRDevice.isPresent)
#else
            if (true)
#endif
            {
                Destroy(this);
            }
            else
            {
                StartCoroutine(SetContentHeight());
            }
        }

        private IEnumerator SetContentHeight()
        {
            if (frameWaitHack < 1)
            {
                // Not waiting a frame often caused the camera's position to be incorrect at this point. This seems like a Unity bug.
                frameWaitHack++;
                yield return null;
            }

#if UNITY_2017_2_OR_NEWER
            transform.position = new Vector3(transform.position.x, CameraCache.Main.transform.position.y, transform.position.z);
#endif
        }
    }
}