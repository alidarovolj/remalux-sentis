using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// This file provides compatibility assurance for Unity Sentis

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public static class SentisCompat
{
#if UNITY_EDITOR
    static SentisCompat()
    {
        Debug.Log("SentisCompat: Initializing Unity Sentis compatibility layer");
        
        // Check for Sentis package
        bool hasSentis = CheckSentisAvailable();
        
        if (hasSentis)
        {
            Debug.Log("SentisCompat: Unity Sentis package is available");
        }
        else
        {
            Debug.LogError("SentisCompat: Unity Sentis package NOT FOUND! Add it via Package Manager");
        }
    }
#endif

      [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
      static void Initialize()
      {
            Debug.Log("SentisCompat: Runtime initialization");

            // Check Sentis availability at runtime
            bool hasSentis = CheckSentisAvailable();

            if (hasSentis)
            {
                  Debug.Log("SentisCompat: Unity Sentis is available at runtime");
            }
            else
            {
                  Debug.LogError("SentisCompat: Unity Sentis is NOT available at runtime!");
            }
      }

      // Check if Sentis is available
      private static bool CheckSentisAvailable()
      {
            try
            {
                  // Try to access some Sentis types via reflection to avoid direct references
                  var modelAssetType = System.Type.GetType("Unity.Sentis.ModelAsset, Unity.Sentis");
                  var iWorkerType = System.Type.GetType("Unity.Sentis.IWorker, Unity.Sentis");
                  var tensorFloatType = System.Type.GetType("Unity.Sentis.TensorFloat, Unity.Sentis");

                  bool sentisAvailable = (modelAssetType != null) && (iWorkerType != null) && (tensorFloatType != null);

                  if (sentisAvailable)
                  {
                        // Try to create a simple instance using reflection to verify assemblies are loaded
                        var backendTypeType = System.Type.GetType("Unity.Sentis.BackendType, Unity.Sentis");

                        if (backendTypeType != null && backendTypeType.IsEnum)
                        {
                              Debug.Log($"SentisCompat: Verified Sentis enum type: {backendTypeType.Name}");
                              return true;
                        }
                  }

                  return false;
            }
            catch (System.Exception e)
            {
                  Debug.LogError($"SentisCompat: Error checking for Sentis: {e.Message}");
                  return false;
            }
      }
}