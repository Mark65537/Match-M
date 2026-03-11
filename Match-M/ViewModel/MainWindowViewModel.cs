using CommunityToolkit.Mvvm.ComponentModel;
using Match_M.Model;

namespace Match_M.ViewModel;

public class MainWindowViewModel : ObservableObject
{
    private GameState _gameState = GameState.Menu;

    public MainWindowViewModel()
    {
        Menu = new MenuViewModel(_gameState);
        Game = new GameViewModel(_gameState);
        GameOver = new GameOverViewModel(_gameState);
    }

    public MenuViewModel Menu { get; }
    public GameViewModel Game { get; }
    public GameOverViewModel GameOver { get; }

    public GameState GameState
    {
        get => _gameState;
        set => SetProperty(ref _gameState, value);
    }

}
