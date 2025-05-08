# Wall Segmentation & Repainting System

Эта система предназначена для реализации функциональности сегментации стен и их перекраски в мобильных AR-приложениях с использованием Unity.

## Компоненты системы

Система состоит из следующих основных компонентов:

1. **SentisWallSegmentation.cs** - новый компонент для сегментации стен с использованием Unity Sentis
2. **WallSegmentation.cs** - старый компонент для сегментации стен на основе Unity Barracuda (устаревший)
3. **WallPaintBlit.cs** - компонент для применения эффекта перекраски
4. **WallPaint.shader** - шейдер для перекраски стен с сохранением текстуры
5. **ImprovedWallPaint.shader** - улучшенный шейдер перекраски

## Установка и настройка

### Требования

- Unity 2022.2 или новее
- Установленный пакет Unity Sentis (для нового компонента) или Unity Barracuda (для старого)
- AR Foundation и соответствующие платформенные пакеты (ARCore для Android, ARKit для iOS)
- Universal Render Pipeline (URP)

### Шаги установки

1. Добавьте пакеты Unity Sentis и AR Foundation через Package Manager
2. Настройте проект на использование URP (подробности в документации Unity)
3. Включите компоненты и префабы данной системы в свой проект
4. Создайте пустую AR сцену и настройте ее как указано ниже

### Настройка AR сцены

1. Создайте пустую сцену
2. Добавьте основные AR компоненты:
   - AR Session
   - AR Session Origin
   - AR Camera
   - AR Plane Manager
3. Настройте AR камеру как активную камеру сцены
4. Добавьте компонент SentisWallSegmentation на AR Session Origin
5. Настройте следующие ссылки:
   - Укажите ARCameraManager в соответствующем поле
   - Добавьте ссылку на AR камеру
   - Создайте RenderTexture для вывода маски сегментации
   - Настройте отладочный RawImage для просмотра результатов сегментации

## Использование SentisWallSegmentation

### Режимы работы

SentisWallSegmentation поддерживает три режима работы:

1. **Demo** - демонстрационный режим без использования нейросети
2. **EmbeddedModel** - использование модели, привязанной в инспекторе
3. **ExternalModel** - использование модели из StreamingAssets

### Добавление модели

#### Вариант 1: Embedded Model
1. Импортируйте модель ONNX в проект
2. Настройте ModelAsset в инспекторе SentisWallSegmentation
3. Установите режим EmbeddedModel

#### Вариант 2: External Model
1. Поместите модель в папку StreamingAssets/Models
2. Укажите путь к модели в поле externalModelPath (по умолчанию "Models/model.onnx")
3. Установите режим ExternalModel

### Особенности настройки моделей

Компонент поддерживает два основных типа моделей:

1. **MobileNet** (DeepLabV3+, UNet):
   - Input: "serving_default_input:0"
   - Output: "StatefulPartitionedCall:0"
   - Wall Class Index: 9 (обычно)
   - Формат: NHWC

2. **YOLOv8-seg**:
   - Input: "images"
   - Output: "output0"
   - Wall Class Index: зависит от обучения модели (требуется адаптация)
   - Формат: NCHW

Компонент пытается автоматически определить тип модели и настраивает параметры соответственно.

### Подключение шейдера перекраски

1. Добавьте GameObject с компонентом WallPaintBlit
2. Создайте материал шейдера WallPaint
3. Настройте связь между WallPaintBlit и выходной текстурой сегментации через:
```csharp
// Пример кода для соединения компонентов
wallPaintBlit.maskTexture = wallSegmentation.outputRenderTexture;
```

## Устранение неполадок

### Проблемы с моделью сегментации

- Если модель не распознается, проверьте имена входных и выходных тензоров и укажите их вручную
- Для YOLOv8 убедитесь, что модель экспортирована с опцией сегментации (--task=segment)
- При проблемах с форматом тензора, попробуйте вручную установить useNCHW в true или false

### Проблемы с AR

- Убедитесь что ARCameraManager настроен правильно
- Проверьте разрешения на использование камеры в настройках приложения

### Проблемы с производительностью

- Уменьшите размер входных данных модели (inputWidth, inputHeight)
- Увеличьте интервал обработки (processingInterval)
- Включите использование GPU (по умолчанию)

## Пример кода использования

```csharp
// Получение ссылки на компонент
var wallSegmentation = GetComponent<SentisWallSegmentation>();

// Изменение цвета перекраски
WallPaintBlit paintEffect = FindObjectOfType<WallPaintBlit>();
if (paintEffect != null)
{
    paintEffect.paintColor = new Color(1, 0, 0); // Красный цвет
    paintEffect.opacity = 0.7f; // Прозрачность
}

// Переключение режима сегментации
wallSegmentation.SwitchMode(SentisWallSegmentation.SegmentationMode.EmbeddedModel);

// Проверка текущего режима
bool isDemoMode = wallSegmentation.IsUsingDemoMode();
``` 