using UnityEngine;

namespace DuluxVisualizer
{
      /// <summary>
      /// This class provides methods that safely wrap package-dependent functionality,
      /// with fallbacks when packages are not available.
      /// </summary>
      public static class PackageCompatibilityWrapper
      {
            /// <summary>
            /// Checks if the Sentis package is available
            /// </summary>
            public static bool IsSentisAvailable()
            {
#if UNITY_SENTIS
            return true;
#else
                  return false;
#endif
            }

            /// <summary>
            /// Checks if the AR Foundation package is available
            /// </summary>
            public static bool IsARFoundationAvailable()
            {
#if UNITY_AR_FOUNDATION_PRESENT
            return true;
#else
                  return false;
#endif
            }

            /// <summary>
            /// Checks if the XR Core Utils package is available
            /// </summary>
            public static bool IsXRCoreUtilsAvailable()
            {
#if UNITY_XR_CORE_UTILS_PRESENT
            return true;
#else
                  return false;
#endif
            }

            /// <summary>
            /// Safely returns a ModelAsset from a ScriptableObject, handling the case where Sentis is not available
            /// </summary>
            public static object GetSentisModelAsset(ScriptableObject asset)
            {
#if UNITY_SENTIS
            if (asset is Unity.Sentis.ModelAsset modelAsset)
                return modelAsset;
#endif
                  return null;
            }

            /// <summary>
            /// Creates a compatible tensor object depending on what packages are available
            /// </summary>
            public static object CreateTensor(Texture2D texture, int width, int height)
            {
#if UNITY_SENTIS
            try
            {
                // Create a Sentis tensor from the texture
                var transform = new Unity.Sentis.TextureTransform()
                    .SetDimensions(width, height)
                    .SetTensorLayout(Unity.Sentis.TensorLayout.NCHW);
                
                return Unity.Sentis.TextureConverter.ToTensor(texture, transform);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error creating Sentis tensor: {e.Message}");
            }
#endif
                  // Fallback for when Sentis is not available or there was an error
                  return null;
            }
      }
}