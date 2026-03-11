using CommunityToolkit.Mvvm.ComponentModel;
using Match_M.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace Match_M.ViewModel;

public sealed class GameViewModel : ObservableObject
{
    private int _score;
    private int _timeLeftSeconds;
    private readonly DispatcherTimer _timer;
    private readonly GameStateService _gameStateService;

    private const int BoardSize = 8;

    public GameViewModel(GameStateService gameStateService)
    {
        _gameStateService = gameStateService;
        _gameStateService.PropertyChanged += GameStateService_PropertyChanged;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;

        InitBoard();
        Reset();
    }

    public ObservableCollection<int> Cells { get; } = [];

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
    }

    private void InitBoard()
    {
        Cells.Clear();
        for (int i = 0; i < BoardSize * BoardSize; i++)
            Cells.Add(i);
    }

    private void GameStateService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(GameStateService.CurrentState))
            return;

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

