namespace Match_M.Model;

/// <summary>
/// Центральное место для всех базовых игровых констант.
/// Изменяя значения здесь, можно быстро перенастроить параметры игры.
/// </summary>
public static class GameConstants
{
    /// <summary>
    /// Ширина экрана.
    /// </summary>
    public const double SCREEN_WIDTH = 700;

    /// <summary>
    /// Высота экрана
    /// </summary>
    public const double SCREEN_HEIGHT = 900;

    /// <summary>
    /// Количество строк на игровом поле.
    /// </summary>
    public const int BOARD_ROWS = 8;

    /// <summary>
    /// Количество столбцов на игровом поле.
    /// </summary>
    public const int BOARD_COLUMNS = 8;

    /// <summary>
    /// Высота одной ячейки в пикселях (для анимации падения).
    /// </summary>
    public const double CELL_HEIGHT_PIXELS = 80;

    /// <summary>
    /// Минимальная длина последовательности для совпадения (match).
    /// </summary>
    public const int MIN_MATCH_LENGTH = 3;

    /// <summary>
    /// Базовое количество очков за одну ячейку в совпадении.
    /// </summary>
    public const int BASE_SCORE_PER_CELL = 10;

    /// <summary>
    /// Общее доступное время раунда (в секундах).
    /// </summary>
    public const int TIME_LIMIT_SECONDS = 60;

    /// <summary>
    /// Максимальное количество попыток перетасовать поле,
    /// чтобы получить ход, если ходов нет.
    /// </summary>
    public const int MAX_SHUFFLE_ATTEMPTS = 50;
}

