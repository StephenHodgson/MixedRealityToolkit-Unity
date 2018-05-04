﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Internal.EventDatum.Input
{
    public class DictationEventData : BaseInputEventData
    {
        /// <summary>
        /// String result of the current dictation.
        /// </summary>
        public string DictationResult { get; private set; }

        /// <summary>
        /// Audio Clip of the last Dictation recording Session.
        /// </summary>
        public AudioClip DictationAudioClip { get; private set; }

        public DictationEventData(UnityEngine.EventSystems.EventSystem eventSystem) : base(eventSystem) { }

        public void Initialize(IInputSource inputSource, string dictationResult, AudioClip dictationAudioClip = null, object[] tags = null)
        {
            BaseInitialize(inputSource, tags);
            DictationResult = dictationResult;
            DictationAudioClip = dictationAudioClip;
        }
    }
}
