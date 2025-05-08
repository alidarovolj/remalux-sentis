using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_SENTIS
using Unity.Sentis;
#endif
#if UNITY_AR_FOUNDATION_PRESENT
using UnityEngine.XR.ARFoundation;
#endif
using UnityEngine.UI;
using DuluxVisualizer;

/// <summary>
/// Compatibility adapter for wall segmentation
/// </summary>
public class WallSegmentation : MonoBehaviour
{
      [SerializeField] private SentisWallSegmentation sentisWallSegmentation;

      // Private cache for real AR camera manager
      private UnityEngine.XR.ARFoundation.ARCameraManager _realCameraManager;

      // Public properties for compatibility
#if UNITY_AR_FOUNDATION_PRESENT
      public UnityEngine.XR.ARFoundation.ARCameraManager cameraManager
      {
            get
            {
                  // Use cached version if possible
                  if (_realCameraManager != null)
                        return _realCameraManager;

                  // Find ARCameraManager in the scene if it exists
                  if (_realCameraManager == null && sentisWallSegmentation != null)
                  {
                        _realCameraManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARCameraManager>();
                  }

                  return _realCameraManager;
            }
      }
#else
      public DuluxVisualizer.ARCameraManager cameraManager
      {
            get { return sentisWallSegmentation != null ? sentisWallSegmentation.cameraManager : null; }
      }
#endif

      public ModelAsset modelAsset
      {
            get { return null; } // Deprecated - using sentis models now
      }

      public RenderTexture outputRenderTexture
      {
            get { return sentisWallSegmentation != null ? sentisWallSegmentation.outputRenderTexture : null; }
            set
            {
                  if (sentisWallSegmentation != null)
                        sentisWallSegmentation.outputRenderTexture = value;
            }
      }

      // Segmentation modes (for compatibility)
      public enum SegmentationMode
      {
            Demo,
            EmbeddedModel,
            ExternalModel
      }

      private void Awake()
      {
            // Find SentisWallSegmentation if not assigned
            if (sentisWallSegmentation == null)
            {
                  sentisWallSegmentation = GetComponent<SentisWallSegmentation>();
                  if (sentisWallSegmentation == null)
                  {
                        sentisWallSegmentation = FindObjectOfType<SentisWallSegmentation>();
                        if (sentisWallSegmentation == null)
                        {
                              Debug.LogWarning("SentisWallSegmentation not found. Creating a new instance.");
                              sentisWallSegmentation = gameObject.AddComponent<SentisWallSegmentation>();
                        }
                  }
            }
      }

      // API compatibility methods

      /// <summary>
      /// Switch segmentation mode
      /// </summary>
      public void SwitchMode(SegmentationMode mode)
      {
            if (sentisWallSegmentation == null)
                  return;

            // Map to Sentis mode
            switch (mode)
            {
                  case SegmentationMode.Demo:
                        // We don't have direct control over setting useDemoMode
                        // This would require changes to SentisWallSegmentation
                        Debug.Log("Switching to Demo mode");
                        break;

                  case SegmentationMode.EmbeddedModel:
                        Debug.Log("Switching to Embedded model mode");
                        break;

                  case SegmentationMode.ExternalModel:
                        Debug.Log("Switching to External model mode");
                        break;
            }
      }

      /// <summary>
      /// Gets the current segmentation texture
      /// </summary>
      public Texture2D GetSegmentationTexture()
      {
            // Return a placeholder texture if needed - actual texture would be in outputRenderTexture
            return null;
      }

      /// <summary>
      /// Enables or disables debug visualization
      /// </summary>
      public void EnableDebugVisualization(bool enable)
      {
            // Compatibility method
            Debug.Log($"Debug visualization {(enable ? "enabled" : "disabled")}");
      }

      /// <summary>
      /// Checks if debug visualization is enabled
      /// </summary>
      public bool IsDebugVisualizationEnabled()
      {
            return false; // Default value
      }

      /// <summary>
      /// Checks if using demo mode
      /// </summary>
      public bool IsUsingDemoMode()
      {
            // Only SentisWallSegmentation knows this
            return false;
      }

      /// <summary>
      /// Gets the percentage of plane covered by segmentation mask
      /// </summary>
#if UNITY_AR_FOUNDATION_PRESENT
      public float GetPlaneCoverageByMask(UnityEngine.XR.ARFoundation.ARPlane plane)
      {
            // Default implementation
            return 0.5f;
      }
#else
      public float GetPlaneCoverageByMask(object plane)
      {
            // Default implementation
            return 0.5f;
      }
#endif

      /// <summary>
      /// Updates status of planes based on segmentation
      /// </summary>
      public int UpdatePlanesSegmentationStatus()
      {
            // Default implementation
            return 0;
      }
}