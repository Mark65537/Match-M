using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Match_M.Animations;
using Match_M.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace Match_M.ViewModel;

public sealed class GameViewModel : ObservableObject
{
    private int _score;
    private int _timeLeftSeconds;
    private readonly DispatcherTimer _timer;
    private readonly GameStateService _gameStateService;
    private readonly GameBoardAnimator _animator;
    private readonly int _shapeCount = Enum.GetValues<ShapeType>().Length;
    private static readonly Random _random = new();
    private Cell? _firstSelectedCell = null;
    private Cell? _lastMovedCell = null;

    private bool _isResolving;

    public GameViewModel(GameStateService gameStateService)
    {
        _gameStateService = gameStateService;
        _gameStateService.StateChanged += GameState_PropertyChanged;
        _animator = new GameBoardAnimator(Cells);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;

        // Команда всегда доступна, но внутри обработчика мы игнорируем клики,
        // пока идёт ResolveBoard (IsResolving = true).
        ToggleCellSelectionCommand = new RelayCommand<Cell>(OnCellClicked);

        Reset();
        ResolveBoard();
    }

    public ObservableCollection<Cell> Cells { get; } = [];

    public RelayCommand<Cell> ToggleCellSelectionCommand { get; }

    public int Score
    {
        get => _score;
        set => SetProperty(ref _score, value);
    }

    public string TimeText => $"{_timeLeftSeconds / 60:00}:{_timeLeftSeconds % 60:00}";

    //TODO так ли сильно мне нужны эти функции
    private void Start() => _timer.Start();

    private void Stop() => _timer.Stop();

    private void Reset()
    {
        Score = 0;
        _timeLeftSeconds = GameConstants.TIME_LIMIT_SECONDS;
        OnPropertyChanged(nameof(TimeText));
        _firstSelectedCell = null;
        InitBoard();
    }

    //TODO можно ли не пересоздавать элементы а менять их
    private void InitBoard()
    {
        Cells.Clear();

        for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
            for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
                Cells.Add(new Cell(r, c, GetRandomShape()));
    }

    private ShapeType GetRandomShape() => (ShapeType)_random.Next(1, _shapeCount);

    /// <summary>
    /// Prints the board to debug output with each cell's shape as its numeric value (0=None, 1=Square, 2=Circle, 3=Triangle, 4=Diamond, 5=Star).
    /// </summary>
    [Conditional("DEBUG")]
    private void PrintBoard()
    {
        for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
        {
            var line = "";
            for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
            {
                int shapeNumber = (int)GetCell(r, c).Shape;
                line += shapeNumber + (c < GameConstants.BOARD_COLUMNS - 1 ? " " : "");
            }
            Debug.WriteLine(line);
        }
        Debug.WriteLine("");
    }

    private void GameState_PropertyChanged()
    {
        switch (_gameStateService.CurrentState)
        {
            case GameState.Menu:
            case GameState.GameOver:
                Stop();
                break;

            case GameState.InGame:
                Reset();
                ResolveBoard();
                Start();
                break;
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (--_timeLeftSeconds > 0)
        {
            OnPropertyChanged(nameof(TimeText));
            return;
        }

        Stop();
        _gameStateService.CurrentState = GameState.GameOver;
    }

    private void OnCellClicked(Cell? cell)
    {
        if (_isResolving)
            return;

        if (cell is null)
            return;

        if (_firstSelectedCell is null)
        {
            ClearSelection();
            cell.IsSelected = true;
            _firstSelectedCell = cell;
            return;
        }

        if (ReferenceEquals(_firstSelectedCell, cell))
        {
            ClearSelection();
            _firstSelectedCell = null;
            return;
        }

        if (AreNeighbour(_firstSelectedCell, cell))
        {
            var first = _firstSelectedCell;
            var second = cell;

            (second.Shape, first.Shape) = (first.Shape, second.Shape);

            var matches = FindMatches();
            if (matches.Contains(first) || matches.Contains(second))
            {
                _lastMovedCell = second;
                ResolveBoard();
            }
            else
            {
                // если ход не приводит к совпадению — откатываем
                (second.Shape, first.Shape) = (first.Shape, second.Shape);
            }
        }

        ClearSelection();
        _firstSelectedCell = null;
    }

    private void ClearSelection()
    {
        foreach (var c in Cells)
            c.IsSelected = false;
    }

    /// <summary>
    /// Проверяет что клетки соседи по вертикали или горизонтали
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private static bool AreNeighbour(Cell a, Cell b)
    {
        int dr = Math.Abs(a.Row - b.Row);
        int dc = Math.Abs(a.Column - b.Column);
        return (dr + dc) == 1;
    }

    private static int GetIndex(int row, int column) =>
        row * GameConstants.BOARD_COLUMNS + column;

    private Cell GetCell(int row, int column) => Cells[GetIndex(row, column)];

    private IEnumerable<IReadOnlyList<Cell>> EnumerateHorizontalRuns()
    {
        for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
        {
            int c = 0;
            while (c < GameConstants.BOARD_COLUMNS)
            {
                var start = GetCell(r, c);
                var shape = start.Shape;

                if (shape == ShapeType.None)
                {
                    c++;
                    continue;
                }

                int runStart = c;
                int runLen = 1;
                while (c + runLen < GameConstants.BOARD_COLUMNS && GetCell(r, c + runLen).Shape == shape)
                    runLen++;

                var runCells = new List<Cell>(runLen);
                for (int k = 0; k < runLen; k++)
                    runCells.Add(GetCell(r, runStart + k));

                yield return runCells;

                c = runStart + runLen;
            }
        }
    }

    private IEnumerable<IReadOnlyList<Cell>> EnumerateVerticalRuns()
    {
        for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
        {
            int r = 0;
            while (r < GameConstants.BOARD_ROWS)
            {
                var start = GetCell(r, c);
                var shape = start.Shape;

                if (shape == ShapeType.None)
                {
                    r++;
                    continue;
                }

                int runStart = r;
                int runLen = 1;
                while (r + runLen < GameConstants.BOARD_ROWS && GetCell(r + runLen, c).Shape == shape)
                    runLen++;

                var runCells = new List<Cell>(runLen);
                for (int k = 0; k < runLen; k++)
                    runCells.Add(GetCell(runStart + k, c));

                yield return runCells;

                r = runStart + runLen;
            }
        }
    }

    private HashSet<Cell> FindMatches()
    {
        var result = new HashSet<Cell>();

        foreach (var run in EnumerateHorizontalRuns())
        {
            if (run.Count < GameConstants.MIN_MATCH_LENGTH)
                continue;

            foreach (var cell in run)
                result.Add(cell);
        }

        foreach (var run in EnumerateVerticalRuns())
        {
            if (run.Count < GameConstants.MIN_MATCH_LENGTH)
                continue;

            foreach (var cell in run)
                result.Add(cell);
        }

        return result;
    }

    /// <summary>
    /// Активирует уже существующие Line‑бонусы (HLine / VLine), попавшие в матч:
    /// добавляет к очистке всю строку или столбец, включая саму ячейку‑бонус.
    /// </summary>
    private void ActivateLineBonuses(HashSet<Cell> matches, HashSet<Cell> cellsToClear)
    {
        foreach (var cell in matches)
        {
            switch (cell.Bonus)
            {
                case BonusType.HLine:
                    for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
                        cellsToClear.Add(GetCell(cell.Row, c));
                    break;

                case BonusType.VLine:
                    for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
                        cellsToClear.Add(GetCell(r, cell.Column));
                    break;
            }
        }
    }

    /// <summary>
    /// Ищет комбинации ровно из четырёх одинаковых фигур, которые включают
    /// последний сдвинутый элемент, и превращает этот элемент в бонус Line
    /// Сам бонус из очистки исключается (остается на поле).
    /// </summary>
    private void CreateLineBonuses(HashSet<Cell> matches, HashSet<Cell> cellsToClear)
    {
        if (_lastMovedCell is null)
            return;

        var last = _lastMovedCell;

        // По горизонтали
        foreach (var run in EnumerateHorizontalRuns())
        {
            if (run.Count != 4)
                continue;

            if (run.Contains(last) && matches.IsSupersetOf(run))
            {
                // Бонус создаётся в ячейке, которую двигали последней
                last.Bonus = BonusType.HLine;

                // Не удаляем бонус с поля
                cellsToClear.Remove(last);
                return;
            }
        }

        // По вертикали
        foreach (var run in EnumerateVerticalRuns())
        {
            if (run.Count != 4)
                continue;

            if (run.Contains(last) && matches.IsSupersetOf(run))
            {
                last.Bonus = BonusType.VLine;

                cellsToClear.Remove(last);
                return;
            }
        }
    }

    private async void ResolveBoard()
    {
        if (_isResolving)
            return;

        _isResolving = true;

        try
        {
            while (true)
            {
                var matches = FindMatches();
                if (matches.Count == 0)
                    break;

                var cellsToClear = PrepareCellsToClear(matches);

                await _animator.FadeOutAsync(cellsToClear);

                ClearCells(cellsToClear);

                Score += matches.Count * GameConstants.BASE_SCORE_PER_CELL;

                await ApplyGravity();
            }
        }
        finally
        {
            _isResolving = false;
        }
    }

    private HashSet<Cell> PrepareCellsToClear(HashSet<Cell> matches)
    {
        var cellsToClear = new HashSet<Cell>(matches);

        ActivateLineBonuses(matches, cellsToClear);

        if (_lastMovedCell is not null)
        {
            CreateLineBonuses(matches, cellsToClear);
            _lastMovedCell = null;
        }

        return cellsToClear;
    }

    private static void ClearCells(IEnumerable<Cell> cells)
    {
        foreach (var cell in cells)
        {
            cell.Shape = ShapeType.None;
            cell.Bonus = BonusType.None;
        }
    }

    private async Task ApplyGravity()
    {
        var fallMoves = CollectFallMoves();

        if (fallMoves.Count > 0)
            await _animator.AnimateFallsAsync(fallMoves);

        var newCells = MoveCellsDownAndSpawn();

        _animator.ResetAnimations();

        foreach (var cell in newCells)
            cell.Animation = AnimationType.FadeIn;
    }

    private List<Cell> MoveCellsDownAndSpawn()
    {
        var newCells = new List<Cell>();

        for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
        {
            int writeRow = GameConstants.BOARD_ROWS - 1;

            // перенос существующих фигур вниз
            for (int r = GameConstants.BOARD_ROWS - 1; r >= 0; r--)
            {
                var fromCell = GetCell(r, c);

                if (fromCell.Shape == ShapeType.None)
                    continue;

                if (writeRow != r)
                {
                    var toCell = GetCell(writeRow, c);

                    toCell.Shape = fromCell.Shape;
                    toCell.Bonus = fromCell.Bonus;

                    fromCell.Shape = ShapeType.None;
                    fromCell.Bonus = BonusType.None;
                }

                writeRow--;
            }

            // создаём новые фигуры сверху
            for (int r = writeRow; r >= 0; r--)
            {
                var cell = GetCell(r, c);

                cell.Shape = GetRandomShape();
                cell.Bonus = BonusType.None;

                newCells.Add(cell);
            }
        }

        return newCells;
    }

    private List<(int fromRow, int toRow, int col)> CollectFallMoves()
    {
        var moves = new List<(int, int, int)>();

        for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
        {
            int writeRow = GameConstants.BOARD_ROWS - 1;

            for (int r = GameConstants.BOARD_ROWS - 1; r >= 0; r--)
            {
                if (GetCell(r, c).Shape == ShapeType.None)
                    continue;

                if (writeRow != r)
                    moves.Add((r, writeRow, c));

                writeRow--;
            }
        }

        return moves;
    }

}

