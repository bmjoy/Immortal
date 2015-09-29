using System;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Collections;

namespace com.ootii.Geometry
{
    /// <summary>
    /// Provides functions for specialized raycast solutions
    /// </summary>
    public static class RaycastExt
    {
        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static bool SafeRaycast(Vector3 rRayStart, Vector3 rRayDirection, ref RaycastHit rHitInfo, float rDistance)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.Raycast(rRayStart, rRayDirection, out rHitInfo, rDistance);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    rRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                return true;
            }

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <param name="rIgnore">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static bool SafeRaycast(Vector3 rRayStart, Vector3 rRayDirection, ref RaycastHit rHitInfo, float rDistance, List<Transform> rIgnore)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;
            Vector3 lRayStart = rRayStart;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.Raycast(lRayStart, rRayDirection, out rHitInfo, rDistance);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }
                if (rIgnore != null && rIgnore.Contains(rHitInfo.collider.transform)) { lIsValidHit = false; }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    lRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;


                rHitInfo.distance = Vector3.Distance(rRayStart, rHitInfo.point);

                return true;
            }

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <param name="rIgnore">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static bool SafeRaycast(Vector3 rRayStart, Vector3 rRayDirection, ref RaycastHit rHitInfo, float rDistance, int rMask, List<Transform> rIgnore)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.Raycast(rRayStart, rRayDirection, out rHitInfo, rDistance, rMask);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }
                if (rIgnore != null && rIgnore.Contains(rHitInfo.collider.transform)) { lIsValidHit = false; }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    rRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                return true;
            }

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static RaycastHit[] SafeRaycastAll(Vector3 rRayStart, Vector3 rRayDirection, float rDistance, bool rRemoveTriggers)
        {
            RaycastHit[] lHitArray = UnityEngine.Physics.RaycastAll(rRayStart, rRayDirection, rDistance);

            // With no hits, this is easy
            if (lHitArray.Length == 0)
            {
            }
            // With one hit, this is easy too
            else if (lHitArray.Length == 1)
            {
                if (rRemoveTriggers && lHitArray[0].collider.isTrigger) 
                {
                    lHitArray = new RaycastHit[0];
                }
            }
            // Find the closest hit
            else
            {
                // Order the array by distance and get rid of items that don't pass
                lHitArray.Sort(delegate(RaycastHit rLeft, RaycastHit rRight) { return (rLeft.distance < rRight.distance ? -1 : (rLeft.distance > rRight.distance ? 1 : 0)); });
                for (int i = lHitArray.Length - 1; i >= 0; i--)
                {
                    if (rRemoveTriggers && lHitArray[i].collider.isTrigger) { lHitArray.RemoveAt(i); }
                }
            }

            return lHitArray;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static RaycastHit[] SafeRaycastAll(Vector3 rRayStart, Vector3 rRayDirection, float rDistance, List<Transform> rIgnore)
        {
            RaycastHit[] lHitArray = UnityEngine.Physics.RaycastAll(rRayStart, rRayDirection, rDistance);

            // With no hits, this is easy
            if (lHitArray.Length == 0)
            {
            }
            // With one hit, this is easy too
            else if (lHitArray.Length == 1)
            {
                if (lHitArray[0].collider.isTrigger ||
                    (rIgnore != null && rIgnore.Contains(lHitArray[0].collider.transform))
                   )
                {
                    lHitArray = new RaycastHit[0];
                }
            }
            // Find the closest hit
            else
            {
                // Order the array by distance and get rid of items that don't pass
                lHitArray.Sort(delegate(RaycastHit rLeft, RaycastHit rRight) { return (rLeft.distance < rRight.distance ? -1 : (rLeft.distance > rRight.distance ? 1 : 0)); });
                for (int i = lHitArray.Length - 1; i >= 0; i--)
                {
                    if (lHitArray[i].collider.isTrigger) { lHitArray.RemoveAt<RaycastHit>(i); }
                    if (rIgnore != null && rIgnore.Contains(lHitArray[i].collider.transform)) { lHitArray.RemoveAt(i); }
                }
            }

            return lHitArray;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static bool SafeSphereCast(Vector3 rRayStart, Vector3 rRayDirection, float rRadius, ref RaycastHit rHitInfo, float rDistance)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.SphereCast(rRayStart, rRadius, rRayDirection, out rHitInfo, rDistance);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    rRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                return true;
            }

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }
        
        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static RaycastHit[] SafeSphereCastAll(Vector3 rRayStart, Vector3 rRayDirection, float rRadius, float rDistance, List<Transform> rIgnore)
        {
            RaycastHit[] lHitArray = UnityEngine.Physics.SphereCastAll(rRayStart, rRadius, rRayDirection, rDistance);

            // With no hits, this is easy
            if (lHitArray.Length == 0)
            {
            }
            // With one hit, this is easy too
            else if (lHitArray.Length == 1)
            {
                if (lHitArray[0].collider.isTrigger ||
                    (rIgnore != null && rIgnore.Contains(lHitArray[0].collider.transform))
                   )
                {
                    lHitArray = new RaycastHit[0];
                }
            }
            // Find the closest hit
            else
            {
                // Order the array by distance and get rid of items that don't pass
                lHitArray.Sort(delegate(RaycastHit rLeft, RaycastHit rRight) { return (rLeft.distance < rRight.distance ? -1 : (rLeft.distance > rRight.distance ? 1 : 0)); });
                for (int i = lHitArray.Length - 1; i >= 0; i--)
                {
                    if (lHitArray[i].collider.isTrigger) { lHitArray.RemoveAt(i); }
                    if (rIgnore != null && rIgnore.Contains(lHitArray[i].collider.transform)) { lHitArray.RemoveAt(i); }
                }
            }

            return lHitArray;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static Collider[] SafeOverlapSphere(Vector3 rRayStart, float rRadius, List<Transform> rIgnore)
        {
            // This causes 28 B of GC.
            Collider[] lColliderHitList = UnityEngine.Physics.OverlapSphere(rRayStart, rRadius);

            // Get rid of elements we don't need
            for (int i = lColliderHitList.Length - 1; i >= 0; i--)
            {
                if (lColliderHitList[i].isTrigger ||
                    (rIgnore != null && rIgnore.Contains(lColliderHitList[i].transform)))
                {
                    lColliderHitList.RemoveAt<Collider>(i);
                }
            }

            // Return the rest
            return lColliderHitList;
        }
    }
}