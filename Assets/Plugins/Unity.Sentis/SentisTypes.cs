using System;
using System.Collections.Generic;
using UnityEngine;

// This file provides placeholder types for Sentis when the actual package is not available
// These only get compiled when Unity.Sentis is not detected

#if !UNITY_SENTIS && !UNITY_EDITOR // Only use these types at runtime when Sentis is unavailable

namespace Unity.Sentis
{
      /// <summary>
      /// Asset type that represents a serialized neural network model
      /// </summary>
      public class ModelAsset : UnityEngine.ScriptableObject
      {
            public List<string> inputs;
            public List<string> outputs;
            public List<string> layers;
            public string name;
      }

      /// <summary>
      /// Worker interface for executing neural network models
      /// </summary>
      public interface IWorker : IDisposable
      {
            void Execute(Dictionary<string, Tensor> inputs);
            Tensor PeekOutput(string name);
            void Dispose();
      }

      /// <summary>
      /// Base tensor class
      /// </summary>
      public class Tensor : IDisposable
      {
            public TensorShape shape;

            public void Dispose() { }
      }

      /// <summary>
      /// Float tensor implementation
      /// </summary>
      public class TensorFloat : Tensor
      {
            public float[] data;

            public ReadOnlySpan<float> ToReadOnlySpan()
            {
                  return new ReadOnlySpan<float>(data);
            }
      }

      /// <summary>
      /// Shape of a tensor
      /// </summary>
      public struct TensorShape
      {
            public int[] shape;
            public int rank;

            public int this[int index]
            {
                  get { return shape[index]; }
            }
      }

      /// <summary>
      /// Backend type for model execution
      /// </summary>
      public enum BackendType
      {
            CPU,
            GPUCompute,
            GPUPixel
      }

      /// <summary>
      /// Factory for creating workers
      /// </summary>
      public static class WorkerFactory
      {
            public static IWorker CreateWorker(BackendType backendType, ModelAsset model)
            {
                  Debug.LogError("Unity Sentis is not properly loaded. Please check your package installation.");
                  return null;
            }
      }

      /// <summary>
      /// Utility for loading models
      /// </summary>
      public static class ModelLoader
      {
            public static ModelAsset Load(string path)
            {
                  Debug.LogError("Unity Sentis is not properly loaded. Please check your package installation.");
                  return null;
            }
      }

      /// <summary>
      /// Tensor layout type
      /// </summary>
      public enum TensorLayout
      {
            NHWC,
            NCHW
      }

      /// <summary>
      /// Texture transform settings for conversion to tensor
      /// </summary>
      public class TextureTransform
      {
            public TextureTransform SetDimensions(int width, int height)
            {
                  return this;
            }

            public TextureTransform SetTensorLayout(TensorLayout layout)
            {
                  return this;
            }

            public TextureTransform SetNormalization(Color mean, Color std)
            {
                  return this;
            }
      }

      /// <summary>
      /// Texture converter utilities
      /// </summary>
      public static class TextureConverter
      {
            public static TensorFloat ToTensor(Texture2D texture, TextureTransform transform)
            {
                  Debug.LogError("Unity Sentis is not properly loaded. Please check your package installation.");
                  return null;
            }
      }
}

#endif