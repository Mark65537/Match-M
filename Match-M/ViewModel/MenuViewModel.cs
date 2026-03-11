using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Match_M.Model;

namespace Match_M.ViewModel;

public sealed class MenuViewModel : ObservableObject
{
    public MenuViewModel()
    {

    }

    public MenuViewModel(GameState gameState)
    {
        PlayCommand = new RelayCommand(() =>
        {
            gameState = GameState.InGame;
        });
    }

    public RelayCommand PlayCommand { get; }

}

