using UnityEngine;

// Only define this type when Barracuda is not available
#if !UNITY_BARRACUDA

// Using a different namespace to avoid conflicts with Unity.Barracuda
namespace DuluxVisualizer.Compat
{
      /// <summary>
      /// Dummy implementation of NNModel for backward compatibility
      /// This allows existing code to compile without changes
      /// </summary>
      public class DummyNNModel : ScriptableObject
      {
            [SerializeField] private byte[] modelData;
            [SerializeField] private string modelName;

            public byte[] ModelData => modelData;
            public string ModelName => modelName;

            public static implicit operator DummyNNModel(Unity.Sentis.ModelAsset modelAsset)
            {
                  if (modelAsset == null)
                        return null;

                  var model = ScriptableObject.CreateInstance<DummyNNModel>();
                  model.modelName = modelAsset.name;
                  return model;
            }
      }
}

#endif