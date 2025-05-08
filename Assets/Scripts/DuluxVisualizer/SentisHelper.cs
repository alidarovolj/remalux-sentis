using UnityEngine;
#if UNITY_SENTIS
using Unity.Sentis;
#endif

/// <summary>
/// Вспомогательный класс для проверки наличия Unity Sentis
/// </summary>
public static class SentisHelper
{
      /// <summary>
      /// Проверяет доступность Unity Sentis в проекте
      /// </summary>
      /// <returns>true, если Unity Sentis доступен</returns>
      public static bool IsSentisAvailable()
      {
            try
            {
#if UNITY_SENTIS
                  // When Sentis is available, we can directly use its types
                  var backendSupported = BackendType.CPU;
                  Debug.Log($"Unity Sentis доступен. Поддерживаемые бэкенды: {backendSupported}");
                  return true;
#else
                  // Try using reflection to check if Sentis is available
                  var sentisAssembly = System.AppDomain.CurrentDomain.GetAssemblies()
                      .FirstOrDefault(a => a.GetName().Name == "Unity.Sentis");
                      
                  if (sentisAssembly == null)
                  {
                        Debug.LogWarning("Unity Sentis не найден");
                        return false;
                  }

                  Debug.Log("Unity Sentis обнаружен через рефлексию");
                  return true;
#endif
            }
            catch (System.Exception ex)
            {
                  Debug.LogError($"Unity Sentis недоступен или возникла ошибка: {ex.Message}");
                  return false;
            }
      }

      /// <summary>
      /// Получает информацию о доступных бэкендах Sentis
      /// </summary>
      /// <returns>Строка с информацией о доступных бэкендах</returns>
      public static string GetSentisBackendInfo()
      {
            string info = "Unity Sentis Backends:\n";

            // In Sentis 2.x, we check what backends are likely to be supported based on system capabilities
            info += $"CPU: Always supported\n";
            info += $"GPU Compute: {SystemInfo.supportsComputeShaders}\n";

            return info;
      }
}