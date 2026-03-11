using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Match_M.Model;

namespace Match_M.ViewModel;

public sealed class GameOverViewModel : ObservableObject
{
    public GameOverViewModel()
    {

    }

    public GameOverViewModel(GameState gameState)
    {
        GameOverOkCommand = new RelayCommand(() =>
        {
            gameState = GameState.Menu;
        });
    }

    public RelayCommand GameOverOkCommand { get; }
}

