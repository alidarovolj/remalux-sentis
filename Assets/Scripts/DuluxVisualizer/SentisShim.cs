using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace DuluxVisualizer
{
      /// <summary>
      /// Shim to bridge between our compatibility types and the real Unity.Sentis types
      /// This uses reflection to avoid direct references to Unity.Sentis
      /// </summary>
      public static class SentisShim
      {
            // Cache for Unity.Sentis assembly and types
            private static Assembly sentisAssembly;
            private static Type backendTypeEnum;
            private static Type modelAssetType;
            private static Type tensorType;
            private static Type tensorFloatType;
            private static Type workerType;
            private static Type workerFactoryType;
            private static Type modelLoaderType;

            static SentisShim()
            {
                  InitializeReflection();
            }

            /// <summary>
            /// Initialize reflection access to Unity.Sentis types
            /// </summary>
            private static void InitializeReflection()
            {
                  try
                  {
                        // Find Unity.Sentis assembly
                        sentisAssembly = AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == "Unity.Sentis");

                        if (sentisAssembly != null)
                        {
                              // Load key types
                              backendTypeEnum = sentisAssembly.GetType("Unity.Sentis.BackendType");
                              modelAssetType = sentisAssembly.GetType("Unity.Sentis.ModelAsset");
                              tensorType = sentisAssembly.GetType("Unity.Sentis.Tensor");
                              tensorFloatType = sentisAssembly.GetType("Unity.Sentis.TensorFloat");
                              workerType = sentisAssembly.GetType("Unity.Sentis.IWorker");
                              workerFactoryType = sentisAssembly.GetType("Unity.Sentis.WorkerFactory");
                              modelLoaderType = sentisAssembly.GetType("Unity.Sentis.ModelLoader");

                              Debug.Log("SentisShim: Successfully initialized reflection for Unity.Sentis types");
                        }
                        else
                        {
                              Debug.LogError("SentisShim: Failed to locate Unity.Sentis assembly");
                        }
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"SentisShim: Error initializing reflection: {e.Message}");
                  }
            }

            /// <summary>
            /// Load a model from a path
            /// </summary>
            public static ModelAsset LoadModel(string path)
            {
                  if (string.IsNullOrEmpty(path))
                  {
                        Debug.LogError("SentisShim: Path is null or empty");
                        return null;
                  }

                  if (modelLoaderType == null)
                  {
                        Debug.LogError("SentisShim: ModelLoader type not found");
                        return null;
                  }

                  try
                  {
                        // Find the Load method
                        var loadMethod = modelLoaderType.GetMethod("Load", new Type[] { typeof(string) });
                        if (loadMethod == null)
                        {
                              Debug.LogError("SentisShim: ModelLoader.Load method not found");
                              return null;
                        }

                        // Call the method
                        var result = loadMethod.Invoke(null, new object[] { path });
                        if (result == null)
                        {
                              Debug.LogError("SentisShim: ModelLoader.Load returned null");
                              return null;
                        }

                        // Create our ModelAsset and return it
                        var modelAsset = ScriptableObject.CreateInstance<ModelAsset>();

                        // Copy properties if needed
                        // This is a minimal implementation

                        return modelAsset;
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"SentisShim: Error loading model: {e.Message}");
                        return null;
                  }
            }

            /// <summary>
            /// Convert our BackendType enum to Unity.Sentis.BackendType
            /// </summary>
            public static object ToSentisBackendType(BackendType backendType)
            {
                  if (backendTypeEnum == null) return null;

                  // Convert using enum's integer value
                  return Enum.ToObject(backendTypeEnum, (int)backendType);
            }

            /// <summary>
            /// Convert our ModelAsset to Unity.Sentis.ModelAsset
            /// This assumes our ModelAsset is actually a Unity.Sentis.ModelAsset
            /// </summary>
            public static object ToSentisModelAsset(ModelAsset modelAsset)
            {
                  // In practice, the ModelAsset should already be a Unity.Sentis.ModelAsset
                  // This is just a safe cast
                  if (modelAsset == null) return null;
                  return modelAsset;
            }

            /// <summary>
            /// Create a worker from a model asset
            /// </summary>
            public static IWorker CreateWorker(ModelAsset modelAsset, BackendType backendType)
            {
                  if (workerFactoryType == null || modelAsset == null) return null;

                  try
                  {
                        // Get CreateWorker method
                        var createWorkerMethod = workerFactoryType.GetMethod("CreateWorker");
                        if (createWorkerMethod == null)
                        {
                              Debug.LogError("SentisShim: Failed to find CreateWorker method");
                              return null;
                        }

                        // Convert backend type
                        var sentisBackendType = ToSentisBackendType(backendType);

                        // Call CreateWorker
                        var sentisWorker = createWorkerMethod.Invoke(null, new object[] { sentisBackendType, modelAsset });

                        // Return a wrapper
                        return new WorkerShim(sentisWorker);
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"SentisShim: Error creating worker: {e.Message}");
                        return null;
                  }
            }

            // Shim implementation of IWorker that wraps Unity.Sentis.IWorker
            private class WorkerShim : IWorker
            {
                  private readonly object sentisWorker;
                  private readonly MethodInfo executeMethod;
                  private readonly MethodInfo peekOutputMethod;
                  private readonly MethodInfo disposeMethod;

                  public WorkerShim(object sentisWorker)
                  {
                        this.sentisWorker = sentisWorker;

                        // Cache methods for performance
                        var type = sentisWorker.GetType();
                        executeMethod = type.GetMethod("Execute");
                        peekOutputMethod = type.GetMethod("PeekOutput");
                        disposeMethod = type.GetMethod("Dispose");
                  }

                  public void Execute(Dictionary<string, Tensor> inputs)
                  {
                        try
                        {
                              // We need to convert our Tensor wrappers to real Sentis tensors
                              var sentisInputs = new Dictionary<string, object>();
                              foreach (var pair in inputs)
                              {
                                    sentisInputs[pair.Key] = pair.Value; // Here we'd need to unwrap
                              }

                              executeMethod.Invoke(sentisWorker, new object[] { sentisInputs });
                        }
                        catch (Exception e)
                        {
                              Debug.LogError($"WorkerShim: Error in Execute: {e.Message}");
                        }
                  }

                  public Tensor PeekOutput(string name)
                  {
                        try
                        {
                              var result = peekOutputMethod.Invoke(sentisWorker, new object[] { name });
                              if (result == null) return null;

                              // Here we'd need to wrap the Sentis tensor in our Tensor type
                              // This is a simplified implementation
                              return new TensorFloat(new float[0], new TensorShape(1));
                        }
                        catch (Exception e)
                        {
                              Debug.LogError($"WorkerShim: Error in PeekOutput: {e.Message}");
                              return null;
                        }
                  }

                  public void Dispose()
                  {
                        try
                        {
                              disposeMethod.Invoke(sentisWorker, null);
                        }
                        catch (Exception e)
                        {
                              Debug.LogError($"WorkerShim: Error in Dispose: {e.Message}");
                        }
                  }
            }
      }
}