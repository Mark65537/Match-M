# Match-M

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white&labelColor=512BD4&color=512BD4)
![WPF](https://img.shields.io/badge/WPF-0C54C2?logo=windows&logoColor=white&labelColor=0C54C2&color=0C54C2)
![CommunityToolkit.Mvvm](https://img.shields.io/badge/CommunityToolkit.Mvvm-8.4.0-2D7D46?labelColor=2D7D46&color=2D7D46)
![Microsoft.Xaml.Behaviors.Wpf](https://img.shields.io/badge/Microsoft.Xaml.Behaviors.Wpf-1.1.77-0078D4?labelColor=0078D4&color=0078D4)
![Visual Studio 2022](https://img.shields.io/badge/Visual%20Studio-2022-5C2D91?logo=visualstudio&logoColor=white&labelColor=5C2D91&color=5C2D91)

## Описание

Игра жанра *match-3* (три-в-ряд) для Windows. Игрок меняет местами соседние фишки, собирает совпадения и активирует бонусы.

## Правила игры

- **Ход**: можно поменять местами **две соседние** фишки.
- **Совпадение (match)**: 3+ одинаковые фишки **в линию** (по горизонтали или вертикали).
- **Очистка**: совпавшие фишки исчезают, а поле затем **дозаполняется** новыми.

## Бонусы

В игре предусмотрены бонусы (см. `BonusType`):

- **Line-бонус** (`HLine` / `VLine`): создаётся при совпадении **4 в ряд** (в зависимости от направления).
  При активации очищает **всю строку** или **весь столбец**.
- **Bomb**: тип бонуса предусмотрен в модели и отображении (иконка бомбочки).

## Структура проекта

```text
Match-M/                   # WPF приложение (игра)
    ├── Animations/            # Анимации ячеек/элементов.
    ├── Behaviors/             # XAML behaviors (в т.ч. для анимаций/интеракций).
    ├── Converters/            # WPF converters
    ├── Model/                 # Модели (например `Cell`, `BonusType`).
    ├── Services/              # игровая логика/сервисы
    ├── View/                  # XAML представления и ресурсы
    └── ViewModel/             # MVVM ViewModels слой
```

## Сборка и запуск

- **Из IDE**: откройте решение/проект и запустите проект `Match-M`.
- **Через CLI (PowerShell)**:

```bash
dotnet restore
dotnet build -c Release
dotnet run --project .\Match-M\Match-M.csproj
```
