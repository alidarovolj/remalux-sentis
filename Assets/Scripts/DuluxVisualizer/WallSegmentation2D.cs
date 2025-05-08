using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.Sentis;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using System.Linq;
using UnityEngine.UI;
using System;

/// <summary>
/// Структура для хранения 4D вектора с целочисленными значениями
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
/// Component for 2D wall segmentation using Sentis neural network
/// </summary>
public class WallSegmentation2D : MonoBehaviour
{
      [Header("Model Settings")]
      [SerializeField] private ModelAsset modelAsset;
      [SerializeField] private string inputName = "image";
      [SerializeField] private string outputName = "predict";
      [SerializeField] private int inputWidth = 256;
      [SerializeField] private int inputHeight = 256;
      [SerializeField] private bool useDemoMode = false;

      [Header("Segmentation Settings")]
      [SerializeField] private float threshold = 0.5f;
      [SerializeField] private int wallClassIndex = 0;
      [SerializeField] private Color wallColor = new Color(1f, 1f, 1f, 0.8f);

      [Header("Output")]
      [SerializeField] private RenderTexture outputTexture;

      [Header("Debug")]
      [SerializeField] private bool showDebug = false;
      [SerializeField] private RawImage debugImage;

      // Sentis objects
      private Model model;
      private Worker worker;
      private Tensor outputTensor;

      // Processing
      private Texture2D inputTexture;
      private Texture2D outputSegmentation;
      private bool isInitialized = false;
      private bool isProcessing = false;

      private void Start()
      {
            InitializeModel();
      }

      private void OnDestroy()
      {
            ReleaseResources();
      }

      /// <summary>
      /// Initializes the Sentis model
      /// </summary>
      private void InitializeModel()
      {
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
                        model = ModelLoader.Load(modelAsset);
                        Debug.Log($"Loaded model: {modelAsset.name}");

                        // Create worker (select backend based on device capabilities)
                        BackendType backend = SystemInfo.supportsComputeShaders ?
                              BackendType.GPUCompute : BackendType.CPU;
                        worker = new Worker(model, backend);

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

                  // Create input tensor from the texture
                  var textureTransform = new TextureTransform()
                        .SetDimensions(inputWidth, inputHeight);

                  var tensorShape = new TensorShape(1, 3, inputHeight, inputWidth);
                  var inputTensor = new Tensor<float>(tensorShape);

                  TextureConverter.ToTensor(sourceImage, inputTensor, textureTransform);

                  // Execute model
                  worker.Schedule(inputTensor);

                  // Get output tensor
                  var outputTensor = worker.PeekOutput(outputName);

                  // Convert to segmentation texture
                  outputSegmentation = CreateSegmentationTexture(outputTensor, sourceImage.width, sourceImage.height);

                  // Update output texture
                  Graphics.Blit(outputSegmentation, outputTexture);

                  // Update debug view
                  if (showDebug && debugImage != null)
                  {
                        debugImage.texture = outputSegmentation;
                  }

                  // Clean up
                  inputTensor.Dispose();
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
      /// Converts the output tensor into a segmentation mask
      /// </summary>
      private Texture2D CreateSegmentationTexture(Tensor outputTensor, int targetWidth, int targetHeight)
      {
            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);

            try
            {
                  // Get tensor data
                  TensorShape shape = outputTensor.shape;
                  int tensorWidth = 0;
                  int tensorHeight = 0;
                  int tensorChannels = 0;

                  // Handle different tensor shapes formats
                  if (shape.rank == 4) // NCHW format is common
                  {
                        tensorChannels = shape[1];
                        tensorHeight = shape[2];
                        tensorWidth = shape[3];
                  }
                  else if (shape.rank == 3) // CHW format (without batch)
                  {
                        tensorChannels = shape[0];
                        tensorHeight = shape[1];
                        tensorWidth = shape[2];
                  }
                  else
                  {
                        Debug.LogError($"Unexpected tensor shape: {shape}");
                        return CreateDemoSegmentation(targetWidth, targetHeight);
                  }

                  // Ensure wall class index is valid
                  int effectiveWallIndex = Mathf.Clamp(wallClassIndex, 0, tensorChannels - 1);

                  // Get tensor data as array
                  float[] tensorData = null;
                  if (outputTensor is Tensor<float> floatTensor)
                  {
                        tensorData = floatTensor.DownloadToArray();
                  }
                  else
                  {
                        Debug.LogError("Output tensor is not a float tensor");
                        return CreateDemoSegmentation(targetWidth, targetHeight);
                  }

                  // Create pixels
                  Color[] pixels = new Color[targetWidth * targetHeight];

                  for (int y = 0; y < targetHeight; y++)
                  {
                        for (int x = 0; x < targetWidth; x++)
                        {
                              // Map to tensor coordinates
                              int tx = (int)(x * (float)tensorWidth / targetWidth);
                              int ty = (int)(y * (float)tensorHeight / targetHeight);

                              // Calculate index based on NCHW format
                              float value;

                              if (tensorChannels == 1)
                              {
                                    // Single channel output - flat intensity map
                                    int index = ty * tensorWidth + tx;
                                    value = (index < tensorData.Length) ? tensorData[index] : 0;
                              }
                              else
                              {
                                    // Multi-channel (get wall class)
                                    int index = (effectiveWallIndex * tensorHeight * tensorWidth) + (ty * tensorWidth + tx);
                                    value = (index < tensorData.Length) ? tensorData[index] : 0;
                              }

                              // Apply threshold
                              if (value > threshold)
                              {
                                    pixels[y * targetWidth + x] = wallColor;
                              }
                              else
                              {
                                    pixels[y * targetWidth + x] = Color.clear;
                              }
                        }
                  }

                  result.SetPixels(pixels);
                  result.Apply();
                  return result;
            }
            catch (Exception e)
            {
                  Debug.LogError($"Error creating segmentation texture: {e.Message}");
                  return CreateDemoSegmentation(targetWidth, targetHeight);
            }
      }

      /// <summary>
      /// Gets the current segmentation texture
      /// </summary>
      public RenderTexture GetSegmentationTexture()
      {
            return outputTexture;
      }

      /// <summary>
      /// Release resources
      /// </summary>
      private void ReleaseResources()
      {
            if (worker != null)
            {
                  worker.Dispose();
                  worker = null;
            }

            if (outputTensor != null)
            {
                  outputTensor.Dispose();
                  outputTensor = null;
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
      /// Helper method for texture scaling
      /// </summary>
      private class TextureScale
      {
            public static void Bilinear(Texture2D texture, int newWidth, int newHeight)
            {
                  if (texture.width == newWidth && texture.height == newHeight)
                        return;

                  Color[] newColors = new Color[newWidth * newHeight];
                  Color[] oldColors = texture.GetPixels();
                  float ratioX = (float)texture.width / newWidth;
                  float ratioY = (float)texture.height / newHeight;

                  for (int y = 0; y < newHeight; y++)
                  {
                        for (int x = 0; x < newWidth; x++)
                        {
                              float oldX = x * ratioX;
                              float oldY = y * ratioY;
                              int oldX1 = Mathf.FloorToInt(oldX);
                              int oldY1 = Mathf.FloorToInt(oldY);
                              int oldX2 = Mathf.Min(oldX1 + 1, texture.width - 1);
                              int oldY2 = Mathf.Min(oldY1 + 1, texture.height - 1);

                              float u = oldX - oldX1;
                              float v = oldY - oldY1;

                              int idx1 = oldY1 * texture.width + oldX1;
                              int idx2 = oldY1 * texture.width + oldX2;
                              int idx3 = oldY2 * texture.width + oldX1;
                              int idx4 = oldY2 * texture.width + oldX2;

                              Color c1 = oldColors[idx1];
                              Color c2 = oldColors[idx2];
                              Color c3 = oldColors[idx3];
                              Color c4 = oldColors[idx4];

                              Color a = Color.Lerp(c1, c2, u);
                              Color b = Color.Lerp(c3, c4, u);
                              Color c = Color.Lerp(a, b, v);

                              newColors[y * newWidth + x] = c;
                        }
                  }

                  texture.Reinitialize(newWidth, newHeight);
                  texture.SetPixels(newColors);
                  texture.Apply();
            }
      }
}