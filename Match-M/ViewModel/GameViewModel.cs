using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Match_M.Model;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Match_M.ViewModel;

public sealed class GameViewModel : ObservableObject
{
    private int _score;
    private int _timeLeftSeconds;
    private readonly DispatcherTimer _timer;
    private readonly GameStateService _gameStateService;
    private static readonly Random _random = new();
    private Cell? _firstSelectedCell = null;

    public GameViewModel(GameStateService gameStateService)
    {
        _gameStateService = gameStateService;
        _gameStateService.StateChanged += GameState_PropertyChanged;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;

        ToggleCellSelectionCommand = new RelayCommand<Cell>(OnCellClicked);

        Reset();
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
        _timeLeftSeconds = 60;
        OnPropertyChanged(nameof(TimeText));
        _firstSelectedCell = null;
        InitBoard();
    }

    //TODO можно ли не пересоздавать элементы а менять их
    private void InitBoard()
    {
        Cells.Clear();

        int shapeCount = Enum.GetValues<ShapeType>().Length;

        for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
        {
            for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
            {
                ShapeType shape = (ShapeType)_random.Next(1, shapeCount);
                Cells.Add(new Cell(r, c, shape));
            }
        }

        ResolveBoard();
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
            (cell.Shape, _firstSelectedCell.Shape) = (_firstSelectedCell.Shape, cell.Shape);
            ResolveBoard();
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
        (row * GameConstants.BOARD_COLUMNS) + column;

    private Cell GetCell(int row, int column) => Cells[GetIndex(row, column)];

    private HashSet<Cell> FindMatches()
    {
        var result = new HashSet<Cell>();

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

                if (runLen >= GameConstants.MIN_MATCH_LENGTH)
                {
                    for (int k = 0; k < runLen; k++)
                        result.Add(GetCell(r, runStart + k));
                }

                c = runStart + runLen;
            }
        }

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

                if (runLen >= GameConstants.MIN_MATCH_LENGTH)
                {
                    for (int k = 0; k < runLen; k++)
                        result.Add(GetCell(runStart + k, c));
                }

                r = runStart + runLen;
            }
        }

        return result;
    }

    private void ResolveBoard()
    {
        int shapeCount = Enum.GetValues<ShapeType>().Length;

        while (true)
        {
            var matches = FindMatches();
            if (matches.Count == 0)
                return;

            foreach (var cell in matches)
                cell.Shape = ShapeType.None;

            Score += matches.Count * GameConstants.BASE_SCORE_PER_CELL;

            for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
            {
                int writeRow = GameConstants.BOARD_ROWS - 1;

                for (int r = GameConstants.BOARD_ROWS - 1; r >= 0; r--)
                {
                    var shape = GetCell(r, c).Shape;
                    if (shape == ShapeType.None)
                        continue;

                    if (writeRow != r)
                    {
                        GetCell(writeRow, c).Shape = shape;
                        GetCell(r, c).Shape = ShapeType.None;
                    }

                    writeRow--;
                }

                for (int r = writeRow; r >= 0; r--)
                    GetCell(r, c).Shape = (ShapeType)_random.Next(1, shapeCount); ;
            }
        }
    }
}

