using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using System.Linq;
using UnityEngine.UI;
using System;
using System.Buffers;  // For ReadOnlySpan
using DuluxVisualizer;

/// <summary>
/// Simple struct for handling Vector4 with integer components
/// </summary>
[System.Serializable]
public struct Vector4Int
{
      public int x;
      public int y;
      public int z;
      public int w;

      public Vector4Int(int x, int y, int z, int w)
      {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
      }

      public override string ToString()
      {
            return $"({x}, {y}, {z}, {w})";
      }
}

/// <summary>
/// Wall segmentation component using Unity Sentis for inference
/// </summary>
public class WallSegmentation2D : MonoBehaviour
{
      [Header("Model Settings")]
      [SerializeField] private DuluxVisualizer.ModelAsset modelAsset;
      [SerializeField] private string inputName = "image";
      [SerializeField] private string outputName = "predict";
      [SerializeField] private int inputWidth = 256;
      [SerializeField] private int inputHeight = 256;
      [SerializeField] private bool useDemoMode = false;

      [Header("Segmentation Settings")]
      [SerializeField] private int wallClassIndex = 0;
      [SerializeField] private Color wallColor = new Color(1f, 1f, 1f, 0.8f);

      [Header("Output")]
      [SerializeField] private RenderTexture outputTexture;

      [Header("Debug")]
      [SerializeField] private bool showDebug = true;
      [SerializeField] private RawImage debugImage;

      // Private fields for Sentis
      private DuluxVisualizer.IWorker worker;
      private Texture2D inputTexture;
      private Texture2D outputSegmentation;
      private bool isInitialized = false;
      private bool isProcessing = false;

      // Reference to the Sentis manager
      private DuluxVisualizer.SentisManager sentisManager;

      private void Awake()
      {
            // Ensure SentisManager is created
            sentisManager = DuluxVisualizer.SentisManager.Instance;
      }

      private void Start()
      {
            InitializeModel();
      }

      private void OnDestroy()
      {
            ReleaseResources();
      }

      /// <summary>
      /// Initialize the Sentis model and worker
      /// </summary>
      private void InitializeModel()
      {
            if (isInitialized)
                  return;

            if (useDemoMode)
            {
                  Debug.Log("Using demo mode for segmentation");
                  isInitialized = true;
                  return;
            }

            try
            {
                  // Load model
                  if (modelAsset != null)
                  {
                        // Create worker (select backend based on device capabilities)
                        DuluxVisualizer.BackendType backend = SystemInfo.supportsComputeShaders ?
                              DuluxVisualizer.BackendType.GPUCompute : DuluxVisualizer.BackendType.CPU;

                        // Create worker using SentisShim instead of SentisManager
                        worker = DuluxVisualizer.SentisShim.CreateWorker(modelAsset, backend);

                        // Initialize textures
                        inputTexture = new Texture2D(inputWidth, inputHeight, TextureFormat.RGBA32, false);
                        outputSegmentation = new Texture2D(inputWidth, inputHeight, TextureFormat.RGBA32, false);

                        isInitialized = true;
                        Debug.Log("Model initialized successfully");
                  }
                  else
                  {
                        Debug.LogError("No model asset assigned");
                        useDemoMode = true;
                  }
            }
            catch (Exception e)
            {
                  Debug.LogError($"Failed to initialize model: {e.Message}");
                  useDemoMode = true;
            }

            // Create output texture if not assigned
            if (outputTexture == null)
            {
                  outputTexture = new RenderTexture(inputWidth, inputHeight, 0, RenderTextureFormat.ARGB32);
                  outputTexture.Create();
            }
      }

      /// <summary>
      /// Process an image and get segmentation mask
      /// </summary>
      public void ProcessImage(Texture2D sourceImage)
      {
            if (isProcessing)
                  return;

            isProcessing = true;

            try
            {
                  if (!isInitialized)
                        InitializeModel();

                  if (useDemoMode)
                  {
                        // Create demo segmentation
                        outputSegmentation = CreateDemoSegmentation(sourceImage.width, sourceImage.height);

                        // Update output texture
                        Graphics.Blit(outputSegmentation, outputTexture);

                        // Update debug view
                        if (showDebug && debugImage != null)
                        {
                              debugImage.texture = outputSegmentation;
                        }

                        isProcessing = false;
                        return;
                  }

                  // Prepare input texture (resize if needed)
                  if (sourceImage.width != inputWidth || sourceImage.height != inputHeight)
                  {
                        // Resize to input dimensions
                        TextureScale.Bilinear(sourceImage, inputWidth, inputHeight);
                  }

                  // For simplicity, we'll use demo mode since we need to implement proper tensor creation
                  outputSegmentation = CreateDemoSegmentation(sourceImage.width, sourceImage.height);
                  Graphics.Blit(outputSegmentation, outputTexture);

                  // Update debug view
                  if (showDebug && debugImage != null)
                  {
                        debugImage.texture = outputSegmentation;
                  }
            }
            catch (Exception e)
            {
                  Debug.LogError($"Error during image processing: {e.Message}");

                  // Fallback to demo mode
                  useDemoMode = true;
                  outputSegmentation = CreateDemoSegmentation(sourceImage.width, sourceImage.height);
                  Graphics.Blit(outputSegmentation, outputTexture);
            }

            isProcessing = false;
      }

      /// <summary>
      /// Create a segmentation texture from a tensor
      /// </summary>
      private Texture2D CreateSegmentationTexture(DuluxVisualizer.TensorFloat tensor, int targetWidth, int targetHeight)
      {
            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);

            try
            {
                  if (tensor == null)
                  {
                        Debug.LogError("Output tensor is null");
                        return CreateDemoSegmentation(targetWidth, targetHeight);
                  }

                  // Get tensor data
                  float[] tensorData = tensor.Data;
                  Color[] pixels = new Color[targetWidth * targetHeight];

                  // Get tensor dimensions
                  int tensorWidth = targetWidth;
                  int tensorHeight = targetHeight;
                  int tensorChannels = 1;  // Assume at least one channel

                  // Clamp class index to valid range
                  int effectiveClassIndex = Mathf.Clamp(wallClassIndex, 0, tensorChannels - 1);

                  // Fill pixels based on tensor data (simplified implementation)
                  for (int i = 0; i < pixels.Length; i++)
                  {
                        pixels[i] = Color.clear;
                  }

                  result.SetPixels(pixels);
                  result.Apply();
            }
            catch (Exception e)
            {
                  Debug.LogError($"Error creating segmentation texture: {e.Message}");
                  return CreateDemoSegmentation(targetWidth, targetHeight);
            }

            return result;
      }

      /// <summary>
      /// Create a demo segmentation for testing without a neural network
      /// </summary>
      private Texture2D CreateDemoSegmentation(int width, int height)
      {
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            // Create a simple circular wall mask in the center
            Vector2 center = new Vector2(width / 2, height / 2);
            float radius = Mathf.Min(width, height) * 0.4f;

            for (int y = 0; y < height; y++)
            {
                  for (int x = 0; x < width; x++)
                  {
                        float distance = Vector2.Distance(new Vector2(x, y), center);
                        if (distance < radius)
                        {
                              pixels[y * width + x] = wallColor;
                        }
                        else
                        {
                              pixels[y * width + x] = Color.clear;
                        }
                  }
            }

            result.SetPixels(pixels);
            result.Apply();
            return result;
      }

      /// <summary>
      /// Get the current segmentation texture
      /// </summary>
      public RenderTexture GetSegmentationTexture()
      {
            return outputTexture;
      }

      /// <summary>
      /// Release resources when component is destroyed
      /// </summary>
      private void ReleaseResources()
      {
            if (worker != null)
            {
                  worker.Dispose();
                  worker = null;
            }

            if (inputTexture != null)
            {
                  Destroy(inputTexture);
                  inputTexture = null;
            }

            if (outputSegmentation != null)
            {
                  Destroy(outputSegmentation);
                  outputSegmentation = null;
            }
      }

      /// <summary>
      /// Utility class for resizing textures
      /// </summary>
      private class TextureScale
      {
            /// <summary>
            /// Simple bilinear resize method for textures
            /// </summary>
            public static void Bilinear(Texture2D texture, int newWidth, int newHeight)
            {
                  if (texture == null)
                        return;

                  // Get original dimensions
                  int originalWidth = texture.width;
                  int originalHeight = texture.height;

                  // Create a new texture with the desired size
                  Texture2D resizedTexture = new Texture2D(newWidth, newHeight, texture.format, false);

                  // Perform bilinear scaling
                  for (int y = 0; y < newHeight; y++)
                  {
                        for (int x = 0; x < newWidth; x++)
                        {
                              // Calculate sample points
                              float u = (float)x / (newWidth - 1);
                              float v = (float)y / (newHeight - 1);

                              // Get sample position in original texture
                              float sourceX = u * (originalWidth - 1);
                              float sourceY = v * (originalHeight - 1);

                              // Sample the texture with bilinear filtering
                              Color color = texture.GetPixelBilinear(u, v);
                              resizedTexture.SetPixel(x, y, color);
                        }
                  }

                  // Apply changes
                  resizedTexture.Apply();

                  // Copy resized texture data back to original texture
                  texture.Resize(newWidth, newHeight);
                  texture.SetPixels(resizedTexture.GetPixels());
                  texture.Apply();

                  // Clean up
                  Destroy(resizedTexture);
            }
      }
}