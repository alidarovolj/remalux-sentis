using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DuluxVisualizer
{
      /// <summary>
      /// Extension methods for ARRaycastManager to handle compatibility issues
      /// with different versions of ARFoundation and ARSubsystems
      /// </summary>
      public static class ARRaycastExtensions
      {
            /// <summary>
            /// A compatible version of Raycast that works with our TrackableType enum
            /// and different versions of ARFoundation
            /// </summary>
            public static bool RaycastWithCompat(
                this ARRaycastManager raycastManager,
                Vector2 screenPoint,
                List<ARRaycastHit> hitResults,
                TrackableType trackableTypes)
            {
                  // Convert our enum to the expected format
                  object convertedTrackableTypes = ConvertTrackableType(trackableTypes);

                  // Get the actual Raycast method
                  MethodInfo raycastMethod = raycastManager.GetType().GetMethod(
                      "Raycast",
                      new[] { typeof(Vector2), typeof(List<ARRaycastHit>), convertedTrackableTypes.GetType() }
                  );

                  if (raycastMethod != null)
                  {
                        // Invoke the method with converted parameter
                        return (bool)raycastMethod.Invoke(raycastManager, new[] { screenPoint, hitResults, convertedTrackableTypes });
                  }

                  // Fallback to direct call if reflection fails (may or may not work depending on AR Foundation version)
                  return raycastManager.Raycast(screenPoint, hitResults, (UnityEngine.XR.ARSubsystems.TrackableType)(int)trackableTypes);
            }

            /// <summary>
            /// Convert our TrackableType enum to the appropriate type for the current ARFoundation version
            /// </summary>
            private static object ConvertTrackableType(TrackableType trackableType)
            {
                  // First try to get the AR Subsystems TrackableType type
                  System.Type subsystemsTrackableType = null;

                  try
                  {
                        // Try to get the type from ARSubsystems
                        subsystemsTrackableType = System.Type.GetType("UnityEngine.XR.ARSubsystems.TrackableType, Unity.XR.ARSubsystems");
                  }
                  catch
                  {
                        // Type not found, fall back to integer value
                        return (int)trackableType;
                  }

                  if (subsystemsTrackableType != null && subsystemsTrackableType.IsEnum)
                  {
                        // Convert to the appropriate enum value
                        switch (trackableType)
                        {
                              case TrackableType.None:
                                    return Enum.Parse(subsystemsTrackableType, "None");
                              case TrackableType.Plane:
                                    return Enum.Parse(subsystemsTrackableType, "Planes");
                              case TrackableType.Point:
                                    return Enum.Parse(subsystemsTrackableType, "FeaturePoints");
                              case TrackableType.Image:
                                    return Enum.Parse(subsystemsTrackableType, "Images");
                              case TrackableType.All:
                                    return Enum.Parse(subsystemsTrackableType, "All");
                              default:
                                    return Enum.Parse(subsystemsTrackableType, "None");
                        }
                  }

                  // Just use the integer value as fallback
                  return (int)trackableType;
            }
      }
}