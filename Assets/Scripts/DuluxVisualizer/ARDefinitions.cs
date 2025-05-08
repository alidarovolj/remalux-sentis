using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.XR;

namespace DuluxVisualizer
{
      /// <summary>
      /// This file provides conditional type definitions for AR Foundation types.
      /// If AR Foundation is available, it will use the real types directly.
      /// Otherwise, it will use our compatibility types.
      /// </summary>

#if UNITY_AR_FOUNDATION_PRESENT
      // If AR Foundation is available, use the real types
      using UnityEngine.XR.ARFoundation;
#if AR_SUBSYSTEMS_4_0_OR_NEWER
      using UnityEngine.XR.ARSubsystems;
#endif

      // Type aliases for clarity
      using ARSession = UnityEngine.XR.ARFoundation.ARSession;
      using ARCameraManager = UnityEngine.XR.ARFoundation.ARCameraManager;
      using ARCameraFrameEventArgs = UnityEngine.XR.ARFoundation.ARCameraFrameEventArgs;
      using ARPlane = UnityEngine.XR.ARFoundation.ARPlane;
      using ARPlaneManager = UnityEngine.XR.ARFoundation.ARPlaneManager;
      using ARRaycastManager = UnityEngine.XR.ARFoundation.ARRaycastManager;
      using ARRaycastHit = UnityEngine.XR.ARFoundation.ARRaycastHit;
#if AR_SUBSYSTEMS_4_0_OR_NEWER
      using TrackableId = UnityEngine.XR.ARSubsystems.TrackableId;
      using PlaneClassification = UnityEngine.XR.ARSubsystems.PlaneClassification;
      using TrackingState = UnityEngine.XR.ARSubsystems.TrackingState;
      using TrackableType = UnityEngine.XR.ARSubsystems.TrackableType;
#endif
#else
      // If AR Foundation is not available, use our compatibility types

      /// <summary>
      /// Simulated AR Session
      /// </summary>
      public class ARSession : MonoBehaviour
      {
            public bool enabled { get; set; } = true;
            public static ARSession instance { get; private set; }

            private void Awake()
            {
                  instance = this;
            }

            public void Reset() { }
      }

      /// <summary>
      /// Simulated AR Camera Manager
      /// </summary>
      public class ARCameraManager : MonoBehaviour
      {
            public delegate void FrameReceivedEventHandler(ARCameraFrameEventArgs eventArgs);
            public event FrameReceivedEventHandler frameReceived;

            public Camera GetComponent()
            {
                  return GetComponent<Camera>();
            }

            public void RaiseFrameReceivedEvent()
            {
                  frameReceived?.Invoke(new ARCameraFrameEventArgs());
            }
      }

      /// <summary>
      /// Simulated AR Camera Frame Event Args
      /// </summary>
      public struct ARCameraFrameEventArgs
      {
            public Texture2D texture;
            public Matrix4x4 displayMatrix;
      }

      /// <summary>
      /// Simulated AR Plane
      /// </summary>
      public class ARPlane : MonoBehaviour
      {
            public TrackingState trackingState { get; set; } = TrackingState.Tracking;
            public Pose pose { get; set; } = new Pose(Vector3.zero, Quaternion.identity);
            public Vector2 size { get; set; } = new Vector2(1, 1);
            public PlaneClassification classification { get; set; } = PlaneClassification.Wall;
            public GameObject gameObject => base.gameObject;
      }

      /// <summary>
      /// Simulated AR Plane Manager
      /// </summary>
      public class ARPlaneManager : MonoBehaviour
      {
            public bool enabled { get; set; } = true;

            public List<ARPlane> trackables { get; } = new List<ARPlane>();

            public ARPlane GetPlane(TrackableId trackableId)
            {
                  return trackables.Count > 0 ? trackables[0] : null;
            }
      }

      /// <summary>
      /// Simulated AR Raycast Manager
      /// </summary>
      public class ARRaycastManager : MonoBehaviour
      {
            public bool Raycast(Vector2 screenPoint, List<ARRaycastHit> hitResults, TrackableType trackableTypes)
            {
                  // Simplified implementation
                  if (hitResults != null)
                  {
                        hitResults.Clear();
                        hitResults.Add(new ARRaycastHit
                        {
                              pose = new Pose(Vector3.zero, Quaternion.identity),
                              trackableId = new TrackableId(1, 1)
                        });
                        return true;
                  }
                  return false;
            }
      }

      /// <summary>
      /// Simulated AR Raycast Hit result
      /// </summary>
      public struct ARRaycastHit
      {
            public TrackableId trackableId;
            public Pose pose;
            public float distance;
            public TrackableType hitType;
      }

      /// <summary>
      /// Trackable identifier
      /// </summary>
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
      }

      /// <summary>
      /// Plane classifications
      /// </summary>
      public enum PlaneClassification
      {
            None = 0,
            Wall = 1,
            Floor = 2,
            Ceiling = 3,
            Table = 4,
            Seat = 5
      }

      /// <summary>
      /// Tracking states
      /// </summary>
      public enum TrackingState
      {
            None = 0,
            Limited = 1,
            Tracking = 2
      }

      /// <summary>
      /// Trackable types for raycasting
      /// </summary>
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
#endif
}