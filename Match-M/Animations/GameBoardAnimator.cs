using Match_M.Behaviors;
using Match_M.Model;

namespace Match_M.Animations;

/// <summary>
/// Orchestrates all board-related animations: fade out matches, falling cells, reset, and completion tracking.
/// </summary>
public sealed class GameBoardAnimator
{
    private int _cellsToAnimateCount;
    private TaskCompletionSource _animationsCompletionSource = new();

    private readonly Cell[,] _cells;

    public GameBoardAnimator(Cell[,] cells)
    {
        _cells = cells;
        AnimationBehavior.AnimationCompleted += OnAnimationCompleted;
    }

    private void OnAnimationCompleted(object? sender, EventArgs e)
    {
        if (--_cellsToAnimateCount <= 0)
            _animationsCompletionSource.TrySetResult();
    }

    /// <summary>
    /// Runs fade-out animation on the given cells and completes when all are done.
    /// </summary>
    public async Task FadeOutAsync(HashSet<Cell> cellsToAnimate)
    {
        if (cellsToAnimate.Count <= 0)
            return;

        _cellsToAnimateCount = cellsToAnimate.Count;
        _animationsCompletionSource = new TaskCompletionSource();

        foreach (var cell in cellsToAnimate)
            cell.Animation = new FadeOutAnimation();

        await _animationsCompletionSource.Task;// ∆дем пока все анимации завершатьс€
    }

    /// <summary>
    /// Animates cells falling and completes when all fall animations are done.
    /// </summary>
    public async Task AnimateFallsAsync(List<FallMove> moves)
    {
        var wait = WaitAnimations(moves.Count);

        foreach (var fallMove in moves)
        {
            var cell = _cells[fallMove.FromRow, fallMove.Col];
            var deltaRows = fallMove.ToRow - fallMove.FromRow;

            // ToY will be derived in behavior from DeltaRows (using actual UI cell size).
            cell.Animation = new MoveAnimation(
                deltaRows,
                TimeSpan.FromMilliseconds(120 + (Math.Abs(deltaRows) * 70))
                );
        }

        await wait;
    }

    /// <summary>
    /// Clears fall distance and animation type on all cells.
    /// </summary>
    public void ResetAnimations()
    {
        foreach (var cell in _cells)
        {
            cell.Animation = null;
        }
    }

    private Task WaitAnimations(int count)
    {
        if (count == 0)
            return Task.CompletedTask;

        _cellsToAnimateCount = count;
        _animationsCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        return _animationsCompletionSource.Task;
    }
}
