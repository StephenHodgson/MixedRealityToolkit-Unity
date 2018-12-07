﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Definitions.BoundarySystem;
using Microsoft.MixedReality.Toolkit.Core.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.Events;
using UnityEngine;
using UnityEngine.Experimental.XR;

namespace Microsoft.MixedReality.Toolkit.Core.Interfaces.BoundarySystem
{
    /// <summary>
    /// Manager interface for a Boundary system in the Mixed Reality Toolkit
    /// All replacement systems for providing Boundary functionality should derive from this interface
    /// </summary>
    public interface IMixedRealityBoundarySystem : IMixedRealityEventSystem, IMixedRealityEventSource
    {
        /// <summary>
        /// The scale (ex: World Scale) of the experience.
        /// </summary>
        ExperienceScale Scale { get; set; }

        /// <summary>
        /// The height of the play space, in meters.
        /// </summary>
        /// <remarks>
        /// This is used to create a three dimensional boundary volume.
        /// </remarks>
        float BoundaryHeight { get; set; }

        /// <summary>
        /// Enable / disable floor rendering.
        /// </summary>
        bool ShowFloor { get; set; }

        /// <summary>
        /// The size at which to display the rectangular floor plane <see cref="GameObject"/>.
        /// </summary>
        Vector2 FloorScale { get; }

        /// <summary>
        /// The material to use for the floor <see cref="GameObject"/> when created by the boundary system.
        /// </summary>
        Material FloorMaterial { get; }

        /// <summary>
        /// Enable / disable play area rendering.
        /// </summary>
        bool ShowPlayArea { get; set; }

        /// <summary>
        /// The material to use for the rectangular play area <see cref="GameObject"/>.
        /// </summary>
        Material PlayAreaMaterial { get; }

        /// <summary>
        /// Enable / disable tracked area rendering.
        /// </summary>
        bool ShowTrackedArea { get; set; }

        /// <summary>
        /// The material to use for the boundary geometry <see cref="GameObject"/>.
        /// </summary>
        Material TrackedAreaMaterial { get; }

        /// <summary>
        /// Enable / disable boundary wall rendering.
        /// </summary>
        bool ShowBoundaryWalls { get; set; }

        /// <summary>
        /// The material to use for displaying the boundary geometry walls.
        /// </summary>
        Material BoundaryWallMaterial { get; }

        /// <summary>
        /// Enable / disable ceiling rendering.
        /// </summary>
        /// <remarks>
        /// The ceiling is defined as a <see cref="GameObject"/> positioned <see cref="BoundaryHeight"/> above the floor.
        /// </remarks>
        bool ShowBoundaryCeiling { get; set; }

        /// <summary>
        /// The material to use for displaying the boundary ceiling.
        /// </summary>
        Material BoundaryCeilingMaterial { get; }

        /// <summary>
        /// Two dimensional representation of the geometry of the boundary, as provided
        /// by the platform.
        /// </summary>
        /// <remarks>
        /// BoundaryGeometry should be treated as the outline of the player's space, placed
        /// on the floor.
        /// </remarks>
        Edge[] Bounds { get; }

        /// <summary>
        /// Indicates the height of the floor, in relation to the coordinate system origin.
        /// </summary>
        /// <remarks>
        /// If a floor has been located, FloorHeight.HasValue will be true, otherwise it will be false.
        /// </remarks>
        float? FloorHeight { get; }

        /// <summary>
        /// Determines if a location is within the specified area of the boundary space.
        /// </summary>
        /// <param name="location">The location to be checked.</param>
        /// <param name="boundaryType">The type of boundary space being checked.</param>
        /// <returns>True if the location is within the specified area of the boundary space.</returns>
        /// <remarks>
        /// Use:
        /// Boundary.Type.PlayArea for the inscribed volume
        /// Boundary.Type.TrackedArea for the area defined by the boundary edges.
        /// </remarks>
        bool Contains(Vector3 location, Boundary.Type boundaryType = Boundary.Type.TrackedArea);

        /// <summary>
        /// Returns the description of the inscribed rectangular bounds.
        /// </summary>
        /// <param name="center">The center of the rectangle.</param>
        /// <param name="angle">The orientation of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns>True if an inscribed rectangle was found in the boundary geometry, false otherwise.</returns>
        bool TryGetRectangularBoundsParams(out Vector2 center, out float angle, out float width, out float height);

        /// <summary>
        /// Gets the <see cref="GameObject"/> that represents the user's floor.
        /// </summary>
        /// <returns>The floor visualization object or null if one does not exist.</returns>
        GameObject GetFloorVisualization();

        /// <summary>
        /// Gets the <see cref="GameObject"/> that represents the user's play area.
        /// </summary>
        /// <returns>The play area visualization object or null if one does not exist.</returns>
        GameObject GetPlayAreaVisualization();

        /// <summary>
        /// Gets the <see cref="GameObject"/> that represents the user's tracked area.
        /// </summary>
        /// <returns>The tracked area visualization object or null if one does not exist.</returns>
        GameObject GetTrackedAreaVisualization();

        // todo: GetBoundaryWallVisualization();

        /// <summary>
        /// Gets the <see cref="GameObject"/> that represents the upper surface of the user's boundary.
        /// </summary>
        /// <returns>The boundary ceiling visualization object or null if one does not exist.</returns>
        GameObject GetBoundaryCeilingVisualization();
    }
}