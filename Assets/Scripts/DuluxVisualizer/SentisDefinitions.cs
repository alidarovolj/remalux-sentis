using UnityEngine;
using System;
using System.Collections.Generic;

namespace DuluxVisualizer
{
      /// <summary>
      /// This file provides conditional type definitions for Sentis types.
      /// If Unity.Sentis is available, it will use the real types directly.
      /// Otherwise, it will use our compatibility types.
      /// </summary>

#if UNITY_SENTIS
      // If Unity.Sentis is available, use the real types
      using Unity.Sentis;

      // Type aliases for clarity
      using ModelAsset = Unity.Sentis.ModelAsset;
      using IWorker = Unity.Sentis.IWorker;
      using Tensor = Unity.Sentis.Tensor;
      using TensorFloat = Unity.Sentis.TensorFloat;
      using BackendType = Unity.Sentis.BackendType;
      using TensorShape = Unity.Sentis.TensorShape;
      using TensorLayout = Unity.Sentis.TensorLayout;

#else
      // If Unity.Sentis is not available, use our compatibility types

      /// <summary>
      /// ModelAsset type compatible with Unity.Sentis.ModelAsset
      /// </summary>
      [Serializable]
      public class ModelAsset : ScriptableObject
      {
            // Properties that match Unity.Sentis.ModelAsset
            public List<string> outputs = new List<string>();
            public List<InputDescription> inputs = new List<InputDescription>();

            [Serializable]
            public class InputDescription
            {
                  public string name;
                  public int[] shape;
                  public string dataType;
            }
      }

      /// <summary>
      /// IWorker interface compatible with Unity.Sentis.IWorker
      /// </summary>
      public interface IWorker : IDisposable
      {
            void Execute(Dictionary<string, Tensor> inputs);
            Tensor PeekOutput(string name);
      }

      /// <summary>
      /// Base Tensor class compatible with Unity.Sentis.Tensor
      /// </summary>
      public class Tensor : IDisposable
      {
            public virtual void Dispose() { }
      }

      /// <summary>
      /// TensorFloat class compatible with Unity.Sentis.TensorFloat
      /// </summary>
      public class TensorFloat : Tensor
      {
            // Simple shape representation
            public TensorShape shape;

            // Data backing
            protected float[] _data;

            // Access to data
            public float[] Data => _data;

            // Constructor
            public TensorFloat(float[] data, TensorShape shape)
            {
                  _data = data;
                  this.shape = shape;
            }

            // Convenience methods
            public ReadOnlySpan<float> ToReadOnlySpan()
            {
                  return new ReadOnlySpan<float>(_data);
            }
      }

      /// <summary>
      /// TensorShape class compatible with Unity.Sentis.TensorShape
      /// </summary>
      public class TensorShape
      {
            private readonly int[] dimensions;

            public int rank => dimensions.Length;

            public TensorShape(params int[] dims)
            {
                  dimensions = dims ?? new int[0];
            }

            public int this[int axis]
            {
                  get => dimensions[axis];
            }

            public override string ToString()
            {
                  return "[" + string.Join(",", dimensions) + "]";
            }
      }

      /// <summary>
      /// BackendType enum compatible with Unity.Sentis.BackendType
      /// </summary>
      public enum BackendType
      {
            CPU,
            GPUCompute
      }

      /// <summary>
      /// TensorLayout enum compatible with Unity.Sentis.TensorLayout
      /// </summary>
      public enum TensorLayout
      {
            NHWC,
            NCHW
      }
#endif
}