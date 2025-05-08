using UnityEngine;
using System.Collections.Generic;
using System;

#if !UNITY_EDITOR
namespace Unity.Sentis
{
      // These types will only be defined if we're not in the editor and the real Sentis types are missing

      /// <summary>
      /// Runtime package fix to ensure Sentis types are available at runtime
      /// </summary>
      [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
      public static class SentisPackageFix
      {
            public static bool hasSentis = false;

            static SentisPackageFix()
            {
                  Debug.Log("Initializing Sentis compatibility types for runtime");

                  try
                  {
                        // Check if real Sentis is available
                        Type modelAssetType = Type.GetType("Unity.Sentis.ModelAsset, Unity.Sentis");
                        Type iWorkerType = Type.GetType("Unity.Sentis.IWorker, Unity.Sentis");

                        hasSentis = (modelAssetType != null) && (iWorkerType != null);

                        if (hasSentis)
                        {
                              Debug.Log("Unity Sentis package found at runtime.");
                        }
                        else
                        {
                              Debug.LogWarning("Unity Sentis not found at runtime. Using compatibility layer.");
                        }
                  }
                  catch (Exception ex)
                  {
                        Debug.LogError($"Error checking Sentis at runtime: {ex.Message}");
                  }
            }
      }
}
#endif