using UnityEngine;
using System;
using System.Collections.Generic;

// This file provides fallbacks for AR Foundation types when the package is not installed
// These types will only be used when UNITY_AR_FOUNDATION_PRESENT is NOT defined

#if !UNITY_AR_FOUNDATION_PRESENT
namespace UnityEngine.XR.ARFoundation
{
      // Basic AR Foundation types
      public class ARSession : MonoBehaviour
      {
            public bool enabled { get; set; } = true;
            public static ARSession instance { get; private set; }

            public void Reset() { }
      }

      public class ARCameraManager : MonoBehaviour
      {
            public delegate void FrameReceivedEventHandler(ARCameraFrameEventArgs eventArgs);
            public event FrameReceivedEventHandler frameReceived;

            public Camera GetComponent() { return null; }
      }

      public struct ARCameraFrameEventArgs
      {
            public Texture2D texture;
            public Matrix4x4 displayMatrix;
      }

      public class ARPlane : MonoBehaviour
      {
            public ARSubsystems.TrackingState trackingState { get; set; } = ARSubsystems.TrackingState.Tracking;
            public Pose pose { get; set; } = new Pose(Vector3.zero, Quaternion.identity);
            public Vector2 size { get; set; } = new Vector2(1, 1);
            public ARSubsystems.PlaneClassification classification { get; set; } = ARSubsystems.PlaneClassification.Wall;
            public GameObject gameObject => base.gameObject;
            public ARSubsystems.TrackableId trackableId { get; set; }
      }

      public class ARPlaneManager : MonoBehaviour
      {
            public bool enabled { get; set; } = true;
            public List<ARPlane> trackables { get; } = new List<ARPlane>();
            public ARSubsystems.PlaneDetectionMode requestedDetectionMode { get; set; }

            public ARPlane GetPlane(ARSubsystems.TrackableId trackableId) { return null; }
      }

      public class ARRaycastManager : MonoBehaviour
      {
            public bool Raycast(Vector2 screenPoint, List<ARRaycastHit> hitResults, ARSubsystems.TrackableType trackableTypes = ARSubsystems.TrackableType.All)
            {
                  hitResults?.Clear();
                  return false;
            }
      }

      public struct ARRaycastHit
      {
            public ARSubsystems.TrackableId trackableId;
            public Pose pose;
            public float distance;
            public ARSubsystems.TrackableType hitType;
      }
}

namespace UnityEngine.XR.ARSubsystems
{
      [Flags]
      public enum TrackableType
      {
            None = 0,
            PlaneWithinPolygon = 1 << 0,
            PlaneWithinBounds = 1 << 1,
            PlaneEstimated = 1 << 2,
            Planes = PlaneWithinPolygon | PlaneWithinBounds | PlaneEstimated,
            FeaturePoint = 1 << 3,
            All = Planes | FeaturePoint
      }

      public enum TrackingState
      {
            None = 0,
            Limited = 1,
            Tracking = 2
      }

      public enum PlaneClassification
      {
            None = 0,
            Wall = 1,
            Floor = 2,
            Ceiling = 3,
            Table = 4,
            Seat = 5
      }

      public struct TrackableId : IEquatable<TrackableId>
      {
            private ulong m_SubId1;
            private ulong m_SubId2;

            public TrackableId(ulong subId1, ulong subId2)
            {
                  m_SubId1 = subId1;
                  m_SubId2 = subId2;
            }

            public bool Equals(TrackableId other)
            {
                  return m_SubId1 == other.m_SubId1 && m_SubId2 == other.m_SubId2;
            }

            public override bool Equals(object obj)
            {
                  return obj is TrackableId other && Equals(other);
            }

            public override int GetHashCode()
            {
                  unchecked
                  {
                        return (m_SubId1.GetHashCode() * 397) ^ m_SubId2.GetHashCode();
                  }
            }

            public static bool operator ==(TrackableId lhs, TrackableId rhs) => lhs.Equals(rhs);
            public static bool operator !=(TrackableId lhs, TrackableId rhs) => !lhs.Equals(rhs);
      }

      public enum PlaneDetectionMode
      {
            None = 0,
            Horizontal = 1,
            Vertical = 2,
            HorizontalAndVertical = 3
      }
}
#endif