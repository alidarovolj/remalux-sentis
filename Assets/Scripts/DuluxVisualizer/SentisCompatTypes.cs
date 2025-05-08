using System;
using System.Collections.Generic;
using UnityEngine;

// This file defines all the Sentis types we need in our codebase without requiring
// direct Unity.Sentis namespace importing, which appears to be causing issues

namespace DuluxVisualizer
{
      // Type aliases/shims for Unity.Sentis types

#if !UNITY_SENTIS
      /// <summary>
      /// Shim for Unity.Sentis.ModelAsset
      /// </summary>
      public class ModelAsset : ScriptableObject
      {
            // Use reflection to get the real ModelAsset instance
            private object realInstance;

            // Method to wrap an existing ModelAsset
            public static ModelAsset FromRealModelAsset(object realModelAsset)
            {
                  var shim = CreateInstance<ModelAsset>();
                  shim.realInstance = realModelAsset;
                  return shim;
            }

            // Access to real instance
            public object RealInstance => realInstance;
      }

      /// <summary>
      /// Shim for Unity.Sentis.IWorker
      /// </summary>
      public interface IWorker : IDisposable
      {
            void Execute(Dictionary<string, object> inputs);
            object PeekOutput(string name);
      }

      /// <summary>
      /// Shim for Unity.Sentis.Tensor
      /// </summary>
      public class Tensor : IDisposable
      {
            // Reference to the real tensor
            protected object realTensor;

            // Create a wrapper for a real tensor
            public static Tensor FromRealTensor(object realTensor)
            {
                  return new Tensor { realTensor = realTensor };
            }

            // Access to real tensor
            public object RealTensor => realTensor;

            public virtual void Dispose()
            {
                  // Use reflection to call Dispose on the real tensor
                  if (realTensor != null)
                  {
                        try
                        {
                              var method = realTensor.GetType().GetMethod("Dispose");
                              if (method != null)
                              {
                                    method.Invoke(realTensor, null);
                              }
                        }
                        catch (Exception e)
                        {
                              Debug.LogError($"Error disposing tensor: {e.Message}");
                        }
                        realTensor = null;
                  }
            }
      }

      /// <summary>
      /// Shim for Unity.Sentis.TensorFloat
      /// </summary>
      public class TensorFloat : Tensor
      {
            // Create a wrapper for a real tensor
            public static new TensorFloat FromRealTensor(object realTensor)
            {
                  return new TensorFloat { realTensor = realTensor };
            }

            // Access data in the tensor
            public float[] Data
            {
                  get
                  {
                        try
                        {
                              // Use reflection to get data
                              var method = realTensor.GetType().GetMethod("ToReadOnlySpan");
                              if (method != null)
                              {
                                    var span = method.Invoke(realTensor, null);
                                    // Convert span to array - this is a simplification
                                    return new float[0]; // Implementation would be more complex in reality
                              }
                              return new float[0];
                        }
                        catch
                        {
                              return new float[0];
                        }
                  }
            }
      }

      /// <summary>
      /// Shim for Unity.Sentis.BackendType
      /// </summary>
      public enum BackendType
      {
            CPU,
            GPUCompute
      }
#endif

      /// <summary>
      /// Utility class to bridge between our shims and real Sentis types
      /// </summary>
      public static class SentisReflectionUtils
      {
            // Cache for loaded types
            private static Type modelAssetType;
            private static Type workerType;
            private static Type tensorType;
            private static Type tensorFloatType;
            private static Type workerfactoryType;
            private static Type backendTypeEnum;

            static SentisReflectionUtils()
            {
                  LoadTypes();
            }

            // Load Sentis types using reflection
            private static void LoadTypes()
            {
                  try
                  {
                        var assembly = AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == "Unity.Sentis");

                        if (assembly != null)
                        {
                              modelAssetType = assembly.GetType("Unity.Sentis.ModelAsset");
                              workerType = assembly.GetType("Unity.Sentis.IWorker");
                              tensorType = assembly.GetType("Unity.Sentis.Tensor");
                              tensorFloatType = assembly.GetType("Unity.Sentis.TensorFloat");
                              workerfactoryType = assembly.GetType("Unity.Sentis.WorkerFactory");
                              backendTypeEnum = assembly.GetType("Unity.Sentis.BackendType");
                        }
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"Error loading Sentis types: {e.Message}");
                  }
            }

            // Load a model using reflection
            public static ModelAsset LoadModel(string path)
            {
                  try
                  {
                        if (modelAssetType != null)
                        {
                              var loadMethod = modelAssetType.GetMethod("Load", new Type[] { typeof(string) });
                              if (loadMethod != null)
                              {
                                    var realModel = loadMethod.Invoke(null, new object[] { path });
                                    if (realModel != null)
                                    {
#if UNITY_SENTIS
                                          return realModel as ModelAsset;
#else
                                          return ModelAsset.FromRealModelAsset(realModel);
#endif
                                    }
                              }
                        }
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"Error loading model: {e.Message}");
                  }

                  return null;
            }

            // Create a worker using reflection
            public static IWorker CreateWorker(ModelAsset model, BackendType backendType)
            {
                  try
                  {
                        if (workerfactoryType != null && modelAssetType != null)
                        {
                              var createMethod = workerfactoryType.GetMethod("CreateWorker");
                              if (createMethod != null)
                              {
                                    var realBackendType = Enum.ToObject(backendTypeEnum, (int)backendType);
                                    var worker = createMethod.Invoke(null, new object[] { realBackendType, model.RealInstance });

                                    // Create an adapter that implements our IWorker interface
                                    return new WorkerAdapter(worker);
                              }
                        }
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"Error creating worker: {e.Message}");
                  }

                  return null;
            }

            // Adapter class for Unity.Sentis.IWorker
            private class WorkerAdapter : IWorker
            {
                  private object realWorker;

                  public WorkerAdapter(object realWorker)
                  {
                        this.realWorker = realWorker;
                  }

                  public void Dispose()
                  {
                        try
                        {
                              var method = realWorker.GetType().GetMethod("Dispose");
                              if (method != null)
                              {
                                    method.Invoke(realWorker, null);
                              }
                              realWorker = null;
                        }
                        catch (Exception e)
                        {
                              Debug.LogError($"Error disposing worker: {e.Message}");
                        }
                  }

                  public void Execute(Dictionary<string, object> inputs)
                  {
                        try
                        {
                              var method = realWorker.GetType().GetMethod("Execute");
                              if (method != null)
                              {
                                    // Convert our inputs to the real format
                                    var realInputs = new Dictionary<string, object>();
                                    foreach (var pair in inputs)
                                    {
                                          if (pair.Value is Tensor tensor)
                                          {
                                                realInputs[pair.Key] = tensor.RealTensor;
                                          }
                                          else
                                          {
                                                realInputs[pair.Key] = pair.Value;
                                          }
                                    }

                                    method.Invoke(realWorker, new object[] { realInputs });
                              }
                        }
                        catch (Exception e)
                        {
                              Debug.LogError($"Error executing model: {e.Message}");
                        }
                  }

                  public object PeekOutput(string name)
                  {
                        try
                        {
                              var method = realWorker.GetType().GetMethod("PeekOutput");
                              if (method != null)
                              {
                                    var result = method.Invoke(realWorker, new object[] { name });
                                    if (result != null)
                                    {
                                          // Wrap the real tensor in our TensorFloat
                                          return TensorFloat.FromRealTensor(result);
                                    }
                              }
                        }
                        catch (Exception e)
                        {
                              Debug.LogError($"Error getting output tensor: {e.Message}");
                        }

                        return null;
                  }
            }
      }
}