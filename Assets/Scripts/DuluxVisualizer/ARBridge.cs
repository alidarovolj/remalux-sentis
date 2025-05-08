using UnityEngine;
#if UNITY_AR_FOUNDATION_PRESENT
using UnityEngine.XR.ARFoundation;
#endif
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
#if UNITY_AR_FOUNDATION_PRESENT
            public static TrackableId GetCompatibleTrackableId(ARPlane plane)
#else
            public static TrackableId GetCompatibleTrackableId(object plane)
#endif
            {
                  if (plane == null)
                        return default;

#if UNITY_AR_FOUNDATION_PRESENT
                  string key = plane.trackableId.ToString();
#else
                  // Use reflection to get trackableId 
                  var property = plane.GetType().GetProperty("trackableId");
                  if (property == null)
                        return default;

                  var trackableId = property.GetValue(plane);
                  string key = trackableId.ToString();
#endif

                  if (trackableIdCache.TryGetValue(key, out var compatId))
                        return compatId;

                  // Create a new compatible TrackableId
                  var newId = new TrackableId(
#if UNITY_AR_FOUNDATION_PRESENT
                      GetSubId1FromTrackableId(plane.trackableId),
                      GetSubId2FromTrackableId(plane.trackableId)
#else
                      GetSubId1FromTrackableId(trackableId),
                      GetSubId2FromTrackableId(trackableId)
#endif
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
#if UNITY_AR_FOUNDATION_PRESENT
            public static bool IsVerticalPlane(ARPlane plane)
#else
            public static bool IsVerticalPlane(object plane)
#endif
            {
                  if (plane == null)
                        return false;

#if UNITY_AR_FOUNDATION_PRESENT
                  // Get the alignment value as int to compare with our enum
                  int alignmentValue = (int)plane.alignment;
#else
                  // Use reflection to get alignment
                  var property = plane.GetType().GetProperty("alignment");
                  if (property == null)
                        return false;

                  int alignmentValue = (int)property.GetValue(plane);
#endif
                  return alignmentValue == (int)PlaneAlignment.Vertical;
            }

            /// <summary>
            /// Checks if a plane has horizontal alignment pointing up
            /// </summary>
#if UNITY_AR_FOUNDATION_PRESENT
            public static bool IsHorizontalUpPlane(ARPlane plane)
#else
            public static bool IsHorizontalUpPlane(object plane)
#endif
            {
                  if (plane == null)
                        return false;

#if UNITY_AR_FOUNDATION_PRESENT
                  // Check using normal vector which is more reliable across versions
                  Vector3 normal = plane.normal;
#else
                  // Use reflection to get normal
                  var property = plane.GetType().GetProperty("normal");
                  if (property == null)
                        return false;

                  Vector3 normal = (Vector3)property.GetValue(plane);
#endif
                  return Vector3.Dot(normal, Vector3.up) > 0.9f;
            }

            /// <summary>
            /// Checks if a plane has horizontal alignment pointing down
            /// </summary>
#if UNITY_AR_FOUNDATION_PRESENT
            public static bool IsHorizontalDownPlane(ARPlane plane)
#else
            public static bool IsHorizontalDownPlane(object plane)
#endif
            {
                  if (plane == null)
                        return false;

#if UNITY_AR_FOUNDATION_PRESENT
                  // Check using normal vector which is more reliable across versions
                  Vector3 normal = plane.normal;
#else
                  // Use reflection to get normal
                  var property = plane.GetType().GetProperty("normal");
                  if (property == null)
                        return false;

                  Vector3 normal = (Vector3)property.GetValue(plane);
#endif
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
#if UNITY_AR_FOUNDATION_PRESENT
            public static bool IsTracking(ARPlane plane)
#else
            public static bool IsTracking(object plane)
#endif
            {
                  if (plane == null)
                        return false;

#if UNITY_AR_FOUNDATION_PRESENT
                  // Get the tracking state value as int to compare with our enum
                  int trackingState = (int)plane.trackingState;
#else
                  // Use reflection to get tracking state
                  var property = plane.GetType().GetProperty("trackingState");
                  if (property == null)
                        return false;

                  int trackingState = (int)property.GetValue(plane);
#endif
                  return trackingState == (int)TrackingState.Tracking;
            }
      }
}