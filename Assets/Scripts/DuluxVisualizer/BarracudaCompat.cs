/*
 * Legacy compatibility layer for Barracuda/Sentis transition
 * This file is completely disabled now that we're using Unity Sentis 2.1.2 directly
 * Do not use any of these types - use the real Unity Sentis API instead
 */

/*
// This file provides compatibility types for projects that need to support both Barracuda and Sentis
// It should not be included in projects that only use Sentis

// Only define these compatibility types when specifically requested
#if USE_BARRACUDA_COMPATIBILITY && !UNITY_SENTIS

using UnityEngine;

namespace DuluxVisualizer
{
    // Compatibility layer for Barracuda/Sentis
    namespace BarracudaCompat
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
        
        // ModelLoader static class for loading models
        public static class ModelLoader
        {
            public static Model Load(ScriptableObject modelAsset)
            {
                Debug.LogWarning("Using BarracudaCompat - actual model loading functionality is not available");
                return new Model();
            }
        }
    }
}

#endif
*/