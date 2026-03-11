using CommunityToolkit.Mvvm.ComponentModel;
using Match_M.Model;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Match_M.ViewModel;

public sealed class GameViewModel : ObservableObject
{
    private int _score;
    private int _timeLeftSeconds;
    private readonly DispatcherTimer _timer;
    private GameState _gameState;


    private const int BoardSize = 8;

    public GameViewModel()
    {
        Score = 9999;
    }

    public GameViewModel(GameState gameState)
    {
        _gameState = gameState;

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

    public void Start() => _timer.Start();

    public void Stop() => _timer.Stop();

    private void EndGame()
    {
        Stop();
        _gameState = GameState.GameOver;
    }

    public void Reset()
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

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_timeLeftSeconds > 0)
        {
            _timeLeftSeconds--;
            OnPropertyChanged(nameof(TimeText));
            return;
        }

        _timer.Stop();
        EndGame();
    }
}

