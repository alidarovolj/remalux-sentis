using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

// This namespace should match your assembly definition
namespace DuluxVisualizer
{
      /// <summary>
      /// Adapter class to help transition from Barracuda to Unity Sentis 2.1.2
      /// This class provides helper methods to simplify working with the new API
      /// </summary>
      public static class SentisAdapter
      {
            /// <summary>
            /// Creates a new worker with the given model and backend
            /// </summary>
            public static object CreateWorker(object model, int backendType)
            {
                  try
                  {
                        // Get the WorkerFactory type using reflection
                        var workerFactoryType = Type.GetType("Unity.Sentis.WorkerFactory, Unity.Sentis");
                        if (workerFactoryType == null)
                        {
                              Debug.LogError("Failed to find Unity.Sentis.WorkerFactory type");
                              return null;
                        }

                        // Get the CreateWorker method
                        var createWorkerMethod = workerFactoryType.GetMethod("CreateWorker");
                        if (createWorkerMethod == null)
                        {
                              Debug.LogError("Failed to find CreateWorker method");
                              return null;
                        }

                        // Call the method
                        return createWorkerMethod.Invoke(null, new object[] { backendType, model });
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"Error creating worker: {e.Message}");
                        return null;
                  }
            }

            /// <summary>
            /// Creates a tensor from an input texture
            /// </summary>
            public static object TextureToTensor(Texture2D texture, int width, int height, bool useNCHW = true)
            {
                  try
                  {
                        // Get the TextureTransform type
                        var textureTransformType = Type.GetType("Unity.Sentis.TextureTransform, Unity.Sentis");
                        if (textureTransformType == null)
                        {
                              Debug.LogError("Failed to find Unity.Sentis.TextureTransform type");
                              return null;
                        }

                        // Create TextureTransform instance
                        var textureTransform = Activator.CreateInstance(textureTransformType);

                        // Get and call SetDimensions method
                        var setDimensionsMethod = textureTransformType.GetMethod("SetDimensions");
                        textureTransform = setDimensionsMethod.Invoke(textureTransform, new object[] { width, height });

                        // Get TensorLayout enum type and values
                        var tensorLayoutType = Type.GetType("Unity.Sentis.TensorLayout, Unity.Sentis");
                        if (tensorLayoutType == null)
                        {
                              Debug.LogError("Failed to find Unity.Sentis.TensorLayout type");
                              return null;
                        }

                        // Get NCHW and NHWC enum values
                        var nchwValue = Enum.Parse(tensorLayoutType, "NCHW");
                        var nhwcValue = Enum.Parse(tensorLayoutType, "NHWC");

                        // Call SetTensorLayout
                        var setLayoutMethod = textureTransformType.GetMethod("SetTensorLayout");
                        textureTransform = setLayoutMethod.Invoke(textureTransform, new object[] { useNCHW ? nchwValue : nhwcValue });

                        // Get the TextureConverter type
                        var textureConverterType = Type.GetType("Unity.Sentis.TextureConverter, Unity.Sentis");
                        if (textureConverterType == null)
                        {
                              Debug.LogError("Failed to find Unity.Sentis.TextureConverter type");
                              return null;
                        }

                        // Get and call ToTensor method
                        var toTensorMethod = textureConverterType.GetMethod("ToTensor");
                        return toTensorMethod.Invoke(null, new object[] { texture, textureTransform });
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"Error creating tensor from texture: {e.Message}");
                        return null;
                  }
            }

            /// <summary>
            /// Execute model inference with the given input tensor
            /// </summary>
            public static void ExecuteModel(object worker, string inputName, object inputTensor)
            {
                  try
                  {
                        // Create inputs dictionary
                        var inputs = new Dictionary<string, object> { { inputName, inputTensor } };

                        // Get Execute method from worker
                        var executeMethod = worker.GetType().GetMethod("Execute", new Type[] { typeof(Dictionary<string, object>) });
                        if (executeMethod == null)
                        {
                              Debug.LogError("Failed to find Execute method");
                              return;
                        }

                        // Call Execute
                        executeMethod.Invoke(worker, new object[] { inputs });
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"Error executing model: {e.Message}");
                  }
            }

            /// <summary>
            /// Get the output tensor from the worker
            /// </summary>
            public static object GetOutputTensor(object worker, string outputName)
            {
                  try
                  {
                        // Get PeekOutput method
                        var peekOutputMethod = worker.GetType().GetMethod("PeekOutput");
                        if (peekOutputMethod == null)
                        {
                              Debug.LogError("Failed to find PeekOutput method");
                              return null;
                        }

                        // Call PeekOutput
                        return peekOutputMethod.Invoke(worker, new object[] { outputName });
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"Error getting output tensor: {e.Message}");
                        return null;
                  }
            }

            /// <summary>
            /// Create a texture from an output tensor representing a segmentation mask
            /// </summary>
            public static Texture2D CreateSegmentationTexture(object tensor, int targetWidth, int targetHeight,
                int classIndex = 0, float threshold = 0.5f, Color maskColor = default)
            {
                  if (maskColor == default)
                        maskColor = Color.white;

                  Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);

                  try
                  {
                        // Get tensor data using reflection
                        var toReadOnlySpanMethod = tensor.GetType().GetMethod("ToReadOnlySpan");
                        if (toReadOnlySpanMethod == null)
                        {
                              Debug.LogError("Failed to find ToReadOnlySpan method");
                              return result;
                        }

                        var tensorData = toReadOnlySpanMethod.Invoke(tensor, null);

                        // Get tensor shape property
                        var shapeProperty = tensor.GetType().GetProperty("shape");
                        if (shapeProperty == null)
                        {
                              Debug.LogError("Failed to find shape property");
                              return result;
                        }

                        var shape = shapeProperty.GetValue(tensor);

                        // Get rank property from shape
                        var rankProperty = shape.GetType().GetProperty("rank");
                        if (rankProperty == null)
                        {
                              Debug.LogError("Failed to find rank property");
                              return result;
                        }

                        int rank = (int)rankProperty.GetValue(shape);

                        // Use indexer to get dimensions
                        var indexer = shape.GetType().GetMethod("get_Item");
                        if (indexer == null)
                        {
                              Debug.LogError("Failed to find shape indexer");
                              return result;
                        }

                        int tensorWidth, tensorHeight, tensorChannels;

                        // Handle different tensor layouts
                        if (rank == 4) // NCHW format (batch, channels, height, width)
                        {
                              tensorChannels = (int)indexer.Invoke(shape, new object[] { 1 });
                              tensorHeight = (int)indexer.Invoke(shape, new object[] { 2 });
                              tensorWidth = (int)indexer.Invoke(shape, new object[] { 3 });
                        }
                        else if (rank == 3) // CHW format (channels, height, width)
                        {
                              tensorChannels = (int)indexer.Invoke(shape, new object[] { 0 });
                              tensorHeight = (int)indexer.Invoke(shape, new object[] { 1 });
                              tensorWidth = (int)indexer.Invoke(shape, new object[] { 2 });
                        }
                        else
                        {
                              Debug.LogError($"Unsupported tensor shape with rank: {rank}");
                              return result;
                        }

                        // Clamp class index to valid range
                        int effectiveClassIndex = Mathf.Clamp(classIndex, 0, tensorChannels - 1);

                        // Fill pixels based on tensor data
                        Color[] pixels = new Color[targetWidth * targetHeight];
                        for (int y = 0; y < targetHeight; y++)
                        {
                              for (int x = 0; x < targetWidth; x++)
                              {
                                    // Scale coordinates to tensor dimensions
                                    int tx = Mathf.FloorToInt((float)x / targetWidth * tensorWidth);
                                    int ty = Mathf.FloorToInt((float)y / targetHeight * tensorHeight);

                                    // Calculate index in tensor data (based on NCHW format)
                                    int index = 0;
                                    if (rank == 4)
                                    {
                                          // NCHW: batch=0, channel=classIndex, y, x
                                          index = ((0 * tensorChannels + effectiveClassIndex) * tensorHeight + ty) * tensorWidth + tx;
                                    }
                                    else
                                    {
                                          // CHW: channel=classIndex, y, x
                                          index = (effectiveClassIndex * tensorHeight + ty) * tensorWidth + tx;
                                    }

                                    // Apply threshold to create binary mask
                                    if (index < tensorData.Length)
                                    {
                                          float value = (float)tensorData[index];
                                          pixels[y * targetWidth + x] = value > threshold ?
                                              maskColor : new Color(0, 0, 0, 0);
                                    }
                                    else
                                    {
                                          pixels[y * targetWidth + x] = new Color(0, 0, 0, 0);
                                    }
                              }
                        }

                        result.SetPixels(pixels);
                        result.Apply();
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"Error creating segmentation texture: {e.Message}");
                  }

                  return result;
            }
      }
}