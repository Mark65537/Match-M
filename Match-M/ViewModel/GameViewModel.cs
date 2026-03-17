using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Match_M.Animations;
using Match_M.Model;
using Match_M.Services;
using System.Windows.Threading;

namespace Match_M.ViewModel;

public sealed class GameViewModel : ObservableObject
{
    private int _score;
    private int _timeLeftSeconds;

    private bool _isResolving;

    private readonly int _shapeCount = Enum.GetValues<ShapeType>().Length;
    private readonly Random _random = new();
    private readonly bool _isBonusesActive = false;

    private readonly DispatcherTimer _timer;
    private readonly GameStateService _gameStateService;
    private readonly GameBoardAnimator _animator;
    private readonly MatchFinderService _matchFinder;
    private readonly BoardDebugService? _boardDebugService;
    private readonly BonusService? _bonusService;

    private Cell? _firstSelectedCell;
    private Cell? _lastMovedCell;
    private readonly Cell[,] _cells = new Cell[GameConstants.BOARD_ROWS, GameConstants.BOARD_COLUMNS];

    public GameViewModel(GameStateService gameStateService)
    {
        _gameStateService = gameStateService;
        _gameStateService.StateChanged += GameState_PropertyChanged;
        _animator = new GameBoardAnimator(_cells);
        _matchFinder = new MatchFinderService(_cells);
        if (_isBonusesActive)
        {
            _bonusService = new BonusService(_cells);
        }

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;

        // Команда всегда доступна, но внутри обработчика мы игнорируем клики,
        // пока идёт ResolveBoard (_isResolving = true).
        ToggleCellSelectionCommand = new RelayCommand<Cell>(OnCellClicked);

#if DEBUG
        Reset();
        _boardDebugService = new(_cells);
        //_boardDebugService.SetBoardWithoutMatches();
        InitBoard();
#else
        ResetAndInit();
#endif

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

    public IEnumerable<Cell> Cells => _cells.Cast<Cell>();

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
    private void ResetAndInit()
    {
        Reset();
        InitBoard();
    }

    /// <summary>
    /// Установка начальных значений переменных
    /// </summary>
    private void Reset()
    {
        Score = 0;
        TimeLeftSeconds = GameConstants.TIME_LIMIT_SECONDS;
        _firstSelectedCell = null;
        _lastMovedCell = null;
        _isResolving = false;
    }

    /// <summary>
    /// Инициализация игрового поля
    /// </summary>
    private void InitBoard()
    {
        for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
        {
            for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
            {
                var cell = new Cell(r, c, GetRandomShape());
                _cells[r, c] = cell;
            }
        }
    }

    private ShapeType GetRandomShape() => (ShapeType)_random.Next(1, _shapeCount);

    private void GameState_PropertyChanged()
    {
        switch (_gameStateService.CurrentState)
        {
            case GameState.Menu:
            case GameState.GameOver:
                _timer.Stop();
                break;

            case GameState.InGame:
                ResetAndInit();
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

        var matches = _matchFinder.FindMatches();
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

    private void ResolveBoard()
    {
        var matches = _matchFinder.FindMatches();
        ResolveBoard(matches);
    }

    private async void ResolveBoard(HashSet<Cell> matches)
    {
        if (_isResolving)
            return;

        _isResolving = true;

        try
        {
            var cellsToClear = matches;

            while (cellsToClear.Count > 0)
            {
                //var cellsToClear = PrepareCellsToClear(currentMatches);

                // анимация исчезновения ячейек
                await _animator.FadeOutAsync(cellsToClear);

                // Удаляем фигуры с ячеек
                ClearCells(cellsToClear);

                // Прибавляем очки
                AddScore(cellsToClear);

                await ApplyGravity();

                cellsToClear = _matchFinder.FindMatches();
            }
        }
        finally
        {
            _isResolving = false;
        }
    }

    /// <summary>
    /// Убирает с ячейки фигуры и бонусы
    /// </summary>
    /// <param name="cells"></param>
    private static void ClearCells(IEnumerable<Cell> cells)
    {
        foreach (var cell in cells)
        {
            cell.Shape = ShapeType.None;
            cell.Bonus = BonusType.None;
        }
    }

    // IMPORTANT функция сделана на тот случай если будет сложный алгоритм подсчета очков
    /// <summary>
    /// Функция которая прибавляет очки
    /// </summary>
    /// <param name="cellsToClearCount"> Количество уничтоженных ячейек</param>
    private void AddScore(HashSet<Cell> cellsToClear)
    {
        Score += cellsToClear.Count * GameConstants.BASE_SCORE_PER_CELL;
    }

    private async Task ApplyGravity()
    {
        var fallMoves = CollectFallMoves();

        if (fallMoves.Count > 0)
            await _animator.AnimateFallsAsync(fallMoves);

        var newCells = MoveCellsDownAndSpawn();

        _animator.ResetAnimations();

        foreach (var cell in newCells)
            cell.Animation = new FadeInAnimation();
    }

    private List<FallMove> CollectFallMoves()
    {
        var moves = new List<FallMove>();

        for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
        {
            int targetRow = GameConstants.BOARD_ROWS - 1;// переменная запоминает в какой строке последний раз была запись

            for (int r = GameConstants.BOARD_ROWS - 1; r >= 0; r--)
            {
                if (_cells[r, c].Shape == ShapeType.None)
                    continue;

                if (targetRow != r)
                    moves.Add(new FallMove(r, targetRow, c));//если фигура не находится на правильной позиции, то нужно добавить её перемещение.

                targetRow--;
            }
        }

        return moves;
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
                var fromCell = _cells[r, c];

                if (fromCell.Shape == ShapeType.None)
                    continue;

                if (writeRow != r)
                {
                    var toCell = _cells[writeRow, c];

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
                var cell = _cells[r, c];

                cell.Shape = GetRandomShape();
                cell.Bonus = BonusType.None;

                newCells.Add(cell);
            }
        }

        return newCells;
    }


}

