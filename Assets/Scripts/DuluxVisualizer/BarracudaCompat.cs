using UnityEngine;
using Unity.Sentis;

// This file provides compatibility types for Barracuda when it's replaced with Sentis
namespace Unity.Sentis
{
      // Basic asset for a serialized model
      public class ModelAsset : ScriptableObject
      {
      }

      // Worker for neural network model execution
      public class Worker : System.IDisposable
      {
            public void Dispose()
            {
            }
      }

      // Model class
      public class Model
      {
            public string[] inputs { get; set; }
            public string[] outputs { get; set; }
      }

      // Basic Tensor compatibility
      public class Tensor<T> where T : struct
      {
            private T[] data;

            public Tensor(int[] shape)
            {
                  int size = 1;
                  foreach (int dim in shape)
                  {
                        size *= dim;
                  }
                  data = new T[size];
            }

            public T[] ToReadOnlyArray()
            {
                  return data;
            }
      }
}

// Only defines these types when Barracuda is not available
#if !UNITY_BARRACUDA

namespace Unity.Barracuda
{
      // Basic Tensor compatibility
      public class Tensor
      {
            public TensorShape shape { get; private set; }
            private float[] data;

            public Tensor(TensorShape shape)
            {
                  this.shape = shape;
                  this.data = new float[shape.length];
            }

            public Tensor(int batch, int height, int width, int channels)
            {
                  shape = new TensorShape(batch, height, width, channels);
                  data = new float[shape.length];
            }

            public int[] ComputeAxisStrides(int dimensions)
            {
                  int[] strides = new int[dimensions];
                  strides[dimensions - 1] = 1;
                  for (int i = dimensions - 2; i >= 0; i--)
                  {
                        strides[i] = strides[i + 1] * shape[i + 1];
                  }
                  return strides;
            }

            public float[] AsFloats()
            {
                  return data;
            }
      }

      // Basic Model compatibility
      public class Model
      {
            public string[] inputs { get; set; }
            public string[] outputs { get; set; }
      }

      // Basic TensorShape compatibility
      public struct TensorShape
      {
            public int[] shape;
            public int length => ComputeLength();

            public TensorShape(params int[] shape)
            {
                  this.shape = shape;
            }

            public int this[int index]
            {
                  get => shape[index];
                  set => shape[index] = value;
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

      // Basic IWorker compatibility
      public interface IWorker : System.IDisposable
      {
            Tensor Execute(Tensor input);
            void Execute(Tensor input, Tensor output);
      }

      // ONNX namespace compatibility
      namespace ONNX
      {
            public class ONNXModelConverter
            {
                  public ONNXModelConverter(bool optimizeModel = true, bool treatErrorsAsWarnings = false, bool forceArbitraryBatchSize = false)
                  {
                        Debug.LogWarning("Using Barracuda ONNX compatibility layer - actual ONNXModelConverter functionality is not available");
                  }

                  public Model Convert(byte[] onnxModel)
                  {
                        Debug.LogWarning("Using Barracuda ONNX compatibility layer - actual conversion functionality is not available");
                        return new Model();
                  }
            }
      }

      // ModelLoader static class for loading models
      public static class ModelLoader
      {
            public static Model Load(ScriptableObject modelAsset)
            {
                  Debug.LogWarning("Using Barracuda compatibility layer - actual model loading functionality is not available");
                  return new Model();
            }
      }
}

#endif