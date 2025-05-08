using UnityEngine;
using System;
using System.Collections;

namespace DuluxVisualizer
{
      /// <summary>
      /// Сервис для обеспечения работы приложения при проблемах с Unity Sentis
      /// Автоматически переключает режим на демонстрационный, если возникают ошибки с Sentis
      /// </summary>
      public class SentisFallbackService : MonoBehaviour
      {
            [Header("Настройки резервирования")]
            [SerializeField] private bool enableFallbackMode = true;
            [SerializeField] private float checkInterval = 5f;
            [SerializeField] private bool showNotifications = true;

            [Header("Компоненты для перенаправления")]
            [SerializeField] private MonoBehaviour sentisWallSegmentation;
            [SerializeField] private MonoBehaviour demoWallSegmentation;

            private bool isSentisAvailable = false;
            private int errorCount = 0;
            private const int ERROR_THRESHOLD = 3;

            private void Start()
            {
                  // Проверяем доступность Sentis при запуске
                  CheckSentisAvailability();

                  // Запускаем периодическую проверку
                  if (enableFallbackMode)
                  {
                        StartCoroutine(PeriodicCheck());
                  }
            }

            private void CheckSentisAvailability()
            {
                  isSentisAvailable = SentisPackageSupport.IsSentisAvailable();

                  if (!isSentisAvailable)
                  {
                        Debug.LogWarning("Unity Sentis недоступен. Будет использован демонстрационный режим.");
                        SwitchToDemoMode();
                  }
                  else
                  {
                        // Проверяем, что все необходимые типы доступны
                        bool typesAvailable = SentisPackageSupport.CheckSentisAssemblyReferences();
                        if (!typesAvailable)
                        {
                              Debug.LogWarning("Некоторые типы Unity Sentis недоступны. Будет использован демонстрационный режим.");
                              SwitchToDemoMode();
                        }
                  }
            }

            public void RegisterError(string errorMessage)
            {
                  errorCount++;
                  Debug.LogError($"Ошибка Sentis ({errorCount}/{ERROR_THRESHOLD}): {errorMessage}");

                  if (errorCount >= ERROR_THRESHOLD && enableFallbackMode)
                  {
                        Debug.LogWarning($"Достигнут порог ошибок ({ERROR_THRESHOLD}). Переключение в демонстрационный режим.");
                        SwitchToDemoMode();
                  }
            }

            private void SwitchToDemoMode()
            {
                  if (sentisWallSegmentation != null)
                  {
                        sentisWallSegmentation.enabled = false;
                        Debug.Log("Sentis компонент отключен");
                  }

                  if (demoWallSegmentation != null)
                  {
                        demoWallSegmentation.enabled = true;
                        Debug.Log("Демо компонент включен");
                  }

                  // Вызываем метод переключения в демо-режим через отражение
                  if (sentisWallSegmentation != null)
                  {
                        try
                        {
                              var method = sentisWallSegmentation.GetType().GetMethod("SwitchToDemoMode",
                                  System.Reflection.BindingFlags.Instance |
                                  System.Reflection.BindingFlags.Public |
                                  System.Reflection.BindingFlags.NonPublic);

                              if (method != null)
                              {
                                    method.Invoke(sentisWallSegmentation, null);
                                    Debug.Log("Вызван метод SwitchToDemoMode");
                              }
                        }
                        catch (Exception ex)
                        {
                              Debug.LogError($"Ошибка при вызове SwitchToDemoMode: {ex.Message}");
                        }
                  }

                  // Отображаем уведомление
                  if (showNotifications)
                  {
                        ShowFallbackNotification();
                  }
            }

            private void ShowFallbackNotification()
            {
                  // Здесь можно добавить код для отображения UI-уведомления пользователю
                  Debug.Log("Показано уведомление: Приложение работает в демонстрационном режиме из-за проблем с Unity Sentis");
            }

            private IEnumerator PeriodicCheck()
            {
                  while (true)
                  {
                        yield return new WaitForSeconds(checkInterval);

                        // Выполняем проверку только если мы еще не переключились в демо-режим
                        if (sentisWallSegmentation != null && sentisWallSegmentation.enabled)
                        {
                              CheckSentisAvailability();
                        }
                  }
            }

            /// <summary>
            /// Метод для подключения к компоненту сегментации
            /// </summary>
            public void ConnectToSegmentation(MonoBehaviour sentisComponent, MonoBehaviour demoComponent)
            {
                  sentisWallSegmentation = sentisComponent;
                  demoWallSegmentation = demoComponent;
                  Debug.Log("Подключены компоненты сегментации к сервису резервирования");
            }
      }
}