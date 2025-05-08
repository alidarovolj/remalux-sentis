using UnityEngine;
#if UNITY_SENTIS
using Unity.Sentis;
#endif

namespace DuluxVisualizer
{
      /// <summary>
      /// Utility class for converting between textures and tensors using Sentis 2.1.2 API
      /// </summary>
      public static class TextureConverterUtils
      {
            /// <summary>
            /// Converts a texture to a tensor with specified dimensions and format
            /// </summary>
#if UNITY_SENTIS
            public static TensorFloat TextureToTensor(Texture2D texture, int width, int height, bool useNCHW = true)
            {
                  if (texture == null)
                        return null;

                  // Create tensor shape based on format
                  TensorShape shape;
                  if (useNCHW)
                  {
                        // Batch, Channels, Height, Width
                        shape = new TensorShape(1, 3, height, width);
                  }
                  else
                  {
                        // Batch, Height, Width, Channels
                        shape = new TensorShape(1, height, width, 3);
                  }

                  // Create empty tensor with the right shape
                  TensorFloat tensor = new TensorFloat(shape);

                  // Create texture transform settings
                  var textureTransform = new TextureTransform()
                      .SetDimensions(width, height)
                      .SetTensorLayout(useNCHW ? TensorLayout.NCHW : TensorLayout.NHWC);

                  // Convert texture to tensor
                  Unity.Sentis.TextureConverter.ToTensor(texture, tensor, textureTransform);

                  return tensor;
            }

            /// <summary>
            /// Extracts a mask from a tensor
            /// </summary>
            public static Texture2D CreateMaskFromTensor(TensorFloat tensor, int width, int height, float threshold = 0.5f)
            {
                  if (tensor == null)
                        return null;

                  Texture2D mask = new Texture2D(width, height, TextureFormat.R8, false);

                  // Get raw data from tensor
                  float[] tensorData = tensor.ToReadOnlySpan().ToArray();
                  Color[] pixels = new Color[width * height];

                  // Determine tensor dimensions
                  TensorShape shape = tensor.shape;
                  int tensorWidth = shape.rank >= 4 ? shape[3] : shape[shape.rank - 1];
                  int tensorHeight = shape.rank >= 4 ? shape[2] : shape[shape.rank - 2];

                  // Fill pixels based on tensor data
                  for (int y = 0; y < height; y++)
                  {
                        for (int x = 0; x < width; x++)
                        {
                              // Scale coordinates to tensor size
                              int tx = Mathf.FloorToInt((float)x / width * tensorWidth);
                              int ty = Mathf.FloorToInt((float)y / height * tensorHeight);

                              // Calculate index in tensor data (assuming single-channel output)
                              int index = ty * tensorWidth + tx;
                              if (index < tensorData.Length)
                              {
                                    // Apply threshold to tensor value
                                    float value = tensorData[index];
                                    float maskValue = value > threshold ? 1.0f : 0.0f;
                                    pixels[y * width + x] = new Color(maskValue, maskValue, maskValue, 1);
                              }
                              else
                              {
                                    // Out of bounds, default to black
                                    pixels[y * width + x] = Color.black;
                              }
                        }
                  }

                  mask.SetPixels(pixels);
                  mask.Apply();
                  return mask;
            }
#else
            public static object TextureToTensor(Texture2D texture, int width, int height, bool useNCHW = true)
            {
                  Debug.LogWarning("TextureConverterUtils: Unity Sentis is not available");
                  return null;
            }

            public static Texture2D CreateMaskFromTensor(object tensor, int width, int height, float threshold = 0.5f)
            {
                  Debug.LogWarning("TextureConverterUtils: Unity Sentis is not available");
                  // Create a simple gradient texture as a fallback
                  Texture2D fallbackTexture = new Texture2D(width, height, TextureFormat.R8, false);
                  Color[] pixels = new Color[width * height];
                  
                  for (int y = 0; y < height; y++)
                  {
                        for (int x = 0; x < width; x++)
                        {
                              float normalizedX = (float)x / width;
                              float normalizedY = (float)y / height;
                              pixels[y * width + x] = new Color(normalizedX, normalizedY, 0, 1);
                        }
                  }
                  
                  fallbackTexture.SetPixels(pixels);
                  fallbackTexture.Apply();
                  return fallbackTexture;
            }
#endif
      }
}