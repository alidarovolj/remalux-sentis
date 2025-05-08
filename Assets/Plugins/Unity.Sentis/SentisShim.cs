using UnityEngine;
using System;
using System.Collections.Generic;

// This file provides shims for Sentis types to ensure they're available when needed

namespace DuluxVisualizer
{
      // Forward declarations of Sentis types with direct mapping to Unity.Sentis namespace

      /// <summary>
      /// Forward reference to Unity.Sentis.IWorker
      /// </summary>
      public interface IWorker : IDisposable
      {
            void Execute(Dictionary<string, Tensor> inputs);
            Tensor PeekOutput(string name);
      }

      /// <summary>
      /// Forward reference to Unity.Sentis.Tensor
      /// </summary>
      public class Tensor : IDisposable
      {
            public void Dispose() { }
      }

      /// <summary>
      /// Forward reference to Unity.Sentis.TensorShape
      /// </summary>
      public struct TensorShape
      {
            public int[] shape;
            public int rank;

            public int this[int index] => shape[index];
      }

      /// <summary>
      /// Forward reference to Unity.Sentis.TensorFloat
      /// </summary>
      public class TensorFloat : Tensor
      {
            public TensorShape shape;

            public ReadOnlySpan<float> ToReadOnlySpan()
            {
                  // Forward to Unity.Sentis.TensorFloat
                  var actualTensor = (this as Unity.Sentis.TensorFloat) ??
                      new Unity.Sentis.TensorFloat(new Unity.Sentis.TensorShape());
                  return actualTensor.ToReadOnlySpan();
            }
      }

      /// <summary>
      /// Forward reference to Unity.Sentis.BackendType
      /// </summary>
      public enum BackendType
      {
            CPU,
            GPUCompute
      }

      /// <summary>
      /// Shim class to convert between DuluxVisualizer types and Unity.Sentis types
      /// </summary>
      public static class SentisShim
      {
            /// <summary>
            /// Convert a DuluxVisualizer IWorker to a Unity.Sentis IWorker
            /// </summary>
            public static Unity.Sentis.IWorker ToSentisWorker(IWorker worker)
            {
                  return worker as Unity.Sentis.IWorker;
            }

            /// <summary>
            /// Convert a Unity.Sentis IWorker to a DuluxVisualizer IWorker
            /// </summary>
            public static IWorker FromSentisWorker(Unity.Sentis.IWorker worker)
            {
                  return worker as IWorker;
            }

            /// <summary>
            /// Convert a DuluxVisualizer TensorFloat to a Unity.Sentis TensorFloat
            /// </summary>
            public static Unity.Sentis.TensorFloat ToSentisTensor(TensorFloat tensor)
            {
                  return tensor as Unity.Sentis.TensorFloat;
            }

            /// <summary>
            /// Convert a Unity.Sentis TensorFloat to a DuluxVisualizer TensorFloat
            /// </summary>
            public static TensorFloat FromSentisTensor(Unity.Sentis.TensorFloat tensor)
            {
                  return tensor as TensorFloat;
            }

            /// <summary>
            /// Convert DuluxVisualizer BackendType to Unity.Sentis BackendType
            /// </summary>
            public static Unity.Sentis.BackendType ToSentisBackendType(BackendType backendType)
            {
                  return (Unity.Sentis.BackendType)backendType;
            }
      }
}