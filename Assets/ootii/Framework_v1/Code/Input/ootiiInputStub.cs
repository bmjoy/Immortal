using System;
using UnityEngine;

namespace com.ootii.Input
{
    /// <summary>
    /// This class is a pass-through in order to abstract the input controls from the asset.
    /// Asset classes will call to the InputManagerStub functions and you can then create
    /// and customize your own InputManager class
    /// 
    /// If you're using the build-in InputManager or Easy Input (found on the Asset Store), you
    /// don't need to do anything else.
    /// 
    /// If you want to use a different input solution, simply modify the function contents below
    /// to be used with your specific setup
    /// </summary>
    public class ootiiInputStub
    {
        /// <summary>
        /// Set by an external object, it tracks the angle of the
        /// user input compared to the camera's forward direction
        /// Note that this info isn't reliable as objects using it 
        /// before it's set it will get float.NaN.
        /// </summary>
        public static float InputFromCameraAngle
        {
            get { return InputManager.InputFromCameraAngle; }
            set { InputManager.InputFromCameraAngle = value; }
        }

        /// <summary>
        /// Set by an external object, it tracks the angle of the
        /// user input compared to the avatars's forward direction
        /// Note that this info isn't reliable as objects using it 
        /// before it's set it will get float.NaN.
        /// </summary>
        public static float InputFromAvatarAngle
        {
            get { return InputManager.InputFromAvatarAngle; }
            set { InputManager.InputFromAvatarAngle = value; }
        }

        /// <summary>
        /// Retrieves horizontal movement from the the input
        /// </summary>
        public static float MovementX
        {
            get { return InputManager.MovementX; }
        }

        /// <summary>
        /// Retrieves vertical movement from the the input
        /// </summary>
        public static float MovementY
        {
            get { return InputManager.MovementY; }
        }

        /// <summary>
        /// Retrieves horizontal view movement from the the input
        /// </summary>
        public static float ViewX
        {
            get { return InputManager.ViewX; }
        }

        /// <summary>
        /// Retrieves vertical view movement from the the input
        /// </summary>
        public static float ViewY
        {
            get { return InputManager.ViewY; }
        }

        /// <summary>
        /// Determines if the player can freely look around
        /// </summary>
        public static bool IsFreeViewing
        {
            get { return InputManager.IsFreeViewing; }
        }

        /// <summary>
        /// Test if a specific key is pressed
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public static bool IsPressed(KeyCode rKey)
        {
            return InputManager.IsPressed(rKey);
        }

        /// <summary>
        /// Test if a specific key is pressed
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public static bool IsJustPressed(KeyCode rKey)
        {
            return InputManager.IsJustPressed(rKey);
        }

        /// <summary>
        /// Tests if a specific action is pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        public static bool IsPressed(string rAction)
        {
            return InputManager.IsPressed(rAction);
        }

        /// <summary>
        /// Tests if a specific action just occured this frame.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        public static bool IsJustPressed(string rAction)
        {
            return InputManager.IsJustPressed(rAction);
        }
    }
}
