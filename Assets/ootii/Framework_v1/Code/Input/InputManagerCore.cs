using System;
using System.Collections;
using UnityEngine;

namespace com.ootii.Input
{
    /// <summary>
    /// Used by the InputManager to hook into the unity update process. This allows us
    /// to update the input and track old values
    /// </summary>
    [Serializable]
    public class InputManagerCore : MonoBehaviour
    {
        /// <summary>
        /// Saves state for the Xbox controller usage
        /// </summary>
        public bool _IsXboxControllerEnabled = false;
        public bool IsXboxControllerEnabled
        {
            get { return _IsXboxControllerEnabled; }

            set
            {
                _IsXboxControllerEnabled = value;
                InputManager.IsXboxControllerEnabled = value;
            }
        }

        /// <summary>
        /// Key or button used to allow movement to be activated
        /// 0 = none
        /// 1 = left mouse button
        /// 2 = right mouse button
        /// </summary>
        public int _MoveActivator = 0;
        public int MoveActivator
        {
            get { return _MoveActivator; }

            set
            {
                _MoveActivator = value;
                InputManager.MoveActivator = value;
            }
        }

        /// <summary>
        /// Key or button used to allow view to be activated
        /// 0 = none
        /// 1 = left mouse button
        /// 2 = right mouse button
        /// </summary>
        public int _ViewActivator = 0;
        public int ViewActivator
        {
            get { return _ViewActivator; }

            set
            {
                _ViewActivator = value;
                InputManager.ViewActivator = value;
            }
        }

        /// <summary>
        /// Raised first when the object comes into existance. Called
        /// even if script is not enabled.
        /// </summary>
        void Awake()
        {
            // Don't destroyed automatically when loading a new scene
            DontDestroyOnLoad(gameObject);

            // Initialize the manager
            InputManager.Initialize();
        }

        /// <summary>
        /// Called after the Awake() and before any update is called.
        /// </summary>
        public IEnumerator Start()
        {
            // Force the xbox value
            InputManager.IsXboxControllerEnabled = _IsXboxControllerEnabled;
            InputManager.MoveActivator = _MoveActivator;
            InputManager.ViewActivator = _ViewActivator;

            // Create the coroutine here so we don't re-create over and over
            WaitForEndOfFrame lWaitForEndOfFrame = new WaitForEndOfFrame();

            // Loop endlessly so we can process the input
            // at the end of each frame, preparing for the next
            while (true)
            {
                yield return lWaitForEndOfFrame;
                InputManager.Update();
            }
        }

        /// <summary>
        /// Called when the InputManager is disabled. We use this to
        /// clean up objects that were created.
        /// </summary>
        public void OnDisable()
        {
        }
    }
}
