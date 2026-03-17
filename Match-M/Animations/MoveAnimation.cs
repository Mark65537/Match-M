namespace Match_M.Animations;
/// <summary>
/// Собирает всю информацию для запуска анимации Move.
/// </summary>
public class MoveAnimation : ICellAnimation
{
    public MoveAnimation(int deltaRows)
    {
        DeltaRows = deltaRows;
    }
    public MoveAnimation(int deltaRows, TimeSpan duration)
    {
        DeltaRows = deltaRows;
        Duration = duration;
    }
    public MoveAnimation(double toX, double toY, TimeSpan duration)
    {
        ToX = toX;
        ToY = toY;
        Duration = duration;
    }
    public AnimationType Type => AnimationType.MoveUpDown;

    /// <summary>
    /// Смещение по строкам (в клетках). Используется, чтобы посчитать пиксели из фактического размера UI-ячейки.
    /// </summary>
    public int DeltaRows { get; init; }

    public double ToX { get; init; }
    public double ToY { get; init; }

    public TimeSpan Duration { get; init; } = TimeSpan.FromMilliseconds(300);
}
