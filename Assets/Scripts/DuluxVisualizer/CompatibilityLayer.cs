using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

#if UNITY_AR_FOUNDATION_PRESENT
using UnityEngine.XR.ARFoundation;
// Не будем напрямую ссылаться на ARSubsystems
#endif

namespace DuluxVisualizer
{
      /// <summary>
      /// Compatibility layer that provides type definitions and utilities for
      /// Unity Sentis and AR Foundation, allowing the application to compile
      /// and run with or without these packages installed.
      /// </summary>
      public static class CompatibilityLayer
      {
            private static bool isSentisAvailable;
            private static bool isARFoundationAvailable;

            // Reflection helpers for Sentis
            private static Assembly sentisAssembly;
            private static Type modelAssetType;
            private static Type tensorType;
            private static Type tensorFloatType;
            private static Type backendTypeType;
            private static Type modelLoaderType;
            private static Type workerFactoryType;

            // Reflection helpers for AR Foundation
            private static Assembly arFoundationAssembly;
            private static Type arSessionType;
            private static Type arCameraManagerType;
            private static Type arPlaneType;
            private static Type arPlaneManagerType;

            /// <summary>
            /// Initialize the compatibility layer
            /// </summary>
            static CompatibilityLayer()
            {
                  // Check for Sentis
                  CheckSentisAvailability();

                  // Check for AR Foundation
                  CheckARFoundationAvailability();

                  Debug.Log($"CompatibilityLayer: Sentis available: {isSentisAvailable}, AR Foundation available: {isARFoundationAvailable}");
            }

            /// <summary>
            /// Check if Unity Sentis is available
            /// </summary>
            private static void CheckSentisAvailability()
            {
                  try
                  {
                        // Find Unity.Sentis assembly
                        sentisAssembly = AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == "Unity.Sentis");

                        if (sentisAssembly != null)
                        {
                              // Load key types
                              modelAssetType = sentisAssembly.GetType("Unity.Sentis.ModelAsset");
                              tensorType = sentisAssembly.GetType("Unity.Sentis.Tensor");
                              tensorFloatType = sentisAssembly.GetType("Unity.Sentis.TensorFloat");
                              backendTypeType = sentisAssembly.GetType("Unity.Sentis.BackendType");
                              modelLoaderType = sentisAssembly.GetType("Unity.Sentis.ModelLoader");
                              workerFactoryType = sentisAssembly.GetType("Unity.Sentis.WorkerFactory");

                              isSentisAvailable = modelAssetType != null && tensorType != null;

                              if (isSentisAvailable)
                              {
                                    Debug.Log("CompatibilityLayer: Unity Sentis is available");
                              }
                              else
                              {
                                    Debug.LogWarning("CompatibilityLayer: Unity Sentis assembly found, but types are missing");
                              }
                        }
                        else
                        {
                              Debug.LogWarning("CompatibilityLayer: Unity Sentis assembly not found");
                              isSentisAvailable = false;
                        }
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"CompatibilityLayer: Error checking Sentis availability: {e.Message}");
                        isSentisAvailable = false;
                  }
            }

            /// <summary>
            /// Check if AR Foundation is available
            /// </summary>
            private static void CheckARFoundationAvailability()
            {
                  try
                  {
                        // Find Unity.XR.ARFoundation assembly
                        arFoundationAssembly = AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == "Unity.XR.ARFoundation");

                        if (arFoundationAssembly != null)
                        {
                              // Load key types
                              arSessionType = arFoundationAssembly.GetType("UnityEngine.XR.ARFoundation.ARSession");
                              arCameraManagerType = arFoundationAssembly.GetType("UnityEngine.XR.ARFoundation.ARCameraManager");
                              arPlaneType = arFoundationAssembly.GetType("UnityEngine.XR.ARFoundation.ARPlane");
                              arPlaneManagerType = arFoundationAssembly.GetType("UnityEngine.XR.ARFoundation.ARPlaneManager");

                              isARFoundationAvailable = arSessionType != null && arCameraManagerType != null;

                              if (isARFoundationAvailable)
                              {
                                    Debug.Log("CompatibilityLayer: AR Foundation is available");
                              }
                              else
                              {
                                    Debug.LogWarning("CompatibilityLayer: AR Foundation assembly found, but types are missing");
                              }
                        }
                        else
                        {
                              Debug.LogWarning("CompatibilityLayer: AR Foundation assembly not found");
                              isARFoundationAvailable = false;
                        }
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"CompatibilityLayer: Error checking AR Foundation availability: {e.Message}");
                        isARFoundationAvailable = false;
                  }
            }

            /// <summary>
            /// Checks if Unity Sentis is available
            /// </summary>
            public static bool IsSentisAvailable => isSentisAvailable;

            /// <summary>
            /// Checks if AR Foundation is available
            /// </summary>
            public static bool IsARFoundationAvailable => isARFoundationAvailable;

            /// <summary>
            /// Load a Sentis model from a file path
            /// </summary>
            public static ModelAsset LoadModel(string path)
            {
                  if (!isSentisAvailable || modelLoaderType == null)
                  {
                        Debug.LogError("CompatibilityLayer: Sentis is not available, cannot load model");
                        return null;
                  }

                  try
                  {
                        // Find the Load method
                        var loadMethod = modelLoaderType.GetMethod("Load", new[] { typeof(string) });
                        if (loadMethod == null)
                        {
                              Debug.LogError("CompatibilityLayer: ModelLoader.Load method not found");
                              return null;
                        }

                        // Call the method
                        var result = loadMethod.Invoke(null, new object[] { path });
                        if (result == null)
                        {
                              Debug.LogError("CompatibilityLayer: ModelLoader.Load returned null");
                              return null;
                        }

                        // Create our ModelAsset wrapper
                        var modelAsset = ScriptableObject.CreateInstance<ModelAsset>();

                        // Set the real model instance
                        var field = typeof(ModelAsset).GetField("realInstance", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (field != null)
                        {
                              field.SetValue(modelAsset, result);
                        }

                        return modelAsset;
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"CompatibilityLayer: Error loading model: {e.Message}");
                        return null;
                  }
            }

            /// <summary>
            /// Create a worker for Sentis model inference
            /// </summary>
            public static IWorker CreateWorker(ModelAsset modelAsset, BackendType backendType)
            {
                  if (!isSentisAvailable || workerFactoryType == null)
                  {
                        Debug.LogError("CompatibilityLayer: Sentis is not available, cannot create worker");
                        return null;
                  }

                  try
                  {
                        // Get real model instance
                        var field = typeof(ModelAsset).GetField("realInstance", BindingFlags.NonPublic | BindingFlags.Instance);
                        var realModel = field?.GetValue(modelAsset);

                        if (realModel == null)
                        {
                              Debug.LogError("CompatibilityLayer: Real model instance is null");
                              return null;
                        }

                        // Convert backend type
                        var realBackendType = Enum.ToObject(backendTypeType, (int)backendType);

                        // Get CreateWorker method
                        var createWorkerMethod = workerFactoryType.GetMethod("CreateWorker");
                        if (createWorkerMethod == null)
                        {
                              Debug.LogError("CompatibilityLayer: WorkerFactory.CreateWorker method not found");
                              return null;
                        }

                        // Call the method
                        var worker = createWorkerMethod.Invoke(null, new[] { realBackendType, realModel });
                        if (worker == null)
                        {
                              Debug.LogError("CompatibilityLayer: WorkerFactory.CreateWorker returned null");
                              return null;
                        }

                        // Create wrapper
                        return new WorkerImpl(worker);
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"CompatibilityLayer: Error creating worker: {e.Message}");
                        return null;
                  }
            }

            /// <summary>
            /// Create a tensor from a texture
            /// </summary>
            public static TensorFloat TextureToTensor(Texture texture, int width, int height)
            {
                  if (!isSentisAvailable || tensorFloatType == null)
                  {
                        Debug.LogError("CompatibilityLayer: Sentis is not available, cannot create tensor");
                        return null;
                  }

                  try
                  {
                        // Get the TextureConverter type
                        var textureConverterType = sentisAssembly.GetType("Unity.Sentis.TextureConverter");
                        if (textureConverterType == null)
                        {
                              Debug.LogError("CompatibilityLayer: TextureConverter type not found");
                              return null;
                        }

                        // Get the TextureTransform type
                        var textureTransformType = sentisAssembly.GetType("Unity.Sentis.TextureTransform");
                        if (textureTransformType == null)
                        {
                              Debug.LogError("CompatibilityLayer: TextureTransform type not found");
                              return null;
                        }

                        // Create a TextureTransform instance
                        var textureTransform = Activator.CreateInstance(textureTransformType);

                        // Set dimensions
                        var setDimensionsMethod = textureTransformType.GetMethod("SetDimensions", new[] { typeof(int), typeof(int) });
                        if (setDimensionsMethod != null)
                        {
                              textureTransform = setDimensionsMethod.Invoke(textureTransform, new object[] { width, height });
                        }

                        // Call ToTensor
                        var toTensorMethod = textureConverterType.GetMethod("ToTensor", new[] { typeof(Texture), textureTransformType });
                        if (toTensorMethod == null)
                        {
                              Debug.LogError("CompatibilityLayer: TextureConverter.ToTensor method not found");
                              return null;
                        }

                        var realTensor = toTensorMethod.Invoke(null, new[] { texture, textureTransform });
                        if (realTensor == null)
                        {
                              Debug.LogError("CompatibilityLayer: TextureConverter.ToTensor returned null");
                              return null;
                        }

                        // Create wrapper
                        var tensorFloat = new TensorFloat(new float[width * height * 3], new TensorShape(1, height, width, 3));
                        tensorFloat.realTensor = realTensor;
                        return tensorFloat;
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"CompatibilityLayer: Error creating tensor from texture: {e.Message}");
                        return null;
                  }
            }

            // Implementation of IWorker using reflection
            private class WorkerImpl : IWorker
            {
                  private object realWorker;

                  public WorkerImpl(object realSentisWorker)
                  {
                        this.realWorker = realSentisWorker;
                  }

                  public void Execute(Dictionary<string, Tensor> inputs)
                  {
                        if (realWorker == null) return;

                        try
                        {
                              // Convert our inputs to real Sentis inputs
                              var realInputs = new Dictionary<string, object>();
                              foreach (var input in inputs)
                              {
                                    var tensor = input.Value as TensorFloat;
                                    if (tensor != null && tensor.realTensor != null)
                                    {
                                          realInputs[input.Key] = tensor.realTensor;
                                    }
                              }

                              // Find the Execute method
                              var method = realWorker.GetType().GetMethod("Execute",
                                  new[] { typeof(Dictionary<string, object>) });

                              if (method != null)
                              {
                                    method.Invoke(realWorker, new object[] { realInputs });
                              }
                        }
                        catch (Exception e)
                        {
                              Debug.LogError($"Error executing worker: {e.Message}");
                        }
                  }

                  public Tensor PeekOutput(string name)
                  {
                        if (realWorker == null) return null;

                        try
                        {
                              // Find the PeekOutput method
                              var method = realWorker.GetType().GetMethod("PeekOutput", new[] { typeof(string) });
                              if (method != null)
                              {
                                    var result = method.Invoke(realWorker, new object[] { name });
                                    if (result != null)
                                    {
                                          // Create a tensor wrapper
                                          var tensor = new TensorFloat(new float[1], new TensorShape(1));
                                          tensor.realTensor = result;
                                          return tensor;
                                    }
                              }
                        }
                        catch (Exception e)
                        {
                              Debug.LogError($"Error peeking output: {e.Message}");
                        }

                        return null;
                  }

                  public void Dispose()
                  {
                        if (realWorker == null) return;

                        try
                        {
                              // Find the Dispose method
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
            }
      }

      /// <summary>
      /// ModelAsset type compatible with Unity.Sentis.ModelAsset
      /// </summary>
      [Serializable]
      public class ModelAsset : ScriptableObject
      {
            // This field is used to store the real Unity.Sentis.ModelAsset instance
            private object realInstance;

            // Properties for compatibility
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
            private readonly float[] data;
            public TensorShape shape { get; private set; }
            internal object realTensor;

            public float[] Data => data;

            public TensorFloat(float[] data, TensorShape shape)
            {
                  this.data = data;
                  this.shape = shape;
            }

            public ReadOnlySpan<float> ToReadOnlySpan()
            {
                  return new ReadOnlySpan<float>(data);
            }

            public override void Dispose()
            {
                  // If we have a real tensor, dispose it
                  if (realTensor != null)
                  {
                        try
                        {
                              var method = realTensor.GetType().GetMethod("Dispose");
                              method?.Invoke(realTensor, null);
                        }
                        catch (Exception e)
                        {
                              Debug.LogError($"Error disposing tensor: {e.Message}");
                        }
                  }
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

            public int this[int axis] => dimensions[axis];

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

#if !UNITY_AR_FOUNDATION_PRESENT
      #region AR Foundation Compatibility Types

      /// <summary>
      /// Simulated AR Session
      /// </summary>
      public class ARSession : MonoBehaviour
      {
            private static ARSession _instance;
            public bool enabled { get; set; } = true;
            public static ARSession instance => _instance;

            private void Awake()
            {
                  _instance = this;
            }

            public void Reset() { }
      }

      /// <summary>
      /// Simulated AR Camera Manager
      /// </summary>
      public class ARCameraManager : MonoBehaviour
      {
            private event Action<ARCameraFrameEventArgs> _frameReceived;
            public delegate void FrameReceivedEventHandler(ARCameraFrameEventArgs eventArgs);
            public event FrameReceivedEventHandler frameReceived
            {
                  add { _frameReceived += value; }
                  remove { _frameReceived -= value; }
            }

            public Camera GetComponent()
            {
                  return GetComponent<Camera>();
            }

            public void RaiseFrameReceivedEvent()
            {
                  _frameReceived?.Invoke(new ARCameraFrameEventArgs());
            }
      }

      /// <summary>
      /// Simulated AR Camera Frame Event Args
      /// </summary>
      public struct ARCameraFrameEventArgs
      {
            public Texture2D texture;
            public Matrix4x4 displayMatrix;
      }

      /// <summary>
      /// Simulated AR Plane
      /// </summary>
      public class ARPlane : MonoBehaviour
      {
            public TrackingState trackingState { get; set; } = TrackingState.Tracking;
            public Pose pose { get; set; } = new Pose(Vector3.zero, Quaternion.identity);
            public Vector2 size { get; set; } = new Vector2(1, 1);
            public PlaneClassification classification { get; set; } = PlaneClassification.Wall;
            public GameObject gameObject => base.gameObject;
      }

      /// <summary>
      /// Simulated AR Plane Manager
      /// </summary>
      public class ARPlaneManager : MonoBehaviour
      {
            private List<ARPlane> _trackables = new List<ARPlane>();
            public bool enabled { get; set; } = true;

            public List<ARPlane> trackables => _trackables;

            public ARPlane GetPlane(TrackableId trackableId)
            {
                  return _trackables.Count > 0 ? _trackables[0] : null;
            }
      }

      /// <summary>
      /// Simulated AR Raycast Manager
      /// </summary>
      public class ARRaycastManager : MonoBehaviour
      {
            public bool Raycast(Vector2 screenPoint, List<ARRaycastHit> hitResults, TrackableType trackableTypes)
            {
                  // Simplified implementation
                  if (hitResults != null)
                  {
                        hitResults.Clear();
                        DuluxARRaycastHit hit = new DuluxARRaycastHit(
                              new DuluxTrackableId(1, 1),
                              new Pose(Vector3.zero, Quaternion.identity),
                              0.0f,
                              TrackableType.Planes);

                        hitResults.Add(new ARRaycastHit
                        {
                              pose = hit.pose,
                              trackableId = hit.trackableId
                        });
                        return true;
                  }
                  return false;
            }
      }

      /// <summary>
      /// Helper struct to prevent ambiguity
      /// </summary>
      public struct DuluxARRaycastHit
      {
            public DuluxTrackableId trackableId;
            public Pose pose;
            public float distance;
            public TrackableType hitType;

            public DuluxARRaycastHit(DuluxTrackableId trackableId, Pose pose, float distance, TrackableType hitType)
            {
                  this.trackableId = trackableId;
                  this.pose = pose;
                  this.distance = distance;
                  this.hitType = hitType;
            }
      }

      /// <summary>
      /// Simulated AR Raycast Hit result
      /// </summary>
      public struct ARRaycastHit
      {
            public DuluxTrackableId trackableId;
            public Pose pose;
            public float distance;
            public TrackableType hitType;
      }

      /// <summary>
      /// Trackable identifier - использует другое имя полей для устранения неоднозначности
      /// </summary>
      public struct DuluxTrackableId : IEquatable<DuluxTrackableId>
      {
            private readonly ulong _id1;
            private readonly ulong _id2;

            public DuluxTrackableId(ulong id1, ulong id2)
            {
                  _id1 = id1;
                  _id2 = id2;
            }

            public bool Equals(DuluxTrackableId other)
            {
                  return _id1 == other._id1 && _id2 == other._id2;
            }

            public override bool Equals(object obj)
            {
                  if (!(obj is DuluxTrackableId)) return false;
                  return Equals((DuluxTrackableId)obj);
            }

            public override int GetHashCode()
            {
                  return (_id1.GetHashCode() * 397) ^ _id2.GetHashCode();
            }

            public static bool operator ==(DuluxTrackableId a, DuluxTrackableId b) => a.Equals(b);
            public static bool operator !=(DuluxTrackableId a, DuluxTrackableId b) => !a.Equals(b);
      }

      /// <summary>
      /// Plane classifications
      /// </summary>
      public enum PlaneClassification
      {
            None = 0,
            Wall = 1,
            Floor = 2,
            Ceiling = 3,
            Table = 4,
            Seat = 5
      }

      /// <summary>
      /// Tracking states
      /// </summary>
      public enum TrackingState
      {
            None = 0,
            Limited = 1,
            Tracking = 2
      }

      /// <summary>
      /// Trackable types for raycasting
      /// </summary>
      [Flags]
      public enum TrackableType
      {
            None = 0,
            PlaneWithinPolygon = 1 << 0,
            PlaneWithinBounds = 1 << 1,
            PlaneEstimated = 1 << 2,
            Planes = PlaneWithinPolygon | PlaneWithinBounds | PlaneEstimated,
            FeaturePoint = 1 << 3,
            All = Planes | FeaturePoint
      }

      #endregion
#endif
}