using System;
using System.Collections;
using UnityEngine;
using com.ootii.Utilities.Debug;

namespace com.ootii.Input
{
    /// <summary>
    /// Simple class to consolidate inut
    /// </summary>
    public class InputManager
    {
        /// <summary>
        /// This stub is a game object that will update the input over time. The
        /// stub can be placed by the scene builder or generated automatically.
        /// </summary>
        public static InputManagerCore Core;

        /// <summary>
        /// Set by an external object, it tracks the angle of the
        /// user input compared to the camera's forward direction
        /// Note that this info isn't reliable as objects using it 
        /// before it's set it will get float.NaN.
        /// </summary>
        private static float mInputFromCameraAngle = 0f;
        public static float InputFromCameraAngle
        {
            get { return mInputFromCameraAngle; }
            set { mInputFromCameraAngle = value; }
        }

        /// <summary>
        /// Set by an external object, it tracks the angle of the
        /// user input compared to the avatars's forward direction
        /// Note that this info isn't reliable as objects using it 
        /// before it's set it will get float.NaN.
        /// </summary>
        private static float mInputFromAvatarAngle = 0f;
        public static float InputFromAvatarAngle
        {
            get { return mInputFromAvatarAngle; }
            set { mInputFromAvatarAngle = value; }
        }

        /// <summary>
        /// Retrieves horizontal movement from the the input
        /// </summary>
        public static float MovementX
        {
            get
            {
                if ((mMoveActivator == 0) ||
                    (mMoveActivator == 1 && UnityEngine.Input.GetMouseButton(0)) ||
                    (mMoveActivator == 2 && UnityEngine.Input.GetMouseButton(1)))
                {
                    return UnityEngine.Input.GetAxis("Horizontal");
                }

                return 0f;    
            }
        }

        /// <summary>
        /// Retrieves vertical movement from the the input
        /// </summary>
        public static float MovementY
        {
            get
            {
                if ((mMoveActivator == 0) ||
                    (mMoveActivator == 1 && UnityEngine.Input.GetMouseButton(0)) ||
                    (mMoveActivator == 2 && UnityEngine.Input.GetMouseButton(1)))
                {
                    return UnityEngine.Input.GetAxis("Vertical");
                }

                return 0f;
            }
        }

        /// <summary>
        /// Retrieves horizontal view movement from the the input
        /// </summary>
        public static float ViewX
        {
            get
            {
                float lView = 0f;
                if (mIsXboxControllerEnabled)
                {
                    lView = UnityEngine.Input.GetAxisRaw("WXRightStickX");
                    if (lView != 0f) { return lView; }
                }

                // Mouse
                if ((mViewActivator == 0) ||
                    (mViewActivator == 1 && UnityEngine.Input.GetMouseButton(0)) ||
                    (mViewActivator == 2 && UnityEngine.Input.GetMouseButton(1)))
                {
                    lView = mViewX;
                }

                return lView;
            }
        }

        /// <summary>
        /// Retrieves vertical view movement from the the input
        /// </summary>
        public static float ViewY
        {
            get
            {
                float lView = 0f;

                if (mIsXboxControllerEnabled)
                {
                    lView = UnityEngine.Input.GetAxisRaw("WXRightStickY");
                    if (lView != 0f) { return lView; }
                }

                // Mouse
                if ((mViewActivator == 0) ||
                    (mViewActivator == 1 && UnityEngine.Input.GetMouseButton(0)) ||
                    (mViewActivator == 2 && UnityEngine.Input.GetMouseButton(1)))
                {                
                    lView = mViewY;
                }

                return lView;
            }
        }

        /// <summary>
        /// Determines if the player can freely look around
        /// </summary>
        public static bool IsFreeViewing
        {
            get { return true; }
        }

        /// <summary>
        /// Determines if the XBox controller is enabled and we should
        /// test for it
        /// </summary>
        private static bool mIsXboxControllerEnabled = false;
        public static bool IsXboxControllerEnabled
        {
            get { return mIsXboxControllerEnabled; }
            set { mIsXboxControllerEnabled = value; }
        }

        /// <summary>
        /// Key or button used to allow movement to be activated
        /// 0 = none
        /// 1 = left mouse button
        /// 2 = right mouse button
        /// </summary>
        private static int mMoveActivator = 0;
        public static int MoveActivator
        {
            get { return mMoveActivator; }
            set { mMoveActivator = value; }
        }

        /// <summary>
        /// Key or button used to allow view to be activated
        /// 0 = none
        /// 1 = left mouse button
        /// 2 = right mouse button
        /// </summary>
        private static int mViewActivator = 2;
        public static int ViewActivator
        {
            get { return mViewActivator; }
            set { mViewActivator = value; }
        }

        private static float mMouseSensativity = 2f;

        private static float mViewX = 0f;
        private static float mViewY = 0f;

        private static bool mOldLTrigger = false;
        private static bool mOldRTrigger = false;

        private static float mVSyncTimer = 0f;

        /// <summary>
        /// Static constructor is called at most one time, before any 
        /// instance constructor is invoked or member is accessed. 
        /// </summary>
        static InputManager()
        {
            // Check to see if an input manager stub exists. If so, associate it.
            // if not, we'll need to create one.
            Core = Component.FindObjectOfType<InputManagerCore>();
            if (Core == null)
            {
#pragma warning disable 0414
                Core = (new GameObject("InputManagerCore")).AddComponent<InputManagerCore>();
                mIsXboxControllerEnabled = Core.IsXboxControllerEnabled;
#pragma warning restore 0414
            }
        }

        /// <summary>
        /// Set the initial values
        /// </summary>
        public static void Initialize()
        {
            if (Core != null)
            {
                mIsXboxControllerEnabled = Core.IsXboxControllerEnabled;
            }
        }

        /// <summary>
        /// Grab and process information from the input in one place. This
        /// allows us to calculated changes over time too.
        /// </summary>
        public static void Update()
        {
            mInputFromCameraAngle = float.NaN;
            mInputFromAvatarAngle = float.NaN;

            float lViewX = 0f;
            float lViewY = 0f;
            float lMouseSensativity = mMouseSensativity;
            if (UnityEngine.QualitySettings.vSyncCount == 0) { lMouseSensativity = lMouseSensativity * 2f; }

            if ((mViewActivator == 0) ||
                (mViewActivator == 1 && UnityEngine.Input.GetMouseButton(0)) ||
                (mViewActivator == 2 && UnityEngine.Input.GetMouseButton(1)))
            {
                lViewX = UnityEngine.Input.GetAxis("Mouse X") * lMouseSensativity;
                if (lViewX != 0)
                {
                    mViewX = lViewX;
                    mVSyncTimer = 0f;
                }

                lViewY = UnityEngine.Input.GetAxis("Mouse Y") * lMouseSensativity;
                if (lViewY != 0)
                {
                    mViewY = lViewY;
                    mVSyncTimer = 0f;
                }
            }

            if (lViewX == 0f && lViewY == 0f)
            {
                mVSyncTimer += Time.deltaTime;
                if (mVSyncTimer > Time.fixedDeltaTime)
                {
                    mViewX = 0f;
                    mViewY = 0f;
                    mVSyncTimer = 0f;
                }
            }
        }

        /// <summary>
        /// Test if a specific key is pressed
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public static bool IsPressed(KeyCode rKey)
        {
            return UnityEngine.Input.GetKey(rKey);
        }

        /// <summary>
        /// Test if a specific key is pressed
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public static bool IsJustPressed(KeyCode rKey)
        {
            return UnityEngine.Input.GetKeyUp(rKey);
        }

        /// <summary>
        /// Tests if a specific action is pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        public static bool IsPressed(string rAction)
        {
            // Determines if the character should go into a first-person
            // perspective for targeting
            if (rAction == "Aiming")
            {
                if (UnityEngine.Input.GetMouseButton(2)) { return true; }
                if (mIsXboxControllerEnabled && UnityEngine.Input.GetAxis("WXLeftTrigger") > 0.5f) { return true; }
            }
            // Determines if the character should sprint forward
            else if (rAction == "Sprint")
            {
                if (UnityEngine.Input.GetKey(KeyCode.LeftShift)) { return true; }
                if (mIsXboxControllerEnabled && UnityEngine.Input.GetButton("WXButton3")) { return true; }
            }
            // Determines if the player is taking action to move the character to the left. 
            // Use this to trigger special case movement, like climbing (after the motion controller sets it!)
            else if (rAction == "MoveLeft")
            {
                if (float.IsNaN(mInputFromAvatarAngle)) { return false; }
                if (mInputFromAvatarAngle <= -45 && mInputFromAvatarAngle >= -135) { return true; }
            }
            // Determines if the player is taking action to move the character to the right. 
            // Use this to trigger special case movement, like climbing (after the motion controller sets it!)
            else if (rAction == "MoveRight")
            {
                if (float.IsNaN(mInputFromAvatarAngle)) { return false; }
                if (mInputFromAvatarAngle >= 45 && mInputFromAvatarAngle <= 135) { return true; }
            }
            // Determines if the player is taking action to move the character up or forward. 
            // Use this to trigger special case movement, like climbing (after the motion controller sets it!)
            else if (rAction == "MoveUp")
            {
                if (float.IsNaN(mInputFromAvatarAngle)) { return false; }
                if (mInputFromAvatarAngle > -45 && mInputFromAvatarAngle < 45) { return true; }
            }
            // Determines if the player is taking action to move the character down or backwards. 
            // Use this to trigger special case movement, like climbing (after the motion controller sets it!)
            else if (rAction == "MoveDown")
            {
                if (float.IsNaN(mInputFromAvatarAngle)) { return false; }
                if (mInputFromAvatarAngle < -135 || mInputFromAvatarAngle > 135) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Tests if a specific action just occured this frame.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        public static bool IsJustPressed(string rAction)
        {
            if (rAction == "Jump")
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) { return true; }
                if (mIsXboxControllerEnabled && UnityEngine.Input.GetButtonDown("WXButton0")) { return true; }
            }
            else if (rAction == "Aiming")
            {
                if (UnityEngine.Input.GetMouseButton(2)) 
                {
                    if (!mOldLTrigger)
                    {
                        mOldLTrigger = true;
                        return true;
                    }
                }
                else if (mIsXboxControllerEnabled && UnityEngine.Input.GetAxis("WXLeftTrigger") > 0.5f)
                {
                    if (!mOldLTrigger)
                    {
                        mOldLTrigger = true;
                        return true;
                    }
                }
                else
                {
                    mOldLTrigger = false;
                }
            }
            else if (rAction == "Release")
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.LeftShift)) { return true; }
                if (mIsXboxControllerEnabled && UnityEngine.Input.GetButtonDown("WXButton3")) { return true; }
            }
            else if (rAction == "ChangeStance")
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.T)) { return true; }

                if (mIsXboxControllerEnabled && UnityEngine.Input.GetAxis("WXRightTrigger") > 0.5f)
                {
                    if (!mOldRTrigger)
                    {
                        mOldRTrigger = true;
                        return true;
                    }
                }
                else
                {
                    mOldRTrigger = false;
                }
            }
            else if (rAction == "PrimaryAttack")
            {
                if (UnityEngine.Input.GetMouseButtonDown(0)) { return true; }
                if (mIsXboxControllerEnabled && UnityEngine.Input.GetButtonDown("WXButton1")) { return true; }
            }
            else if (rAction == "Sprint")
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.LeftShift)) { return true; }
                if (mIsXboxControllerEnabled && UnityEngine.Input.GetButtonDown("WXButton3")) { return true; }
            }

            return false;
        }
    }
}
