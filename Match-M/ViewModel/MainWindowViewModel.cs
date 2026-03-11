using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using Match_M.Model;

namespace Match_M.ViewModel;

public class MainWindowViewModel : ObservableObject
{
    private GameState _gameState;

    private int _score;
    private int _timeLeftSeconds;
    private readonly DispatcherTimer _timer;

    /// <summary>
    /// Размер поля в клетках
    /// </summary>
    private const int BOARD_SIZE = 8;

    public ObservableCollection<int> Cells { get; } = [];

    public MainWindowViewModel()
    {
        PlayCommand = new RelayCommand(OnPlay);
        GameOverOkCommand = new RelayCommand(OnGameOverOk);

        // Настройка таймера (тиков в секунду)
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;

        InitBoard();
        ResetGameState();
        GameState = GameState.Menu;
    }

    public GameState GameState
    {
        get => _gameState;
        set => SetProperty(ref _gameState, value);
    }

    public int Score
    {
        get => _score;
        set => SetProperty(ref _score, value);
    }

    public string TimeText =>
        $"{_timeLeftSeconds / 60:00}:{_timeLeftSeconds % 60:00}";

    public RelayCommand PlayCommand { get; }
    public RelayCommand GameOverOkCommand { get; }

    private void OnPlay()
    {
        ResetGameState();
        GameState = GameState.InGame;
        _timer.Start();
    }

    private void OnGameOverOk()
    {
        GameState = GameState.Menu;
    }

    private void InitBoard()
    {
        Cells.Clear();

        for (int i = 0; i < BOARD_SIZE * BOARD_SIZE; i++)
        {
            Cells.Add(i);
        }
    }

    private void ResetGameState()
    {
        Score = 0;
        _timeLeftSeconds = 60;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_timeLeftSeconds > 0)
        {
            _timeLeftSeconds--;
            OnPropertyChanged(nameof(TimeText));
        }
        else
        {
            _timer.Stop();
            GameState = GameState.GameOver;
        }
    }
}
