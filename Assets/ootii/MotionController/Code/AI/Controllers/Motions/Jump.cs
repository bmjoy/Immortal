using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using com.ootii.Base;
using com.ootii.Cameras;
using com.ootii.Helpers;
using com.ootii.Input;
using com.ootii.Utilities.Debug;

namespace com.ootii.AI.Controllers
{
    /// <summary>
    /// Standing or running jump. The jump allows for control
    /// while in the air.
    /// </summary>
    [MotionTooltip("A physics based multi-part jump that allows the player to launch into the " +
                   "air and recover into the idle pose or a run. The jump is created so the avatar " +
                   "can jump as high as mass, gravity, and impulse allow.")]
    public class Jump : MotionControllerMotion
    {
        // Enum values for the motion
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_LAUNCH = 201; //1
        public const int PHASE_RISE = 202; //2
        public const int PHASE_RISE_TO_TOP = 203; //3
        public const int PHASE_TOP = 204; //4
        public const int PHASE_TOP_TO_FALL = 205; //5
        public const int PHASE_FALL = 206; //6
        public const int PHASE_LAND = 207; //7
        public const int PHASE_RECOVER_TO_IDLE = 208; //8
        public const int PHASE_RECOVER_TO_RUN = 209; //9
        public const int PHASE_START_FALL = 250; // 50
        public const int PHASE_START_JUMP = 251; // 51

        /// <summary>
        /// Stores hit info so we don't have to keep allocating
        /// </summary>
        private static RaycastHit sHitInfo;

        /// <summary>
        /// The amount of force caused by the player jumping
        /// </summary>
        [SerializeField]
        protected float mImpulse = 31f;

        [MotionTooltip("Amount of instant force applied to raise the avatar up. A value of 31 is good for a mass of 5 (human).")]
        public float Impulse
        {
            get { return mImpulse; }
            set { mImpulse = value; }
        }

        /// <summary>
        /// Use the launch velocity throughout the jump
        /// </summary>
        [SerializeField]
        protected bool mMomentumEnabled = false;

        [MotionTooltip("Determines if the avatar's speed and direction before the jump are used to propel the avatar while in the air.")]
        public bool MomentumEnabled
        {
            get { return mMomentumEnabled; }
            set { mMomentumEnabled = value; }
        }

        /// <summary>
        /// Determines if the player can control the avatar movement
        /// and rotation while in the air.
        /// </summary>
        [SerializeField]
        protected bool mControlEnabled = true;

        [MotionTooltip("Determines if the player can control the avatar while in the air.")]
        public bool ControlEnabled
        {
            get { return mControlEnabled; }
            set { mControlEnabled = value; }
        }

        /// <summary>
        /// When in air, the player can still move the avatar. This
        /// value is the max speed the player can move the avatar by.
        /// </summary>
        [SerializeField]
        protected float mMovementSpeed = 5f;

        [MotionTooltip("Speed of the avatar when in the air. This should roughly match the ground speed of the avatar.")]
        public float MovementSpeed
        {
            get { return mMovementSpeed; }
            set { mMovementSpeed = value; }
        }

        /// <summary>
        /// Minumum degrees the player needs to turn in order for
        /// the avatar to start rotating.
        /// </summary>
        [SerializeField]
        protected float mRotationMin = 0f;

        [MotionTooltip("Minimum degree the player needs to rotate the avatar before the avatar actually starts to turn. Otherwise, the avatar simply moves in that direction.")]
        public float RotationMin
        {
            get { return mRotationMin; }
            set { mRotationMin = value; }
        }

        /// <summary>
        /// When in air, the player can still rotate the avatar. This
        /// value is the max speed the player can rotate the avatar by.
        /// </summary>
        [SerializeField]
        protected float mRotationSpeed = 10f;

        [MotionTooltip("Determines how quickly the avatar rotates while in the air. This would be in degrees per second.")]
        public float RotationSpeed
        {
            get { return mRotationSpeed; }
            set { mRotationSpeed = value; }
        }

        /// <summary>
        /// If the value is great than 0, we'll do a check to see if there
        /// is enough room to even attempt a jump. While in a jump, we'll cancel it
        /// if there isn't enough room
        /// </summary>
        protected float mRequiredOverheadDistance = 0f;

        [MotionTooltip("When greater than 0, a test will be made to determine if we can jump or can continue a jump.")]
        public float RequiredOverheadDistance
        {
            get { return mRequiredOverheadDistance; }
            set { mRequiredOverheadDistance = value; }
        }

        /// <summary>
        /// Forward the player was facing when they launched. It helps
        /// us control the total rotation that can happen in the air.
        /// </summary>
        protected Vector3 mLaunchForward = Vector3.zero;

        /// <summary>
        /// Tracks the camera mode as we move in and out for the jump
        /// </summary>
        protected int mSavedCameraMode = EnumCameraMode.THIRD_PERSON_FOLLOW;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Jump()
            : base()
        {
            _Priority = 10;
            mIsStartable = true;
            mIsNavMeshChangeExpected = true;
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public Jump(MotionController rController)
            : base(rController)
        {
            _Priority = 10;
            mIsStartable = true;
            mIsNavMeshChangeExpected = true;
        }

        /// <summary>
        /// Preprocess any animator data so the motion can use it later
        /// </summary>
        public override void LoadAnimatorData()
        {
#if UNITY_4_0 || UNITY_4_0_1 ||UNITY_4_1|| UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
            string lBaseLayer = "";
#else
            string lBaseLayer = "Base Layer.";
#endif

            mController.AddAnimatorName("Entry -> " + lBaseLayer + "Jump-SM.JumpRise");
            mController.AddAnimatorName("AnyState -> " + lBaseLayer + "Jump-SM.JumpRise");

            mController.AddAnimatorName("Entry -> " + lBaseLayer + "Jump-SM.JumpFallPose");
            mController.AddAnimatorName("AnyState -> " + lBaseLayer + "Jump-SM.JumpFallPose");

            mController.AddAnimatorName(lBaseLayer + "Idle-SM.Idle_Casual -> " + lBaseLayer + "Jump-SM.JumpRise");

            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpRise");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpRise -> " + lBaseLayer + "Jump-SM.JumpRiseToTop");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpRise -> " + lBaseLayer + "Jump-SM.JumpRisePose");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpRisePose");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpRisePose -> " + lBaseLayer + "Jump-SM.JumpRiseToTop");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpRiseToTop");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpRiseToTop -> " + lBaseLayer + "Jump-SM.JumpTopToFall");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpTopPose");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpTopToFall");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpTopToFall -> " + lBaseLayer + "Jump-SM.JumpLandStand");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpFallPose");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpLand");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpLand -> " + lBaseLayer + "Jump-SM.JumpRecoverIdle");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpLand -> " + lBaseLayer + "Jump-SM.JumpRecoverRun");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpRecoverIdle");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpRecoverIdle -> " + lBaseLayer + "Idle-SM.idle_Alert");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpRecoverIdle -> " + lBaseLayer + "Idle-SM.idle_Casual");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpRecoverRun");
            mController.AddAnimatorName(lBaseLayer + "Jump-SM.JumpRecoverRun -> AdventureForward-SM.RunForward");
        }

        /// <summary>
        /// Tests if this motion should be started. However, the motion
        /// isn't actually started.
        /// </summary>
        /// <returns></returns>
        public override bool TestActivate()
        {
            if (!mIsStartable) { return false; }
            if (!mController.UseInput) { return false; }

            if (ootiiInputStub.IsJustPressed("Jump"))
            {
                ControllerState lState = mController.State;
                if (lState.IsGrounded)
                {
                    if (lState.Stance != EnumControllerStance.COMBAT_RANGED)
                    {
                        // Perform an upward raycast to determine if something is overhead. If it is, we need
                        // to prevent or stop a jump
                        if (mRequiredOverheadDistance > 0)
                        {
                            if (mController.SafeRaycastAll(mController.transform.position, Vector3.up, ref sHitInfo, mRequiredOverheadDistance))
                            {
                                return false;
                            }
                        }  
                        
                        // We can get to the launch if we jump while we are ending a previous jump
                        if (mPhase == Jump.PHASE_LAUNCH || mPhase == Jump.PHASE_START_JUMP)
                        {
                            return true;
                        }

#if UNITY_4_0 || UNITY_4_0_1 ||UNITY_4_1|| UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
                        string lBaseLayer = "";
#else
                        string lBaseLayer = "Base Layer.";
#endif

                        // If we're not in the middle of a jump, let it happen
                        if (!IsInJumpState ||
                            mController.CompareAnimatorStateName(mAnimatorLayerIndex, lBaseLayer + "Jump-SM.JumpLand") ||
                            mController.CompareAnimatorStateName(mAnimatorLayerIndex, lBaseLayer + "Jump-SM.JumpRecoverIdle") ||
                            mController.CompareAnimatorStateName(mAnimatorLayerIndex, lBaseLayer + "Jump-SM.JumpRecoverRun"))
                        {
                            return true;
                        }
                    }
                }
            }
                
            return false;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion
        /// is typically disabled
        /// </summary>
        /// <returns></returns>
        public override bool TestUpdate()
        {
            if (mIsActivatedFrame) { return true; }
            if (mMotionLayer.ActiveMotionDuration > 5f) { return false; }

            // Perform an upward raycast to determine if something is overhead. If it is, we need
            // to prevent or stop a jump
            if (mRequiredOverheadDistance > 0)
            {
                if (mController.SafeRaycastAll(mController.transform.position, Vector3.up, ref sHitInfo, mRequiredOverheadDistance))
                {
                    return false;
                }
            } 
            
            return true;
        }

        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public override bool Activate(MotionControllerMotion rPrevMotion)
        {
            // Force the camera so we can do side jumps
            if (mController.UseInput && mController.CameraRig != null)
            {
                mSavedCameraMode = mController.CameraRig.Mode;
                mController.CameraRig.TransitionToMode(EnumCameraMode.THIRD_PERSON_FIXED);
            }

            // Flag the motion as active
            mIsActive = true;
            mIsActivatedFrame = true;
            mIsStartable = false;
            mLaunchForward = mController.transform.forward;

            // Initialize the jump
            mPhase = Jump.PHASE_START_JUMP;
            mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_START_JUMP, true);

            // Remove any accumulated velocity so that gravity can start over
            mController.AccumulatedVelocity = new Vector3(mController.AccumulatedVelocity.x, 0f, mController.AccumulatedVelocity.z);

            // Set the ground velocity so that we can keep momentum going
            ControllerState lState = mController.State;
            lState.IsGrounded = false;
            lState.GroundLaunchVelocity = (mController.transform.rotation * mController.RootMotionVelocityAvg.Average);
            mController.State = lState;

            // Create the total impulse (direction + magnitude)
            Vector3 lImpulse = mController.transform.up * mImpulse;
            mController.AddImpulse(lImpulse);

            // Report that we're good to enter the jump
            return true;
        }

        /// <summary>
        /// Called to stop the motion. If the motion is stopable. Some motions
        /// like jump cannot be stopped early
        /// </summary>
        public override void Deactivate()
        {
            mPhase = Jump.PHASE_UNKNOWN;

            // TT 7/26/14 - Removed so jump can finish while grabbing ledge
            //mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_UNKNOWN);

            mIsActive = false;
            mIsStartable = true;
            mDeactivationTime = Time.time;

            mVelocity = Vector3.zero;
            if (mController.UseInput && mController.CameraRig != null) { mController.CameraRig.TransitionToMode(mSavedCameraMode); }
        }

        /// <summary>
        /// Allows the motion to modify the ground and support information
        /// </summary>
        /// <param name="rState">Current state whose support info can be modified</param>
        /// <returns>Boolean that determines if the avatar is grounded</returns>
        public override bool DetermineGrounding(ref ControllerState rState)
        {
            // If we've just started the jump, force the grounded flag off
            if (mIsActive && mPhase == Jump.PHASE_START_JUMP)
            {
                return false;
            }
            else
            {
                return rState.IsGrounded;
            }
        }

        /// <summary>
        /// Updates the motion over time. This is called by the controller
        /// every update cycle so animations and stages can be updated.
        /// </summary>
        public override void UpdateMotion()
        {
            ControllerState lState = mController.State;
            float lStateTime = mController.State.AnimatorStates[mAnimatorLayerIndex].StateInfo.normalizedTime;

#if UNITY_4_0 || UNITY_4_0_1 ||UNITY_4_1|| UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
            string lBaseLayer = "";
#else
            string lBaseLayer = "Base Layer.";
#endif


            // If we've moved out of the jump motions, disable this motion
            if (!TestUpdate() || mMotionLayer.ActiveMotion != this)
            {
                mIsActive = false;
                mIsStartable = true;

                mVelocity = Vector3.zero;
                if (mController.UseInput && mController.CameraRig != null) { mController.CameraRig.TransitionToMode(mSavedCameraMode); }

                return;
            }

            // This is the start of the jump. The animator will automatically move on after the node
            // has finished. However, it could move to the "JumpRisePose" or "JumpRiseToTop"
            if (mController.CompareAnimatorStateName(mAnimatorLayerIndex, lBaseLayer + "Jump-SM.JumpRise"))
            {
                // Shrink the collider as it rises
                mController.SetColliderHeightAtBase(mController.ActorHeight * 0.83f);

                // If our velocity is trailing off, move to the top position
                if (lState.Velocity.y < 4.0f)
                {
                    if (mPhase != Jump.PHASE_RISE_TO_TOP)
                    {
                        mPhase = Jump.PHASE_RISE_TO_TOP;
                        mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_RISE_TO_TOP);
                    }
                }
                // Otherwise, move to the pose. We need to ensure we're out
                // of the AnyState condition
                else if (mPhase == Jump.PHASE_START_JUMP)
                { 
                    mPhase = Jump.PHASE_LAUNCH;
                    mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_LAUNCH);
                }
            }
            // This is the holding position for a super high jump. The pose gives us time
            // before the top occurs.
            else if (mController.CompareAnimatorStateName(mAnimatorLayerIndex, lBaseLayer + "Jump-SM.JumpRisePose"))
            {
                // Shrink the collider as it rises
                mController.SetColliderHeightAtBase(mController.ActorHeight * 0.83f);

                // If our velocity is trailing off, move to the top position
                if (lState.Velocity.y < 4.0f)
                {
                    if (mPhase != Jump.PHASE_RISE_TO_TOP)
                    {
                        mPhase = Jump.PHASE_RISE_TO_TOP;
                        mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_RISE_TO_TOP);
                    }
                }
            }
            // At this point, we're close to the peak of the jump and we need to transition
            // into the top position.
            else if (mController.CompareAnimatorStateName(mAnimatorLayerIndex, lBaseLayer + "Jump-SM.JumpRiseToTop"))
            {
                // Shrink the collider as it peaks
                mController.SetColliderHeightAtBase(mController.ActorHeight * 0.77f);

                // It's possible that the jump moved us over higher ground. If we're
                // too close to the ground, simply move into the recover.
                if (lState.GroundDistance < 0.5f) // || (lState.Velocity.y < -2f && lState.GroundDistance < 0.45f))
                {
                    if (lState.InputMagnitudeTrend.Value == 0f && lState.GroundLaunchVelocity.sqrMagnitude == 0)
                    {
                        if (mPhase != Jump.PHASE_RECOVER_TO_IDLE)
                        {
                            mPhase = Jump.PHASE_RECOVER_TO_IDLE;
                            mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_RECOVER_TO_IDLE);
                        }
                    }
                    else
                    {
                        if (Mathf.Abs(lState.InputFromAvatarAngle) > 140)
                        {
                            lState.GroundLaunchVelocity = Vector3.zero;
                            if (mPhase != Jump.PHASE_RECOVER_TO_IDLE)
                            {
                                mPhase = Jump.PHASE_RECOVER_TO_IDLE;
                                mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_RECOVER_TO_IDLE);
                            }
                        }
                        else if (mPhase != Jump.PHASE_RECOVER_TO_RUN)
                        {
                            mPhase = Jump.PHASE_RECOVER_TO_RUN;
                            mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_RECOVER_TO_RUN);
                        }
                    }
                }
                // If we slow down, start moving to the fall position
                else if (lState.Velocity.y < -1.0f) // || lState.GroundDistance < 0.45f)
                {
                    if (mPhase != Jump.PHASE_TOP_TO_FALL)
                    {
                        mPhase = Jump.PHASE_TOP_TO_FALL;
                        mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_TOP_TO_FALL);
                    }
                }
            }
            // We should be at the peak of the jump. We don't expect to wait here
            // long, but this gives us a "pose" to hold onto if needed
            else if (mController.CompareAnimatorStateName(mAnimatorLayerIndex, lBaseLayer + "Jump-SM.JumpTopPose"))
            {
                // Shrink the collider as it peaks
                mController.SetColliderHeightAtBase(mController.ActorHeight * 0.72f);

                // We may have moved over something during the jump. If so, 
                // we can move straight to the recover
                if (lState.GroundDistance < 0.5f)
                {
                    if (lState.InputMagnitudeTrend.Value == 0f && lState.GroundLaunchVelocity.sqrMagnitude == 0)
                    {
                        if (mPhase != Jump.PHASE_RECOVER_TO_IDLE)
                        {
                            mPhase = Jump.PHASE_RECOVER_TO_IDLE;
                            mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_RECOVER_TO_IDLE);
                        }
                    }
                    else
                    {
                        if (Mathf.Abs(lState.InputFromAvatarAngle) > 140)
                        {
                            lState.GroundLaunchVelocity = Vector3.zero;
                            if (mPhase != Jump.PHASE_RECOVER_TO_IDLE)
                            {
                                mPhase = Jump.PHASE_RECOVER_TO_IDLE;
                                mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_RECOVER_TO_IDLE);
                            }
                        }
                        else if (mPhase != Jump.PHASE_RECOVER_TO_RUN)
                        {
                            mPhase = Jump.PHASE_RECOVER_TO_RUN;
                            mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_RECOVER_TO_RUN);
                        }
                    }
                }
                // If we've reached the fall speed, transition
                else if (lState.Velocity.y < -1.0f)
                {
                    if (mPhase != Jump.PHASE_TOP_TO_FALL)
                    {
                        mPhase = Jump.PHASE_TOP_TO_FALL;
                        mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_TOP_TO_FALL);
                    }
                }
                // Otherwise, ensure we're in the right phase
                else if (mPhase == Jump.PHASE_RISE_TO_TOP)
                {
                    mPhase = Jump.PHASE_TOP;
                    mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_TOP);
                }
            }
            // Here we come out of the top pose and start moving into the fall pose
            else if (mController.CompareAnimatorStateName(mAnimatorLayerIndex, lBaseLayer + "Jump-SM.JumpTopToFall"))
            {
                // Grow the collider as it falls
                if (lStateTime < 0.5f)
                {
                    mController.SetColliderHeightAtBase(mController.ActorHeight * 0.72f);
                }
                else
                {
                    mController.SetColliderHeightAtBase(mController.ActorHeight * 0.94f);
                }

                // If we got ontop of something, we may need to recover.
                if (lState.GroundDistance < 0.15f)
                {
                    // If we're not moving, we can head to the idle
                    if (lState.InputMagnitudeTrend.Value == 0f && lState.GroundLaunchVelocity.sqrMagnitude == 0)
                    {
                        if (mPhase != Jump.PHASE_RECOVER_TO_IDLE)
                        {
                            mPhase = Jump.PHASE_RECOVER_TO_IDLE;
                            mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_RECOVER_TO_IDLE);
                        }
                    }
                    else
                    {
                        if (Mathf.Abs(lState.InputFromAvatarAngle) > 140)
                        {
                            lState.GroundLaunchVelocity = Vector3.zero;
                            if (mPhase != Jump.PHASE_RECOVER_TO_IDLE)
                            {
                                mPhase = Jump.PHASE_RECOVER_TO_IDLE;
                                mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_RECOVER_TO_IDLE);
                            }
                        }
                        else if (mPhase != Jump.PHASE_RECOVER_TO_RUN)
                        {
                            mPhase = Jump.PHASE_RECOVER_TO_RUN;
                            mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_RECOVER_TO_RUN);
                        }
                    }
                }
                // Look for the ground and prepare to transition
                else if (lState.GroundDistance < 0.25f)
                {
                    if (mPhase != Jump.PHASE_LAND)
                    {
                        mPhase = Jump.PHASE_LAND;
                        mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_LAND);
                    }
                }
            }
            // We could be falling for a while. This animation allows us to 
            // hold in the falling state until we hit the ground.
            else if (mController.CompareAnimatorStateName(mAnimatorLayerIndex, lBaseLayer + "Jump-SM.JumpFallPose"))
            {
                // Grow the collider as it falls
                mController.SetColliderHeightAtBase(mController.ActorHeight * 0.94f);

                // We need to move out of 'START_FALL' so we're not
                // constantly transitioning from Mecanim's 'Any State'
                if (mPhase == Jump.PHASE_START_FALL)
                {
                    mPhase = Jump.PHASE_FALL;
                    mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_FALL);
                }
                // We may need to transition out of the top state
                else if (mPhase == Jump.PHASE_TOP_TO_FALL)
                {
                    mPhase = Jump.PHASE_FALL;
                    mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_FALL);
                }

                // Look for the ground. In this case, we want a 
                // value that is slightly greater than our collider radius
                if (lState.GroundDistance < 0.25f)
                {
                    if (mPhase != Jump.PHASE_LAND)
                    {
                        mPhase = Jump.PHASE_LAND;
                        mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_LAND);
                    }
                }
            }
            // This is the first state in the jump where we hit the ground
            else if (mController.CompareAnimatorStateName(mAnimatorLayerIndex, lBaseLayer + "Jump-SM.JumpLand"))
            {
                // Shrink the collider as we impact
                if (lStateTime <= 0.2f)
                {
                    mController.SetColliderHeightAtBase(mController.ActorHeight * 0.94f);
                }
                else
                {
                    mController.SetColliderHeightAtBase(mController.ActorHeight * 0.77f);
                }

                // We only want to get a result once.
                if (mPhase == Jump.PHASE_LAND)
                {
                    // Make sure we're on the ground before we actually adjust the velocities
                    if ((lState.IsGrounded || mController.CharController.isGrounded))
                    {
                        // Move the camera back to what it was
                        if (mController.UseInput && mController.CameraRig != null) { mController.CameraRig.TransitionToMode(mSavedCameraMode); }

                        // If there is no controller input, we can stop
                        if (lState.InputMagnitudeTrend.Value < 0.1f)
                        {
                            lState.GroundLaunchVelocity = Vector3.zero;

                            if (mPhase != Jump.PHASE_RECOVER_TO_IDLE)
                            {
                                mPhase = Jump.PHASE_RECOVER_TO_IDLE;
                                mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_RECOVER_TO_IDLE);
                            }
                        }
                        // If the player is messing with the input, we need to think about
                        // what to transition to
                        else
                        {
                            if (Mathf.Abs(lState.InputFromAvatarAngle) > 140)
                            {
                                lState.GroundLaunchVelocity = Vector3.zero;
                                if (mPhase != Jump.PHASE_RECOVER_TO_IDLE)
                                {
                                    mPhase = Jump.PHASE_RECOVER_TO_IDLE;
                                    mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_RECOVER_TO_IDLE);
                                }
                            }
                            // If we haven't been told to recover to idle, we assume
                            // we'll head into a run.
                            else if (mPhase != Jump.PHASE_RECOVER_TO_IDLE)
                            {
                                if (mPhase != Jump.PHASE_RECOVER_TO_RUN)
                                {
                                    mPhase = Jump.PHASE_RECOVER_TO_RUN;
                                    mController.SetAnimatorMotionPhase(mAnimatorLayerIndex, Jump.PHASE_RECOVER_TO_RUN);
                                }

                                // We may be comeing from an idle jump into a run. If so, we'll
                                // apply a fake launch velocity so we move forward on the land.
                                if (lState.GroundLaunchVelocity.sqrMagnitude == 0f)
                                {
                                    lState.GroundLaunchVelocity = mController.transform.forward * mMovementSpeed;
                                }
                            }
                        }
                    }
                }
            }
            // Called when the avatar starts to come out of the impact
            else if (mController.CompareAnimatorStateName(mAnimatorLayerIndex, lBaseLayer + "Jump-SM.JumpRecoverIdle"))
            {
                // Grow the collider as we come out of the impact
                if (lStateTime > 0.5f)
                {
                    mController.ResetColliderHeight();
                }
                else
                {
                    mController.SetColliderHeightAtBase(mController.ActorHeight * 0.67f);
                }

                // Allow jumps to happen as we recover
                if (lStateTime > 0.5f && mPhase != Jump.PHASE_START_JUMP && mPhase != Jump.PHASE_START_FALL) { mIsStartable = true; }

                // If we've gone past the end, disable the jump
                if (lStateTime > 0.55f && mPhase != Jump.PHASE_START_JUMP && mPhase != Jump.PHASE_START_FALL) { Deactivate(); }
            }
            // Called when the avatar starts to come out of the impact
            else if (mController.CompareAnimatorStateName(mAnimatorLayerIndex, lBaseLayer + "Jump-SM.JumpRecoverRun"))
            {
                // Grow the collider as we come out of the impact
                if (lStateTime > 0.35f)
                {
                    mController.ResetColliderHeight();
                }
                else
                {
                    mController.SetColliderHeightAtBase(mController.ActorHeight * 0.67f);
                }

                // Allow jumps to happen as we recover
                if (lStateTime > 0.35f && mPhase != Jump.PHASE_START_JUMP && mPhase != Jump.PHASE_START_FALL) { mIsStartable = true; }

                // If we've gone past the end, disable the jump
                if (lStateTime > 0.70f && mPhase != Jump.PHASE_START_JUMP && mPhase != Jump.PHASE_START_FALL) 
                { 
                    Deactivate(); 
                }
                // We'll also deactivate if the player isn't pushing the avatar forward
                else if (mController.State.InputMagnitudeTrend.Average == 0f) 
                { 
                    // Actually, don't do this. It can get us in a wierd state
                    //Deactivate(); 
                }
            }

            // Set the controller state with the modified values
            mController.State = lState;

            // Determine the resulting velocity of this update
            DetermineVelocity();

            // Determine the resulting angular velocity of this update
            DetermineAngularVelocity();
        }

        /// <summary>
        /// Returns the current velocity of the motion
        /// </summary>
        protected override Vector3 DetermineVelocity()
        {
            ControllerState lState = mController.State;

#if UNITY_4_0 || UNITY_4_0_1 ||UNITY_4_1|| UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
            string lBaseLayer = "";
#else
            string lBaseLayer = "Base Layer.";
#endif

            // If we're recovering in idle, this is easy. There is no velocity
            if (mController.CompareAnimatorStateName(mAnimatorLayerIndex, lBaseLayer + "Jump-SM.JumpRecoverIdle"))
            {
                mVelocity = Vector3.zero;
            }
            // If were in the midst of jumping, we want to add velocity based on 
            // the magnitude of the controller. However, we don't add it we're heading back to idle
            else if (IsInJumpState)
            {
                Vector3 lBaseForward = mController.CameraTransform.forward;
                if (!mController.UseInput) { lBaseForward = mController.transform.forward; }

                // Direction of the camera
                Vector3 lCameraForward = lBaseForward;
                lCameraForward.y = 0f;
                lCameraForward.Normalize();

                // Create a quaternion that gets us from our world-forward to our camera direction.
                // FromToRotation creates a quaternion using the shortest method which can sometimes
                // flip the angle. LookRotation will attempt to keep the "up" direction "up".
                Quaternion lFromCamera = Quaternion.LookRotation(lCameraForward);

                // Determine the avatar displacement direction. This isn't just
                // normal movement forward, but includes movement to the side
                Vector3 lMoveDirection = lFromCamera * lState.InputForward;

                // Allow the player to create an initial launch velocity if there isn't one
                //if (!mControlEnabled && lState.InputMagnitudeTrend.Value != 0f && lState.GroundLaunchVelocity.magnitude == 0f)
                //{
                //    lState.GroundLaunchVelocity = lFromCamera * (lBaseForward * mMovementSpeed);
                //}

                // Determine the max air speed
                Vector3 lMomentum = lState.GroundLaunchVelocity;

                // Determine the air speed. We want the max of the momentum or control
                // speed. This gives us smooth movement while running and jumping
                float lControlSpeed = (mControlEnabled ? mMovementSpeed * lState.InputMagnitudeTrend.Value : 0f);

                float lAirSpeed = (mMomentumEnabled ? lMomentum.magnitude : lMomentum.magnitude * lState.InputMagnitudeTrend.Value);
                lAirSpeed = Mathf.Max(lAirSpeed, lControlSpeed);

                // Combine our control velocity with momentum
                Vector3 lAirVelocity = Vector3.zero;

                // When on the ground, continue with our momenum
                if (lState.IsGrounded)
                {
                    // If we're pulling back from the avatar's forward direction, 
                    // stop the movement
                    if (Mathf.Abs(mController.State.InputFromAvatarAngle) > 140)
                    {
                        lAirSpeed = 0f;
                    }

                    lAirVelocity += mController.transform.forward * lAirSpeed;
                }
                // While in the air, we have a speed based the max of our momentum or control speed
                else
                {
                    // If we allow control, let the player determine the direction
                    if (mControlEnabled)
                    {
                        lAirVelocity += lMoveDirection * lAirSpeed;
                    }

                    // If momementum is enabled, add it to keep the player moving in the direction of the jump
                    if (mMomentumEnabled)
                    {
                        lAirVelocity += lMomentum;
                    }
                }

                // Don't exceed our air speed
                if (lAirVelocity.magnitude > lAirSpeed)
                {
                    lAirVelocity = lAirVelocity.normalized * lAirSpeed;
                }

                // Return the final velocity
                mVelocity = lAirVelocity;
            }

            return mVelocity;
        }

        /// <summary>
        /// Returns the current angular velocity of the motion
        /// </summary>
        protected override Vector3 DetermineAngularVelocity()
        {
            ControllerState lState = mController.State;

            // Clear the rotation value
            mAngularVelocity = Vector3.zero;

            // Only create a rotation value if we're jumping
            if (IsInJumpState)
            {
                Quaternion lBaseRotation = mController.CameraTransform.rotation;
                if (!mController.UseInput) { lBaseRotation = mController.transform.rotation; }

                // If we've reached the end and we're on the ground, go back to 
                // rotating based on the player's input.
                if (lState.IsGrounded)
                {
                    //mAngularVelocity.y = lState.InputFromAvatarAngle * mController.RotationSpeed;
                    if (Mathf.Abs(mController.State.InputFromAvatarAngle) < 140)
                    {
                        mAngularVelocity.y = (lState.InputFromAvatarAngle / 90f) * mController.RotationSpeed;
                    }
                }
                // If we're in the air, we will limit how much rotation can actually happen
                else
                {
                    // If the player is in control, rotate the avatar based on the input
                    if (mControlEnabled)
                    {
                        // Determine the direction of the input relative to the camera
                        Vector3 lInputDirection = lBaseRotation * lState.InputForward;

                        // Check how much we're rotating vs. the original launch forward. We'll only 
                        // rotate if we're changing direction drastically.
                        float lDeltaAngle = Mathf.Abs(NumberHelper.GetHorizontalAngle(mLaunchForward, lInputDirection));
                        if (mRotationMin > 0 && (lDeltaAngle > mRotationMin || Mathf.Abs(lState.InputFromCameraAngle) < 5f))
                        {
                            //mAngularVelocity.y = lState.InputFromAvatarAngle * mRotationSpeed;
                            mAngularVelocity.y = (lState.InputFromAvatarAngle / 90f) * mRotationSpeed;
                        }
                    }
                    // Otherwise, control the rotation ourselves
                    else if (mMomentumEnabled)
                    {
                        // If we have a launch velocity, rotate the player towards the direction
                        if (mRotationSpeed > 0f && lState.GroundLaunchVelocity.sqrMagnitude != 0f)
                        {
                            float lRotation = NumberHelper.GetHorizontalAngle(mController.transform.forward, lState.GroundLaunchVelocity);

                            //mAngularVelocity.y = lRotation * mRotationSpeed;
                            mAngularVelocity.y = (lRotation / 90f) * mRotationSpeed;
                        }
                    }
                }
            }

            return mAngularVelocity;
        }

        /// <summary>
        /// Allows the motion to modify the velocity before it is applied.
        /// </summary>
        /// <returns></returns>
        public override void CleanRootMotion(ref Vector3 rVelocityDelta, ref Quaternion rRotationDelta)
        {
            // No movement when jumping
            if (IsInJumpState)
            {
                rVelocityDelta = Vector3.zero;
                rRotationDelta = Quaternion.identity;
            }
        }

        /// <summary>
        /// Raised when the animator's state has changed
        /// </summary>
        public override void OnAnimatorStateChange(int rLastStateID, int rNewStateID)
        {
#if UNITY_4_0 || UNITY_4_0_1 ||UNITY_4_1|| UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
            string lBaseLayer = "";
#else
            string lBaseLayer = "Base Layer.";
#endif

            // As soon as we get into the recovery state, revert the camera
            if (rNewStateID == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpRecoverIdle"] ||
                rNewStateID == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpRecoverRun"])
            {
                if (mController.UseInput && mController.CameraRig != null) { mController.CameraRig.TransitionToMode(mSavedCameraMode); }
            }

            // If we've moved out the recovery state, reset the jump info
            if (rLastStateID == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpRecoverIdle"] ||
                rLastStateID == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpRecoverRun"])
            {
                if (mPhase != Jump.PHASE_START_JUMP &&
                    mPhase != Jump.PHASE_START_FALL)
                {
                    Deactivate();

                    // Clear out the root motion for the idle so that nothing lingers
                    if (rLastStateID == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpRecoverIdle"])
                    {
                        mController.RootMotionVelocity = Vector3.zero;
                    }
                }
            }
        }

        /// <summary>
        /// Sets a value indicating whether this instance is in idle state.
        /// </summary>
        /// <value><c>true</c> if this instance is in idle state; otherwise, <c>false</c>.</value>
        protected bool IsInIdleState
        {
            get
            {

#if UNITY_4_0 || UNITY_4_0_1 ||UNITY_4_1|| UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
                string lBaseLayer = "";
#else
                string lBaseLayer = "Base Layer.";
#endif

                if (mController.CompareAnimatorStateName(mAnimatorLayerIndex, lBaseLayer + "Idle-SM.Idle_Casual"))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Test to see if we're currently in a jump state
        /// </summary>
        protected bool IsInJumpState
        {
            get
            {
                if (mPhase == Jump.PHASE_LAUNCH ||
                    mPhase == Jump.PHASE_START_JUMP ||
                    mPhase == Jump.PHASE_START_FALL
                    )
                {
                    return true;
                }
                else
                {
                    AnimatorStateInfo lAnimatorState = mController.State.AnimatorStates[mAnimatorLayerIndex].StateInfo;
                    AnimatorTransitionInfo lAnimatorTransition = mController.State.AnimatorStates[mAnimatorLayerIndex].TransitionInfo;

#if UNITY_4_0 || UNITY_4_0_1 ||UNITY_4_1|| UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
                    string lBaseLayer = "";
                    int lStateHash = lAnimatorState.nameHash;
                    int lTransitionHash = lAnimatorTransition.nameHash;
#else
                    string lBaseLayer = "Base Layer.";
                    int lStateHash = lAnimatorState.fullPathHash;
                    int lTransitionHash = lAnimatorTransition.fullPathHash;
#endif

                    if (lTransitionHash == mController.AnimatorStateIDs["Entry -> " + lBaseLayer + "Jump-SM.JumpRise"] ||
                        lTransitionHash == mController.AnimatorStateIDs["Entry -> " + lBaseLayer + "Jump-SM.JumpFallPose"] ||
                        lTransitionHash == mController.AnimatorStateIDs["AnyState -> " + lBaseLayer + "Jump-SM.JumpRise"] ||
                        lTransitionHash == mController.AnimatorStateIDs["AnyState -> " + lBaseLayer + "Jump-SM.JumpFallPose"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpRise"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpRisePose"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpRiseToTop"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpTopPose"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpTopToFall"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpFallPose"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpLand"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpRecoverIdle"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpRecoverRun"]
                        )
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Test to see if we're currently in a jump state prior to landing
        /// </summary>
        protected bool IsInMidJumpState
        {
            get
            {
                if (mPhase == Jump.PHASE_LAUNCH ||
                    mPhase == Jump.PHASE_START_JUMP ||
                    mPhase == Jump.PHASE_START_FALL
                    )
                {
                    return true;
                }
                else
                {
                    AnimatorStateInfo lAnimatorState = mController.State.AnimatorStates[mAnimatorLayerIndex].StateInfo;
                    AnimatorTransitionInfo lAnimatorTransition = mController.State.AnimatorStates[mAnimatorLayerIndex].TransitionInfo;

#if UNITY_4_0 || UNITY_4_0_1 ||UNITY_4_1|| UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
                    string lBaseLayer = "";
                    int lStateHash = lAnimatorState.nameHash;
                    int lTransitionHash = lAnimatorTransition.nameHash;
#else
                    string lBaseLayer = "Base Layer.";
                    int lStateHash = lAnimatorState.fullPathHash;
                    int lTransitionHash = lAnimatorTransition.fullPathHash;
#endif

                    if (lTransitionHash == mController.AnimatorStateIDs["Entry -> " + lBaseLayer + "Jump-SM.JumpRise"] ||
                        lTransitionHash == mController.AnimatorStateIDs["Entry -> " + lBaseLayer + "Jump-SM.JumpFallPose"] ||
                        lTransitionHash == mController.AnimatorStateIDs["AnyState -> " + lBaseLayer + "Jump-SM.JumpRise"] ||
                        lTransitionHash == mController.AnimatorStateIDs["AnyState -> " + lBaseLayer + "Jump-SM.JumpFallPose"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpRise"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpRisePose"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpRiseToTop"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpTopPose"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpTopToFall"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpFallPose"] ||
                        lStateHash == mController.AnimatorStateIDs[lBaseLayer + "Jump-SM.JumpLand"]
                        )
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
