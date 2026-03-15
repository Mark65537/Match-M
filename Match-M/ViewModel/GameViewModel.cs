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
    private Cell? _firstSelectedCell;
    private Cell? _lastMovedCell;

    private readonly bool _isBonusesActive = false;
    private bool _isResolving;

    public GameViewModel(GameStateService gameStateService)
    {
        _gameStateService = gameStateService;
        _gameStateService.StateChanged += GameState_PropertyChanged;
        _animator = new GameBoardAnimator(Cells);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;

        // Команда всегда доступна, но внутри обработчика мы игнорируем клики,
        // пока идёт ResolveBoard (_isResolving = true).
        ToggleCellSelectionCommand = new RelayCommand<Cell>(OnCellClicked);

        Reset();
        ResolveBoard();
    }

    private int TimeLeftSeconds
    {
        get => _timeLeftSeconds;
        set
        {
            if (SetProperty(ref _timeLeftSeconds, value))
                OnPropertyChanged(nameof(TimeText));
        }
    }

    //TODO возможно нужно переделать в двухмерный массив
    public ObservableCollection<Cell> Cells { get; } = [];

    public RelayCommand<Cell> ToggleCellSelectionCommand { get; }

    public int Score
    {
        get => _score;
        set => SetProperty(ref _score, value);
    }

    public string TimeText => $"{_timeLeftSeconds / 60:00}:{_timeLeftSeconds % 60:00}";

    /// <summary>
    /// Установка начальных значений переменных и инициализация игрового поля
    /// </summary>
    private void Reset()
    {
        Score = 0;
        TimeLeftSeconds = GameConstants.TIME_LIMIT_SECONDS;
        _firstSelectedCell = null;
        InitBoard();
    }

    private void InitBoard()
    {
        if (Cells.Count <= 0)
        {
            for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
                for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
                    Cells.Add(new Cell(r, c, GetRandomShape()));
        }
        else
        {
            foreach (var cell in Cells)
                cell.Shape = GetRandomShape();
        }
    }

    private ShapeType GetRandomShape() => (ShapeType)_random.Next(1, _shapeCount);

    /// <summary>
    /// Prints the board to debug output: shape number (0–5) and Line bonuses (↔ HLine, ↨ VLine);
    /// Bomb is shown as "B" only (no shape, no color).
    /// </summary>
    [Conditional("DEBUG")]
    private void PrintBoard()
    {
        for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
        {
            var line = "";
            for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
            {
                var cell = GetCell(r, c);
                string cellStr = cell.Bonus switch
                {
                    BonusType.Bomb => "B",
                    BonusType.HLine => (int)cell.Shape + "↔",
                    BonusType.VLine => (int)cell.Shape + "↨",
                    _ => ((int)cell.Shape).ToString()
                };
                line += cellStr + (c < GameConstants.BOARD_COLUMNS - 1 ? " " : "");
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
                _timer.Stop();
                break;

            case GameState.InGame:
                Reset();
                ResolveBoard();
                _timer.Start();
                break;
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (--TimeLeftSeconds <= 0)
            _gameStateService.CurrentState = GameState.GameOver;
    }

    private void OnCellClicked(Cell? cell)
    {
        if (_isResolving || cell is null)
            return;

        // выбор первой ячейки
        if (_firstSelectedCell is null)
        {
            cell.IsSelected = true;
            _firstSelectedCell = cell;
            return;
        }

        // если выбрал туже самую ячейку — отменяем
        if (ReferenceEquals(_firstSelectedCell, cell))
        {
            cell.IsSelected = false;
            _firstSelectedCell = null;
            return;
        }

        // если выбрал ячейку рядом — меняем местами
        if (AreNeighbour(_firstSelectedCell, cell))
        {
            TrySwap(_firstSelectedCell, cell);

        }

        _firstSelectedCell.IsSelected = false;
        _firstSelectedCell = null;
        //_firstSelectedCell.IsSelected = false;
        //_firstSelectedCell = cell;
        //cell.IsSelected = true;
    }

    private void TrySwap(Cell first, Cell second)
    {
        (first.Shape, second.Shape) = (second.Shape, first.Shape);

        var matches = FindMatches();
        if (matches.Contains(first) || matches.Contains(second))
        {
            _lastMovedCell = second;
            ResolveBoard(matches);
        }
        else
        {
            // откат
            (first.Shape, second.Shape) = (second.Shape, first.Shape);
        }
    }

    /// <summary>
    /// Проверяет что клетки находяться рядом по вертикали или горизонтали
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

    private HashSet<Cell> FindMatches()
    {
        var result = new HashSet<Cell>();

        void Process(IEnumerable<IReadOnlyList<Cell>> runs)
        {
            foreach (var run in runs)
            {
                if (run.Count < GameConstants.MIN_MATCH_LENGTH)
                    continue;

                foreach (var cell in run)
                    result.Add(cell);
            }
        }

        Process(EnumerateHorizontalRuns());
        Process(EnumerateVerticalRuns());

        return result;
    }


    /// <summary>
    /// Проверяет что одинаковые фигуры находяться рядом по горизонтали
    /// </summary>
    /// <returns>Возвращает последовательно список <see cref="Cell"/> по горизонтали</returns>
    private IEnumerable<IReadOnlyList<Cell>> EnumerateHorizontalRuns()
    {
        for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
            foreach (var run in EnumerateRuns(r, 0, 0, 1))
                yield return run;
    }

    /// <summary>
    /// Проверяет что одинаковые фигуры находяться рядом по вертикали
    /// </summary>
    /// <returns>Возвращает последовательно список <see cref="Cell"/> по вертикали</returns>
    private IEnumerable<IReadOnlyList<Cell>> EnumerateVerticalRuns()
    {
        for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
            foreach (var run in EnumerateRuns(0, c, 1, 0))
                yield return run;
    }

    private IEnumerable<IReadOnlyList<Cell>> EnumerateRuns(int startR, int startC, int stepR, int stepC)
    {
        int r = startR;
        int c = startC;

        while (r < GameConstants.BOARD_ROWS && c < GameConstants.BOARD_COLUMNS)
        {
            var start = GetCell(r, c);
            var shape = start.Shape;

            if (shape == ShapeType.None)
            {
                r += stepR;
                c += stepC;
                continue;
            }

            int len = 1;

            while (true)
            {
                int nr = r + stepR * len;
                int nc = c + stepC * len;

                if (nr >= GameConstants.BOARD_ROWS || nc >= GameConstants.BOARD_COLUMNS)
                    break;

                if (GetCell(nr, nc).Shape != shape)
                    break;

                len++;
            }

            var run = new List<Cell>(len);

            for (int i = 0; i < len; i++)
                run.Add(GetCell(r + stepR * i, c + stepC * i));

            yield return run;

            r += stepR * len;
            c += stepC * len;
        }
    }


    /// <summary>
    /// Активирует уже существующие Line‑бонусы (HLine / VLine), попавшие в матч:
    /// добавляет к очистке всю строку или столбец, включая саму ячейку‑бонус.
    /// </summary>
    private void ActivateLineBonuses(HashSet<Cell> matches, HashSet<Cell> cellsToClear)
    {
        if (!_isBonusesActive)
            return;

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
        if (!_isBonusesActive || _lastMovedCell is null)
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

    private void ResolveBoard()
    {
        var matches = FindMatches();
        ResolveBoard(matches);
    }

    private async void ResolveBoard(HashSet<Cell> matches)
    {
        if (_isResolving)
            return;

        _isResolving = true;

        try
        {
            var currentMatches = matches;

            while (currentMatches.Count > 0)
            {
                var cellsToClear = PrepareCellsToClear(currentMatches);

                await _animator.FadeOutAsync(cellsToClear);

                ClearCells(cellsToClear);

                Score += currentMatches.Count * GameConstants.BASE_SCORE_PER_CELL;

                await ApplyGravity();

                currentMatches = FindMatches();
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

