using UnityEngine;

public class DirectSentisTest : MonoBehaviour
{
      void Start()
      {
            Debug.Log("DirectSentisTest: Testing Unity.Sentis availability...");

            // The compiler will verify these types exist at compile time
#if UNITY_SENTIS
        Debug.Log("UNITY_SENTIS define is present");
#else
            Debug.Log("UNITY_SENTIS define is NOT present");
#endif

            try
            {
                  // This will be checked at runtime
                  var modelAssetType = System.Type.GetType("Unity.Sentis.ModelAsset, Unity.Sentis");
                  Debug.Log($"ModelAsset type exists: {modelAssetType != null}");

                  if (modelAssetType != null)
                  {
                        Debug.Log($"  Full name: {modelAssetType.FullName}");
                        Debug.Log($"  Assembly: {modelAssetType.Assembly.FullName}");
                  }
            }
            catch (System.Exception e)
            {
                  Debug.LogError($"Error accessing Sentis types: {e.Message}");
            }
      }
}