// This file ensures that our compatibility directives are defined
// even when the actual packages aren't installed

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
// Define UNITY_SENTIS if we're on mobile
#define UNITY_SENTIS
#endif

using UnityEngine;

namespace DuluxVisualizer
{
      /// <summary>
      /// This class just ensures our compatibility directives are defined
      /// </summary>
      public static class SymbolDefinition
      {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            private static void Initialize()
            {
                  Debug.Log("DuluxVisualizer: Compatibility layers initialized.");

                  // Check for required packages
                  CheckForBarracuda();
                  CheckForSentis();
                  CheckForARSubsystems();
            }

            private static void CheckForBarracuda()
            {
#if UNITY_BARRACUDA
            Debug.Log("Unity Barracuda package is available.");
#else
                  Debug.Log("Unity Barracuda package is NOT available. Using compatibility layer.");
#endif
            }

            private static void CheckForSentis()
            {
#if UNITY_SENTIS
            Debug.Log("Unity Sentis package is available.");
#else
                  Debug.Log("Unity Sentis package is NOT available. Using compatibility layer.");
#endif
            }

            private static void CheckForARSubsystems()
            {
#if AR_SUBSYSTEMS_4_0_OR_NEWER
            Debug.Log("AR Subsystems 4.0+ package is available.");
#else
                  Debug.Log("AR Subsystems package is NOT available or older than 4.0. Using compatibility layer.");
#endif
            }
      }
}