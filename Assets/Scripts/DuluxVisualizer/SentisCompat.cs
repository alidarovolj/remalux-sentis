using UnityEngine;
using System;
using System.Collections.Generic;

// Compatibility layer for Sentis when the actual package is not available
// Only defines types when Unity Sentis is not present
#if !UNITY_SENTIS

namespace Unity.Sentis
{
      // Model Asset class (replaces Barracuda's NNModel)
      public class ModelAsset : ScriptableObject
      {
            public byte[] modelData;
            public string modelName;
      }

      // Core Model class
      public class Model
      {
            public List<Input> inputs = new List<Input>();
            public List<string> outputs = new List<string>();
            public List<Layer> layers = new List<Layer>();

            public struct Input
            {
                  public string name;
                  public int[] shape;
            }

            public class Layer
            {
                  public string name;
                  public string type;
                  public string[] inputs;
                  public string[] outputs;
            }

            public T GetTensorByName<T>(string name) where T : class
            {
                  return null;
            }
      }

      // Worker class for model execution
      public class Worker : IDisposable
      {
            public Worker(Model model)
            {
                  Debug.Log("Created dummy Sentis worker");
            }

            public void Dispose()
            {
                  Debug.Log("Disposing dummy Sentis worker");
            }

            public void Schedule(Tensor<float> inputTensor)
            {
                  Debug.Log("Scheduled execution with tensor");
            }

            public void Schedule(Dictionary<string, Tensor<float>> inputs)
            {
                  Debug.Log("Scheduled execution with tensor dictionary");
            }

            public void WaitForCompletion()
            {
                  Debug.Log("Waiting for completion");
            }

            public Tensor<float> PeekOutput(string name)
            {
                  Debug.Log($"Peeking output {name}");
                  return null;
            }

            public string[] GetOutputNames()
            {
                  return new string[] { "output0" };
            }
      }

      // Generic Tensor class
      public class Tensor<T> where T : struct
      {
            public TensorShape shape;
            private T[] data;

            public Tensor(TensorShape shape)
            {
                  this.shape = shape;
                  int size = 1;
                  for (int i = 0; i < shape.length; i++)
                  {
                        size *= shape[i];
                  }
                  data = new T[size];
            }

            public Tensor(params int[] dimensions)
            {
                  shape = new TensorShape(dimensions);
                  int size = 1;
                  for (int i = 0; i < dimensions.Length; i++)
                  {
                        size *= dimensions[i];
                  }
                  data = new T[size];
            }

            public T[] DownloadToArray()
            {
                  return data;
            }

            public void Dispose()
            {
                  // Clean up resources
            }
      }

      // Non-generic Tensor class
      public class Tensor
      {
            public TensorShape shape;
            private float[] data;

            public Tensor(TensorShape shape)
            {
                  this.shape = shape;
                  data = new float[shape.length];
            }

            public Tensor(TensorShape shape, float[] data)
            {
                  this.shape = shape;
                  this.data = data;
            }

            public float[] ToReadOnlyArray()
            {
                  return data;
            }

            public void Dispose()
            {
                  // Clean up resources
            }
      }

      // TensorShape struct
      public struct TensorShape
      {
            public int[] shape;
            public int length => ComputeLength();

            public int batch => shape.Length > 0 ? shape[0] : 0;
            public int height => shape.Length > 2 ? shape[2] : 0;
            public int width => shape.Length > 3 ? shape[3] : 0;
            public int channels => shape.Length > 1 ? shape[1] : 0;

            public TensorShape(params int[] dimensions)
            {
                  shape = dimensions;
            }

            public int this[int index]
            {
                  get => index < shape.Length ? shape[index] : 0;
                  set
                  {
                        if (index < shape.Length)
                              shape[index] = value;
                  }
            }

            private int ComputeLength()
            {
                  if (shape == null || shape.Length == 0)
                        return 0;

                  int length = 1;
                  for (int i = 0; i < shape.Length; i++)
                  {
                        length *= shape[i];
                  }
                  return length;
            }
      }

      // BackendType enum
      public enum BackendType
      {
            CPU,
            GPUCompute,
            GPUPixel
      }

      // Texture conversion utilities
      public class TextureConverter
      {
            public static void ToTensor(RenderTexture texture, Tensor<float> tensor, TextureTransform transform)
            {
                  Debug.Log("Dummy ToTensor conversion");
            }
      }

      // Texture transform class
      public class TextureTransform
      {
            public void SetDimensions(int width, int height, int channels)
            {
                  Debug.Log($"Setting dimensions to {width}x{height}x{channels}");
            }

            public void FlipY()
            {
                  Debug.Log("Flipping Y");
            }
      }

      // Model loader static class
      public static class ModelLoader
      {
            public static Model Load(ModelAsset modelAsset)
            {
                  Debug.Log($"Loading model asset: {modelAsset?.name ?? "null"}");
                  return new Model();
            }
      }
}

#endif