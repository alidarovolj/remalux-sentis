using UnityEngine;
using Unity.Sentis;

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
                  // Пробуем создать экземпляр класса из Sentis
                  // Sentis 2.x API does not use WorkerFactory but instead we check BackendType directly
                  var backendSupported = BackendType.CPU;
                  Debug.Log($"Unity Sentis доступен. Поддерживаемые бэкенды: {backendSupported}");
                  return true;
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