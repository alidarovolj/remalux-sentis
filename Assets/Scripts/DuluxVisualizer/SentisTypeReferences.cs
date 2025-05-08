using UnityEngine;
#if UNITY_SENTIS
using Unity.Sentis;
#endif
using System.Collections.Generic;

namespace DuluxVisualizer
{
      /// <summary>
      /// This class exists solely to directly reference Sentis types so that Unity properly resolves them
      /// </summary>
      public class SentisTypeReferences : MonoBehaviour
      {
#if UNITY_SENTIS
            public Unity.Sentis.ModelAsset modelAsset;
            private Unity.Sentis.IWorker worker;
            private Unity.Sentis.TensorFloat tensorFloat;
            private Unity.Sentis.Tensor tensor;
            private Unity.Sentis.BackendType backendType;
            private Unity.Sentis.TensorShape tensorShape;
            private Unity.Sentis.TextureTransform textureTransform;

            // This method is never meant to be called - it just references all the types
            private void ReferenceAllTypes()
            {
                  if (modelAsset == null) return;

                  backendType = Unity.Sentis.BackendType.CPU;
                  worker = Unity.Sentis.WorkerFactory.CreateWorker(backendType, modelAsset);

                  textureTransform = new Unity.Sentis.TextureTransform()
                      .SetDimensions(256, 256)
                      .SetTensorLayout(Unity.Sentis.TensorLayout.NCHW);

                  Texture2D texture = new Texture2D(256, 256);
                  tensorFloat = Unity.Sentis.TextureConverter.ToTensor(texture, textureTransform);

                  worker.Execute(new Dictionary<string, Unity.Sentis.Tensor> { { "input", tensorFloat } });
                  tensorFloat = worker.PeekOutput("output") as Unity.Sentis.TensorFloat;

                  tensorShape = tensorFloat.shape;
                  var span = tensorFloat.ToReadOnlySpan();

                  worker.Dispose();
                  tensorFloat.Dispose();
            }
#else
            // Placeholders when Unity Sentis is not available
            public ScriptableObject modelAsset;

            private void ReferenceAllTypes()
            {
                  // Empty implementation
            }
#endif
      }
}