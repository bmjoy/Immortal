/// Tim Tryzbiak, ootii, LLC
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace com.ootii.Helpers
{
    /// <summary>
    /// Static functions to help us
    /// </summary>
    public class StringHelper
    {
        /// <summary>
        /// Converts the data to a string value
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="rInput">Value to convert</param>
        public static string ToString(Vector3 rInput)
        {
            return String.Format("[x:{0:f4} y:{1:f4} z:{2:f4}]", rInput.x, rInput.y, rInput.z);
            //return String.Format("[{0:f4},{1:f4},{2:f4}]", rInput.x, rInput.y, rInput.z);
        }

        /// <summary>
        /// Converts the data to a string value
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="rInput">Value to convert</param>
        public static string ToString(Quaternion rInput)
        {
            Vector3 lEuler = rInput.eulerAngles;

            float lAngle = 0f;
            Vector3 lAxis = Vector3.zero;
            rInput.ToAngleAxis(out lAngle, out lAxis);

            return String.Format("[p:{0:f4} y:{1:f4} r:{2:f4} x:{3:f7} y:{4:f7} z:{5:f7} w:{6:f7} angle:{7:f7} axis:{8}]", lEuler.x, lEuler.y, lEuler.z, rInput.x, rInput.y, rInput.z, rInput.w, lAngle, ToString(lAxis));
        }

        /// <summary>
        /// Adds a space between camel cased text
        /// </summary>
        /// <param name="rInput"></param>
        /// <returns></returns>
        public static string FormatCamelCase(string rInput)
        {
            return Regex.Replace(Regex.Replace(rInput, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }
    }
}

