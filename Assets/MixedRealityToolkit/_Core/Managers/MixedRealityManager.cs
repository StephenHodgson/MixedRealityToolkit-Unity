﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Definitions;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces;
using Microsoft.MixedReality.Toolkit.Internal.Interfaces.InputSystem;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Internal.Managers
{
    /// <summary>
    /// The Mixed Reality manager is responsible for coordinating the operation of the Mixed Reality Toolkit.
    /// It provides a service registry for all active managers that are used within a project as well as providing the active configuration profile for the project.
    /// The Profile can be swapped out at any time to meet the needs of your project.
    /// </summary>
    public class MixedRealityManager : MonoBehaviour
    {
        #region Mixed Reality Manager Profile configuration

        /// <summary>
        /// The active profile of the Mixed Reality Manager which controls which components are active and their initial configuration.
        /// *Note configuration is used on project initialization or replacement, changes to properties while it is running has no effect.
        /// </summary>
        [SerializeField]
        [Tooltip("The current active configuration for the Mixed Reality project")]
        private MixedRealityConfigurationProfile activeProfile = null;

        /// <summary>
        /// The public property of the Active Profile, ensuring events are raised on the change of the configuration
        /// </summary>
        public MixedRealityConfigurationProfile ActiveProfile
        {
            get { return activeProfile; }
            set { activeProfile = value; if (resetOnProfileChange) gameObject.SetActive(true); ResetConfiguration(); }
        }

        /// <summary>
        /// When a configuration Profile is replaced with a new configuration, force all managers to reset and read the new values
        /// </summary>
        private void ResetConfiguration()
        {
            // Reset all active managers in the registry
            foreach (var manager in ActiveProfile.ActiveManagers)
            {
                manager.Value.Reset();
            }

            // Reset all registered runtime components
            foreach (var manager in MixedRealityComponents)
            {
                manager.Item2.Reset();
            }
        }

        /// <summary>
        /// If the configuration profile is changed, should all the registered managers be force to read the new profile
        /// *Note may give unexpected results if Managers are not forced to read the new profile
        /// </summary>
        [SerializeField]
        [Tooltip("The current active configuration for the Mixed Reality project")]
        private bool resetOnProfileChange = true;

        #endregion Mixed Reality Manager Profile configuration

        #region Mixed Reality runtime component registry

        /// <summary>
        /// Local component registry for the Mixed Reality Manager, to allow runtime use of the Manager.
        /// </summary>
        public List<Tuple<Type, IMixedRealityManager>> MixedRealityComponents { get; } = new List<Tuple<Type, IMixedRealityManager>>();

        private int mixedRealityComponentsCount = 0;

        #endregion

        #region Active SDK components

        /// <summary>
        /// The Active Controllers property lists all the controllers detected by the Mixed Reality manager on startup
        /// </summary>
        //[SerializeField]
        //[Tooltip("The collection of currently active / detected controllers")]
        //private Controller[] activeControllers = null;

        ///// <summary>
        ///// The Active Headset property maintains the Headsets/SDK detected by the Mixed Reality manager on startup
        ///// </summary>
        //[SerializeField]
        //[Tooltip("The currently active / detected Headset or SDK")]
        //private Headset activeHeadset = default(Headset);

        #endregion Active SDK components

        /// <summary>
        /// Function called when the instance is assigned.
        /// Once all managers are registered and properties updated, the Mixed Reality Manager will initialize all active managers.
        /// This ensures all managers can reference each other once started.
        /// </summary>
        private void Initialize()
        {
            #region ActiveSDK Discovery
            // TODO Microsoft.MixedReality.Toolkit - Active SDK Discovery
            #endregion ActiveSDK Discovery

            #region SDK Initialization
            // TODO Microsoft.MixedReality.Toolkit - SDK Initialization
            #endregion SDK Initialization

            #region Managers Initialization

            //If the Mixed Reality Manager is not configured, stop.
            if (!ActiveProfile)
            {
                Debug.LogError("No Mixed Reality Configuration Profile found, cannot initialize the Mixed Reality Manager");
                gameObject.SetActive(false);
                return;
            }

            //If the Input system has been selected for initialization in the Active profile, enable it in the project
            if (ActiveProfile.EnableInputSystem)
            {
                //Enable Input (example initializer)
                AddManager(typeof(IMixedRealityInputSystem), Activator.CreateInstance(ActiveProfile.InputSystem) as IMixedRealityInputSystem);
            }

            //If the Boundary system has been selected for initialization in the Active profile, enable it in the project
            if (ActiveProfile.EnableBoundarySystem)
            {
                //Enable Boundary (example initializer)
                AddManager(typeof(IMixedRealityBoundarySystem), new MixedRealityBoundaryManager());
            }

            //TODO should this be optional?
            //Sort the managers based on Priority
            var orderedManagers = ActiveProfile.ActiveManagers.OrderBy(m => m.Value.Priority).ToArray();
            ActiveProfile.ActiveManagers.Clear();
            foreach (var manager in orderedManagers)
            {
                AddManager(manager.Key, manager.Value);
            }

            //Initialize all managers
            foreach (var manager in ActiveProfile.ActiveManagers)
            {
                manager.Value.Initialize();
            }

            #endregion Managers Initialization
        }

        #region MonoBehaviour Implementation

        private static MixedRealityManager instance;

        /// <summary>
        /// Returns the Singleton instance of the classes type.
        /// If no instance is found, then we search for an instance in the scene.
        /// If more than one instance is found, we throw an error and no instance is returned.
        /// </summary>
        public static MixedRealityManager Instance
        {
            get
            {
                if (IsInitialized)
                {
                    return instance;
                }

                if (Application.isPlaying && !searchForInstance)
                {
                    return null;
                }

                MixedRealityManager[] objects = FindObjectsOfType<MixedRealityManager>();
                searchForInstance = false;

                if (objects.Length == 1)
                {
                    objects[0].InitializeInternal();
                    return instance;
                }

                Debug.LogError($"Expected exactly 1 MixedRealityManager but found {objects.Length}.");
                return null;
            }
        }

        /// <summary>
        /// Flag to search for instance the first time Instance property is called.
        /// Subsequent attempts will generally switch this flag false, unless the instance was destroyed.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static bool searchForInstance = true;

        /// <summary>
        /// Expose an assertion whether the MixedRealityManager class is initialized.
        /// </summary>
        public static void AssertIsInitialized()
        {
            Debug.Assert(IsInitialized, "The MixedRealityManager has not been initialized.");
        }

        /// <summary>
        /// Returns whether the instance has been initialized or not.
        /// </summary>
        public static bool IsInitialized => instance != null;

        /// <summary>
        /// Static function to determine if the MixedRealityManager class has been initialized or not.
        /// </summary>
        /// <returns></returns>
        public static bool ConfirmInitialized()
        {
            // ReSharper disable once UnusedVariable
            // Assigning the Instance to access is used Implicitly.
            MixedRealityManager access = Instance;
            return IsInitialized;
        }

        /// <summary>
        /// Lock property for the Mixed Reality Manager to prevent reinitialization
        /// </summary>
        private readonly object initializedLock = new object();

        private void InitializeInternal()
        {
            lock (initializedLock)
            {
                if (IsInitialized) { return; }

                instance = this;

                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(instance.transform.root);
                }

                Initialize();
            }
        }

        /// <summary>
        /// Base Awake method that sets the Singleton's unique instance.
        /// Called by Unity when initializing a MonoBehaviour.
        /// Scripts that extend Singleton should be sure to call base.Awake() unless they want
        /// lazy initialization
        /// </summary>
        private void Awake()
        {
            if (IsInitialized && instance != this)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(this);
                }
                else
                {
                    Destroy(this);
                }

                Debug.LogWarning($"Trying to instantiate a second instance of the Mixed Reality Manager. Additional Instance was destroyed");
            }
            else if (!IsInitialized)
            {
                InitializeInternal();
                searchForInstance = false;
            }
        }


        /// <summary>
        /// The MonoBehaviour OnEnable event, which is then circulated to all active managers
        /// </summary>
        private void OnEnable()
        {
            // Enable all active managers in the registry
            foreach (var manager in ActiveProfile.ActiveManagers)
            {
                manager.Value.Enable();
            }

            // Enable all registered runtime components
            foreach (var manager in MixedRealityComponents)
            {
                manager.Item2.Enable();
            }
        }

        /// <summary>
        /// The MonoBehaviour Update event, which is then circulated to all active managers
        /// </summary>
        private void Update()
        {
            //If the Mixed Reality Manager is not configured, stop.
            if (!ActiveProfile)
            {
                return;
            }

            // Update manager registry
            foreach (var manager in ActiveProfile.ActiveManagers)
            {
                manager.Value.Update();
            }

            //Update runtime component registry
            foreach (var manager in MixedRealityComponents)
            {
                manager.Item2.Update();
            }
        }

        /// <summary>
        /// The MonoBehaviour OnDisable event, which is then circulated to all active managers
        /// </summary>
        private void OnDisable()
        {
            if (!ActiveProfile)
            {
                return;
            }

            // Disable all active managers in the registry
            foreach (var manager in ActiveProfile.ActiveManagers)
            {
                manager.Value.Disable();
            }

            // Disable all registered runtime components
            foreach (var manager in MixedRealityComponents)
            {
                manager.Item2.Disable();
            }
        }

        /// <summary>
        /// The MonoBehaviour Destroy event, which is then circulated to all active managers prior to the Mixed Reality Manager being destroyed
        /// </summary>
        private void OnDestroy()
        {
            if (!ActiveProfile)
            {
                return;
            }

            // Destroy all active managers in the registry
            foreach (var manager in ActiveProfile.ActiveManagers)
            {
                manager.Value.Destroy();
            }

            // Destroy all registered runtime components
            foreach (var manager in MixedRealityComponents)
            {
                manager.Item2.Destroy();
            }

            if (instance == this)
            {
                instance = null;
                searchForInstance = true;
            }
        }

        #endregion MonoBehaviour Implementation

        #region Manager Container Management

        /// <summary>
        /// Add a new manager to the Mixed Reality Manager active Manager registry.
        /// </summary>
        /// <param name="type">The interface type for the system to be managed.  E.G. InputSystem, BoundarySystem</param>
        /// <param name="manager">The Instance of the manager class to register</param>
        public void AddManager(Type type, IMixedRealityManager manager)
        {
            if (!ActiveProfile)
            {
                Debug.LogError($"Unable to add a new {type.Name} Manager as the Mixed Reality manager has to Active Profile");
            }

            if (type == null) { throw new ArgumentNullException(nameof(type)); }
            if (manager == null) { throw new ArgumentNullException(nameof(manager)); }


            if (IsCoreManagerType(type))
            {
                if (GetManager(type) == null)
                {
                    ActiveProfile.ActiveManagers.Add(type, manager);
                }
            }
            else
            {
                MixedRealityComponents.Add(new Tuple<Type, IMixedRealityManager>(type, manager));
                manager.Initialize();
                mixedRealityComponentsCount = MixedRealityComponents.Count;
            }
        }

        /// <summary>
        /// Retrieve a manager from the Mixed Reality Manager active manager registry
        /// </summary>
        /// <param name="type">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem</param>
        /// <returns>The Mixed Reality manager of the specified type</returns>
        public IMixedRealityManager GetManager(Type type)
        {
            if (!ActiveProfile)
            {
                Debug.LogError($"Unable to add a new {type.Name} Manager as the Mixed Reality manager has to Active Profile");
                return null;
            }

            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            IMixedRealityManager manager;

            if (IsCoreManagerType(type))
            {
                ActiveProfile.ActiveManagers.TryGetValue(type, out manager);
            }
            else
            {
                GetComponentByType(type, out manager);
            }

            return manager;
        }

        /// <summary>
        /// Retrieve a manager from the Mixed Reality Manager active manager registry
        /// </summary>
        /// <param name="type">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem</param>
        /// <param name="managerName">Name of the specific manager</param>
        /// <returns>The Mixed Reality manager of the specified type</returns>
        public IMixedRealityManager GetManager(Type type, string managerName)
        {
            if (!ActiveProfile)
            {
                Debug.LogError($"Unable to add a new {type.Name} Manager as the Mixed Reality manager has to Active Profile");
                return null;
            }

            if (type == null) { throw new ArgumentNullException(nameof(type)); }
            if (string.IsNullOrEmpty(managerName)) { throw new ArgumentNullException(nameof(managerName)); }

            IMixedRealityManager manager = null;

            if (IsCoreManagerType(type))
            {
                ActiveProfile.ActiveManagers.TryGetValue(type, out manager);
            }
            else
            {
                GetComponentByTypeAndName(type, managerName, out manager);
            }
            return manager;
        }

        /// <summary>
        /// Retrieve all managers from the Mixed Reality Manager active manager registry for a given type and an optional name
        /// </summary>
        /// <param name="type">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem</param>
        /// <returns>An array of Managers that meet the search criteria</returns>
        public IEnumerable<IMixedRealityManager> GetManagers(Type type)
        {
            return GetManagers(type, "");
        }

        /// <summary>
        /// Retrieve all managers from the Mixed Reality Manager active manager registry for a given type and an optional name
        /// </summary>
        /// <param name="type">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem</param>
        /// <param name="managerName">Name of the specific manager</param>
        /// <returns>An array of Managers that meet the search criteria</returns>
        public List<IMixedRealityManager> GetManagers(Type type, string managerName)
        {
            if (!ActiveProfile)
            {
                Debug.LogError($"Unable to add a new {type.Name} Manager as the Mixed Reality manager has to Active Profile");
                return null;
            }

            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            List<IMixedRealityManager> managers = new List<IMixedRealityManager>();

            if (IsCoreManagerType(type))
            {
                foreach (var manager in ActiveProfile.ActiveManagers)
                {
                    if (manager.Key.Name == type.Name)
                    {
                        managers.Add(manager.Value);
                    }
                }
            }
            else
            {
                //If no name provided, return all components of the same type. Else return the type/name combination.
                if (string.IsNullOrWhiteSpace(managerName))
                {
                    for (int i = 0; i < mixedRealityComponentsCount; i++)
                    {
                        if (MixedRealityComponents[i].Item1.Name == type.Name)
                        {
                            managers.Add(MixedRealityComponents[i].Item2);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < mixedRealityComponentsCount; i++)
                    {
                        if (MixedRealityComponents[i].Item1.Name == type.Name && MixedRealityComponents[i].Item2.Name == managerName)
                        {
                            managers.Add(MixedRealityComponents[i].Item2);
                        }
                    }
                }
            }
            return managers;
        }

        /// <summary>
        /// Remove all managers from the Mixed Reality Manager active manager registry for a given type
        /// </summary>
        /// <param name="type">The interface type for the system to be removed.  E.G. InputSystem, BoundarySystem</param>
        public void RemoveManager(Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            if (IsCoreManagerType(type))
            {
                ActiveProfile.ActiveManagers.Remove(type);
            }
            else
            {
                MixedRealityComponents.RemoveAll(tuple => tuple.Item1.Name == type.Name);
            }
        }

        /// <summary>
        /// Remove managers from the Mixed Reality Manager active manager registry for a given type and name
        /// Name is only supported for Mixed Reality runtime components
        /// </summary>
        /// <param name="type">The interface type for the system to be removed.  E.G. InputSystem, BoundarySystem</param>
        /// <param name="managerName">The name of the manager to be removed. (Only for runtime components) </param>
        public void RemoveManager(Type type, string managerName)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }
            if (string.IsNullOrEmpty(managerName)) { throw new ArgumentNullException(nameof(managerName)); }

            if (IsCoreManagerType(type))
            {
                ActiveProfile.ActiveManagers.Remove(type);
            }
            else
            {
                MixedRealityComponents.RemoveAll(t => t.Item1.Name == type.Name && t.Item2.Name == managerName);
            }
        }

        /// <summary>
        /// Disable all managers in the Mixed Reality Manager active manager registry for a given type
        /// </summary>
        /// <param name="type">The interface type for the system to be removed.  E.G. InputSystem, BoundarySystem</param>
        public void DisableManager(Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            if (IsCoreManagerType(type))
            {
                GetManager(type).Disable();
            }
            else
            {
                foreach (var manager in GetManagers(type))
                {
                    manager.Disable();
                }
            }
        }

        /// <summary>
        /// Disable a specific manager from the Mixed Reality Manager active manager registry
        /// </summary>
        /// <param name="type">The interface type for the system to be removed.  E.G. InputSystem, BoundarySystem</param>
        /// <param name="managerName">Name of the specific manager</param>
        public void DisableManager(Type type, string managerName)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }
            if (string.IsNullOrEmpty(managerName)) { throw new ArgumentNullException(nameof(managerName)); }

            if (IsCoreManagerType(type))
            {
                GetManager(type).Disable();
            }
            else
            {
                foreach (var manager in GetManagers(type, managerName))
                {
                    manager.Disable();
                }
            }
        }

        /// <summary>
        /// Enable all managers in the Mixed Reality Manager active manager registry for a given type
        /// </summary>
        /// <param name="type">The interface type for the system to be removed.  E.G. InputSystem, BoundarySystem</param>
        public void EnableManager(Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            if (IsCoreManagerType(type))
            {
                GetManager(type).Enable();
            }
            else
            {
                foreach (var manager in GetManagers(type))
                {
                    manager.Enable();
                }
            }
        }

        /// <summary>
        /// Enable a specific manager from the Mixed Reality Manager active manager registry
        /// </summary>
        /// <param name="type">The interface type for the system to be removed.  E.G. InputSystem, BoundarySystem</param>
        /// <param name="managerName">Name of the specific manager</param>
        public void EnableManager(Type type, string managerName)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }
            if (string.IsNullOrEmpty(managerName)) { throw new ArgumentNullException(nameof(managerName)); }

            if (IsCoreManagerType(type))
            {
                GetManager(type).Enable();
            }
            else
            {
                foreach (var manager in GetManagers(type, managerName))
                {
                    manager.Enable();
                }
            }
        }

        /// <summary>
        /// Generic function used to retrieve a manager from the Mixed Reality Manager active manager registry
        /// </summary>
        /// <typeparam name="T">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem.
        /// *Note type should be the Interface of the system to be retrieved and not the class itself</typeparam>
        /// <returns>The instance of the manager class that is registered with the selected Interface</returns>
        public T GetManager<T>() where T : IMixedRealityManager
        {
            var manager = GetManager(typeof(T));

            if (manager == null)
            {
                return default(T);
            }

            return (T)manager;
        }

        /// <summary>
        /// Generic function used to interrogate the Mixed Reality Manager active manager registry for the existence of a manager
        /// </summary>
        /// <typeparam name="T">The interface type for the system to be retrieved.  E.G. InputSystem, BoundarySystem.
        /// *Note type should be the Interface of the system to be retrieved and not the class itself</typeparam>
        /// <returns>True, there is a manager registered with the selected interface, False, no manager found for that interface</returns>
        public bool ManagerExists<T>() where T : class
        {
            return GetManager(typeof(T)) != null;
        }

        private bool IsCoreManagerType(Type type)
        {
            return type == typeof(IMixedRealityInputSystem) ||
                   type == typeof(IMixedRealityBoundarySystem);
        }

        /// <summary>
        /// Retrieve the first component from the registry that meets the selected type
        /// </summary>
        /// <param name="type">Interface type of the component being requested</param>
        /// <param name="manager">return parameter of the function</param>
        private void GetComponentByType(Type type, out IMixedRealityManager manager)
        {
            manager = null;

            for (int i = 0; i < mixedRealityComponentsCount; i++)
            {
                if (MixedRealityComponents[i].Item1.Name == type.Name)
                {
                    manager = MixedRealityComponents[i].Item2;
                    break;
                }
            }
        }

        /// <summary>
        /// Retrieve the first component from the registry that meets the selected type and name
        /// </summary>
        /// <param name="type">Interface type of the component being requested</param>
        /// <param name="managerName">Name of the specific manager</param>
        /// <param name="manager">return parameter of the function</param>
        private void GetComponentByTypeAndName(Type type, string managerName, out IMixedRealityManager manager)
        {
            manager = null;

            for (int i = 0; i < mixedRealityComponentsCount; i++)
            {
                if (MixedRealityComponents[i].Item1.Name == type.Name && MixedRealityComponents[i].Item2.Name == managerName)
                {
                    manager = MixedRealityComponents[i].Item2;
                    break;
                }
            }
        }

        #endregion Manager Container Management
    }
}
