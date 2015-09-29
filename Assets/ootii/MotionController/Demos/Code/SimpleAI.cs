using UnityEngine;
using System.Collections;
using com.ootii.AI.Controllers;

/// <summary>
/// Simple test to show controllering a character with the 
/// Motion Controller through script
/// </summary>
public class SimpleAI : MonoBehaviour
{
    public GameObject Target1 = null;
    public GameObject Target2 = null;

    private GameObject mActiveTarget = null;

    /// <summary>
    /// Reference to the motion controller for the avatar
    /// </summary>
    private MotionController mController = null;

    /// <summary>
    /// Called right before the first frame update
    /// </summary>
    void Start()
    {
        mActiveTarget = Target1;
        mController = GetComponent<MotionController>();
    }

    /// <summary>
    /// Called once per frame to update objects. This happens after FixedUpdate().
    /// Reactions to calculations should be handled here.
    /// </summary>
    void Update()
    {
        if (mController == null) { return; }
        if (mController.GetAnimatorStateName(0) == "") { return; }

        if (mActiveTarget == Target1 && Vector3.Distance(transform.position, Target1.transform.position) < 1f)
        {
            mActiveTarget = Target2;
            mController.SetTargetPosition(Target2.transform.position, 1f);
        }
        else if (mActiveTarget == Target2 && Vector3.Distance(transform.position, Target2.transform.position) < 1f)
        {
            mActiveTarget = Target1;
            mController.SetTargetPosition(Target1.transform.position, 1f);
        }
        else if (transform.position.x < -15 && transform.position.x > -16)
        {
            MotionControllerMotion lJump = mController.GetMotion(0, typeof(Jump));
            if (lJump != null) { mController.ActivateMotion(lJump); }

            if (mActiveTarget != null)
            {
                Quaternion lLookAt = Quaternion.LookRotation(mActiveTarget.transform.position - mController.transform.position, Vector3.up);
                mController.SetTargetRotation(lLookAt);
            }
        }
    }
}
