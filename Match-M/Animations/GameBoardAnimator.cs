using Match_M.Behaviors;
using Match_M.Model;
using System.Collections.ObjectModel;

namespace Match_M.Animations;

/// <summary>
/// Orchestrates all board-related animations: fade out matches, falling cells, reset, and completion tracking.
/// </summary>
public sealed class GameBoardAnimator
{
    private int _pendingAnimations;
    private TaskCompletionSource _animationsCompletionSource = new();

    private readonly ObservableCollection<Cell> _cells;

    public GameBoardAnimator(ObservableCollection<Cell> cells)
    {
        _cells = cells;
        AnimationBehavior.AnimationCompleted += OnAnimationCompleted;
    }

    private void OnAnimationCompleted(object? sender, EventArgs e)
    {
        if (--_pendingAnimations == 0)
            _animationsCompletionSource.TrySetResult();
    }

    /// <summary>
    /// Runs fade-out animation on the given cells and completes when all are done.
    /// </summary>
    public async Task FadeOutAsync(HashSet<Cell> cellsToAnimate)
    {
        if (cellsToAnimate.Count == 0)
            return;

        _pendingAnimations = cellsToAnimate.Count;
        _animationsCompletionSource = new TaskCompletionSource();

        foreach (var cell in cellsToAnimate)
            cell.Animation = AnimationType.FadeOut;

        await _animationsCompletionSource.Task;
    }

    /// <summary>
    /// Animates cells falling and completes when all fall animations are done.
    /// </summary>
    public async Task AnimateFallsAsync(List<(int fromRow, int toRow, int col)> moves)
    {
        var wait = WaitAnimations(moves.Count);

        foreach (var (fromRow, toRow, col) in moves)
        {
            var cell = _cells[fromRow * GameConstants.BOARD_COLUMNS + col];
            cell.FallDistanceCells = toRow - fromRow;
            cell.Animation = AnimationType.MoveUpDown;
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
            cell.FallDistanceCells = 0;
            cell.Animation = AnimationType.None;
        }
    }

    private Task WaitAnimations(int count)
    {
        if (count == 0)
            return Task.CompletedTask;

        _pendingAnimations = count;
        _animationsCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        return _animationsCompletionSource.Task;
    }
}
