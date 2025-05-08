using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

namespace DuluxVisualizer
{
      /// <summary>
      /// Bridge class to handle AR Foundation compatibility issues
      /// This class provides helper methods for working with different AR Foundation versions
      /// and our compatibility layer
      /// </summary>
      public static class ARBridge
      {
            // Dictionary of wrapper objects for TrackableIds
            private static Dictionary<string, TrackableId> trackableIdCache = new Dictionary<string, TrackableId>();

            /// <summary>
            /// Gets a compatible TrackableId from an ARPlane
            /// Works with both our compatibility layer and the actual ARSubsystems implementation
            /// </summary>
            public static TrackableId GetCompatibleTrackableId(ARPlane plane)
            {
                  if (plane == null)
                        return default;

                  string key = plane.trackableId.ToString();
                  if (trackableIdCache.TryGetValue(key, out var compatId))
                        return compatId;

                  // Create a new compatible TrackableId
                  var newId = new TrackableId(
                      GetSubId1FromTrackableId(plane.trackableId),
                      GetSubId2FromTrackableId(plane.trackableId)
                  );

                  trackableIdCache[key] = newId;
                  return newId;
            }

            /// <summary>
            /// Gets the SubId1 value from a trackable ID using reflection
            /// </summary>
            private static ulong GetSubId1FromTrackableId(object trackableId)
            {
                  var type = trackableId.GetType();
                  var field = type.GetField("m_SubId1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                  if (field != null)
                        return (ulong)field.GetValue(trackableId);

                  // Try to access a property instead
                  var prop = type.GetProperty("subId1", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                  if (prop != null)
                        return (ulong)prop.GetValue(trackableId);

                  return 0;
            }

            /// <summary>
            /// Gets the SubId2 value from a trackable ID using reflection
            /// </summary>
            private static ulong GetSubId2FromTrackableId(object trackableId)
            {
                  var type = trackableId.GetType();
                  var field = type.GetField("m_SubId2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                  if (field != null)
                        return (ulong)field.GetValue(trackableId);

                  // Try to access a property instead
                  var prop = type.GetProperty("subId2", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                  if (prop != null)
                        return (ulong)prop.GetValue(trackableId);

                  return 0;
            }

            /// <summary>
            /// Checks if a plane has vertical alignment
            /// Works with both our compatibility layer and the actual ARSubsystems implementation
            /// </summary>
            public static bool IsVerticalPlane(ARPlane plane)
            {
                  if (plane == null)
                        return false;

                  // Get the alignment value as int to compare with our enum
                  int alignmentValue = (int)plane.alignment;
                  return alignmentValue == (int)PlaneAlignment.Vertical;
            }

            /// <summary>
            /// Checks if a plane has horizontal alignment pointing up
            /// </summary>
            public static bool IsHorizontalUpPlane(ARPlane plane)
            {
                  if (plane == null)
                        return false;

                  // Check using normal vector which is more reliable across versions
                  Vector3 normal = plane.normal;
                  return Vector3.Dot(normal, Vector3.up) > 0.9f;
            }

            /// <summary>
            /// Checks if a plane has horizontal alignment pointing down
            /// </summary>
            public static bool IsHorizontalDownPlane(ARPlane plane)
            {
                  if (plane == null)
                        return false;

                  // Check using normal vector which is more reliable across versions
                  Vector3 normal = plane.normal;
                  return Vector3.Dot(normal, Vector3.down) > 0.9f;
            }

            /// <summary>
            /// Gets a compatible TrackableType value that works across versions
            /// </summary>
            public static int GetCompatibleTrackableType(TrackableType type)
            {
                  // Convert our TrackableType enum to the int value expected by ARFoundation
                  switch (type)
                  {
                        case TrackableType.Plane:
                              return 1 << 1; // Corresponds to TrackableType.Plane in ARSubsystems
                        case TrackableType.Image:
                              return 1 << 0; // Corresponds to TrackableType.Image in ARSubsystems
                        case TrackableType.Point:
                              return 1 << 2; // Corresponds to TrackableType.Point in ARSubsystems
                        case TrackableType.All:
                              return ~0;     // All bits set
                        default:
                              return 0;      // None
                  }
            }

            /// <summary>
            /// Checks if a tracking state is Tracking
            /// </summary>
            public static bool IsTracking(ARPlane plane)
            {
                  if (plane == null)
                        return false;

                  // Get the tracking state value as int to compare with our enum
                  int trackingState = (int)plane.trackingState;
                  return trackingState == (int)TrackingState.Tracking;
            }
      }
}