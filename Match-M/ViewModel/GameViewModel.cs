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

    public GameViewModel(GameStateService gameStateService)
    {
        _gameStateService = gameStateService;
        _gameStateService.StateChanged += GameState_PropertyChanged;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;

        ToggleCellSelectionCommand = new RelayCommand<Cell>((cell) =>
        {
            if (cell is null)
                return;

            cell.IsSelected = !cell.IsSelected;
        });

        InitBoard();
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

    private void Start()
    {
        if (!_timer.IsEnabled)
            _timer.Start();
    }

    private void Stop()
    {
        if (_timer.IsEnabled)
            _timer.Stop();
    }

    private void Reset()
    {
        Score = 0;
        _timeLeftSeconds = 60;
        OnPropertyChanged(nameof(TimeText));
        InitBoard();
    }

    private void InitBoard()
    {
        Cells.Clear();

        int shapeCount = Enum.GetValues<ShapeType>().Length;

        for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
        {
            for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
            {
                ShapeType shape = (ShapeType)_random.Next(shapeCount);

                Cells.Add(new Cell(r, c, shape));
            }
        }
    }

    private void GameState_PropertyChanged()
    {
        switch (_gameStateService.CurrentState)
        {
            case GameState.Menu:
                Stop();
                break;
            case GameState.InGame:
                Reset();
                Start();
                break;
            case GameState.GameOver:
                Stop();
                break;
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_timeLeftSeconds > 0)
        {
            _timeLeftSeconds--;
            OnPropertyChanged(nameof(TimeText));
            return;
        }

        Stop();
        _gameStateService.CurrentState = GameState.GameOver;
    }
}

