using UnityEngine;
using UnityEngine.XR.ARFoundation;
#if UNITY_SENTIS
using Unity.Sentis;
#endif
using System.Collections.Generic;
using System.Collections;
using System;

namespace DuluxVisualizer
{
      /// <summary>
      /// Processes wall segmentation for AR scenes
      /// </summary>
      [RequireComponent(typeof(ARCameraManager))]
      public class WallSegmentationProcessor : MonoBehaviour
      {
            [Header("Processing Settings")]
            [SerializeField] private float processingInterval = 0.5f;
            [SerializeField] private bool useTemporalSmoothing = true;
            [SerializeField] private float temporalSmoothingFactor = 0.5f;

            [Header("References")]
            [SerializeField] private SentisWallSegmentation wallSegmentation;
            [SerializeField] private ARCameraManager cameraManager;

            // Processing state
            private bool isProcessing = false;
            private float lastProcessTime = 0f;

            // Result storage
            private Texture2D previousResult;
            private RenderTexture outputTexture;

            // Events
            public delegate void ProcessingCompleteDelegate(RenderTexture segmentationResult);
            public event ProcessingCompleteDelegate OnProcessingComplete;

            private void Awake()
            {
                  // Find references if not assigned
                  if (wallSegmentation == null)
                  {
                        wallSegmentation = FindObjectOfType<SentisWallSegmentation>();
                  }

                  if (cameraManager == null && wallSegmentation != null)
                  {
                        cameraManager = wallSegmentation.cameraManager;
                  }

                  // Initialize output texture
                  CreateOutputTexture(512, 512);

                  // Start processing routine
                  StartCoroutine(ProcessingRoutine());
            }

            /// <summary>
            /// Sets the processing interval (seconds between updates)
            /// </summary>
            public void SetProcessingInterval(float interval)
            {
                  processingInterval = Mathf.Max(0.1f, interval);
            }

            /// <summary>
            /// Sets the temporal smoothing factor (0-1)
            /// </summary>
            public void SetTemporalSmoothing(float smoothing)
            {
                  temporalSmoothingFactor = Mathf.Clamp01(smoothing);
            }

            /// <summary>
            /// Processing routine that runs at regular intervals
            /// </summary>
            private IEnumerator ProcessingRoutine()
            {
                  // Wait for initialization
                  yield return new WaitForSeconds(0.5f);

                  while (true)
                  {
                        // Check if we should process
                        if (!isProcessing && Time.time - lastProcessTime > processingInterval)
                        {
                              // Update timestamp
                              lastProcessTime = Time.time;

                              // Start processing
                              isProcessing = true;

                              // Get result from wall segmentation
                              if (wallSegmentation != null && wallSegmentation.outputRenderTexture != null)
                              {
                                    // Apply temporal smoothing if enabled
                                    if (useTemporalSmoothing && previousResult != null)
                                    {
                                          ApplyTemporalSmoothing(wallSegmentation.outputRenderTexture);
                                    }
                                    else
                                    {
                                          // Just copy the result
                                          Graphics.Blit(wallSegmentation.outputRenderTexture, outputTexture);
                                    }

                                    // Notify listeners
                                    OnProcessingComplete?.Invoke(outputTexture);
                              }

                              // Mark as done
                              isProcessing = false;
                        }

                        // Wait before checking again
                        yield return new WaitForSeconds(0.1f);
                  }
            }

            /// <summary>
            /// Creates the output texture with the specified dimensions
            /// </summary>
            private void CreateOutputTexture(int width, int height)
            {
                  if (outputTexture != null)
                  {
                        outputTexture.Release();
                  }

                  outputTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
                  outputTexture.Create();
            }

            /// <summary>
            /// Applies temporal smoothing between frames
            /// </summary>
            private void ApplyTemporalSmoothing(RenderTexture currentFrame)
            {
                  // Read current frame
                  RenderTexture currentRT = RenderTexture.active;
                  RenderTexture.active = currentFrame;

                  if (previousResult == null || previousResult.width != currentFrame.width || previousResult.height != currentFrame.height)
                  {
                        previousResult = new Texture2D(currentFrame.width, currentFrame.height, TextureFormat.RGBA32, false);
                  }

                  previousResult.ReadPixels(new Rect(0, 0, currentFrame.width, currentFrame.height), 0, 0);
                  previousResult.Apply();

                  RenderTexture.active = currentRT;

                  // Apply blending with previous result
                  // This would typically be done with a custom shader,
                  // but for simplicity, we'll just blit with standard alpha blending
                  Graphics.Blit(previousResult, outputTexture);
            }

            private void OnDestroy()
            {
                  if (outputTexture != null)
                  {
                        outputTexture.Release();
                        outputTexture = null;
                  }

                  if (previousResult != null)
                  {
                        Destroy(previousResult);
                        previousResult = null;
                  }
            }
      }
}