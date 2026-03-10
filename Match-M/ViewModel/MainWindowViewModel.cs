using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace Match_M.ViewModel;

public class MainWindowViewModel : ObservableObject
{
    private bool _isMainMenuVisible = true;
    private bool _isGameScreenVisible;

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

        // Настройка таймера (тиков в секунду)
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;

        InitBoard();
        ResetGameState();
    }

    public Visibility MainMenuVisibility =>
        _isMainMenuVisible ? Visibility.Visible : Visibility.Collapsed;

    public Visibility GameScreenVisibility =>
        _isGameScreenVisible ? Visibility.Visible : Visibility.Collapsed;

    public int Score
    {
        get => _score;
        set => SetProperty(ref _score, value);
    }

    public string TimeText =>
        $"{_timeLeftSeconds / 60:00}:{_timeLeftSeconds % 60:00}";

    public RelayCommand PlayCommand { get; }

    private void OnPlay()
    {
        _isMainMenuVisible = false;
        _isGameScreenVisible = true;

        OnPropertyChanged(nameof(MainMenuVisibility));
        OnPropertyChanged(nameof(GameScreenVisibility));

        ResetGameState();
        _timer.Start();
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
        }
    }
}
