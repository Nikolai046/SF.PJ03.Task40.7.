# Галерея изображений (SF.PJ03.Task40.7)

## Описание проекта

Это **учебный проект** мобильного приложения "Галерея изображений", разработанного на платформе .NET MAUI. Приложение позволяет пользователям защищать доступ к галерее с помощью PIN-кода, просматривать изображения из хранилища устройства, открывать их для детального просмотра и удалять.

Проект является частью учебной программы и демонстрирует работу с:
* Пользовательским интерфейсом на XAML и C#
* Навигацией между страницами
* Привязками данных и конвертерами значений
* Управлением состоянием приложения
* Механизмами безопасного хранения данных (PIN-код)
* Доступом к файловой системе и медиа-контенту устройства
* Реализацией специфичных для платформы Android возможностей (работа с `MediaStore`, обработка разрешений).

## Основные возможности

* **Установка PIN-кода**: При первом запуске пользователь устанавливает 4-значный PIN-код для защиты доступа.
* **Вход по PIN-коду**: Доступ к основной функциональности галереи осуществляется только после ввода корректного PIN-кода.
* **Просмотр галереи**: Отображение списка изображений из галереи устройства в виде сетки (3 колонки).
* **Выбор изображения**: Пользователь может выбрать изображение в галерее, которое будет выделено рамкой.
* **Просмотр изображения**: Открытие выбранного изображения в полноэкранном режиме с отображением даты и времени его создания.
* **Удаление изображения**: Возможность удаления выбранного изображения из галереи устройства с предварительным подтверждением от пользователя.
* **Индикатор загрузки**: Отображение индикатора активности во время загрузки списка изображений.
* **Обработка разрешений**: Динамический запрос необходимых разрешений для доступа к медиафайлам на Android.

## Целевая платформа

* **Android**

## Среда разработки и отладки

* **Framework**: .NET MAUI (.NET 8)
* **Язык программирования**: C#
* **Разметка интерфейса**: XAML
* **Целевая ОС для отладки**: Android
* **Устройство отладки**: Эмулятор Android 13.0 (API 33)
* **IDE**: (Visual Studio 2022)

## Структура проекта (основные компоненты)

* **Pages/**: Содержит XAML файлы страниц и связанный с ними C# код (логику представления).
    * `SignUpPage.xaml` / `.cs`: Страница создания PIN-кода.
    * `SignInPage.xaml` / `.cs`: Страница ввода PIN-кода.
    * `GalleryPage.xaml` / `.cs`: Страница отображения галереи изображений.
    * `ImageViewerPage.xaml` / `.cs`: Страница просмотра отдельного изображения.
* **Models/**: Определяет классы моделей данных и интерфейсы сервисов.
    * `ImageItem.cs`: Модель, представляющая изображение в галерее.
    * `IGalleryService.cs`: Интерфейс для сервиса работы с галереей.
    * `DefaultGalleryService.cs`: Реализация `IGalleryService` по умолчанию (для не-Android платформ или как заглушка).
* **Services/**: Содержит общие сервисы, например, пользовательские исключения.
    * `GalleryAccessException.cs`: Исключение для ошибок доступа к галерее.
* **Platforms/Android/Services/**: Содержит реализации сервисов, специфичные для Android.
    * `AndroidGalleryService.cs`: Реализация `IGalleryService` для Android, использующая `MediaStore`.
* **Platforms/Android/Helpers/**: (Если был создан) Вспомогательные классы для Android.
    * `PendingIntentRequester.cs`: Класс для обработки асинхронных запросов `PendingIntent` (например, для подтверждения удаления файла на Android 11+).
* **Converters/**: Содержит конвертеры значений для использования в XAML привязках.
    * `InvertedBoolConverter.cs`: Инвертирует логические значения.
* **Корневая директория проекта**:
    * `App.xaml` / `.cs`: Главный класс приложения.
    * `AppShell.xaml` / `.cs`: Оболочка приложения, управляющая начальной навигацией.
    * `MauiProgram.cs`: Точка входа приложения, где происходит его конфигурация и регистрация сервисов.
* **Platforms/Android/**:
    * `MainActivity.cs`: Главная Activity для Android.
    * `AndroidManifest.xml`: Манифест приложения Android, где объявляются разрешения и другие компоненты.

## Установка и запуск

1.  **Клонируйте репозиторий**:
    ```bash
    git clone https://github.com/Nikolai046/SF.PJ03.Task40.7..git
    ```
2.  **Откройте проект**: Откройте файл решения `.sln` в вашей IDE (например, Visual Studio 2022 с установленной рабочей нагрузкой .NET Multi-platform App UI development).
3.  **Настройте среду**: Убедитесь, что у вас установлены необходимые Android SDK и настроен эмулятор (рекомендуется Android 13.0 - API 33 для соответствия среде отладки проекта) или подключено физическое устройство.
4.  **Соберите проект**: Выполните сборку проекта (Build Solution).
5.  **Запустите приложение**: Выберите целевое устройство Android и запустите проект. При первом запуске потребуется установить PIN-код.

## Заметки по разрешениям Android

Приложение реализует логику запроса разрешений во время выполнения для доступа к медиафайлам:
* Для Android 13 (API 33) и выше запрашивается разрешение `READ_MEDIA_IMAGES`.
* Для версий Android ниже 13 запрашивается `READ_EXTERNAL_STORAGE`.
* При удалении изображений на Android 11 (API 30) и выше используется `MediaStore.createDeleteRequest`, что инициирует системный диалог для подтверждения пользователем. На более ранних версиях Android используется прямой вызов `ContentResolver.Delete`.

---
