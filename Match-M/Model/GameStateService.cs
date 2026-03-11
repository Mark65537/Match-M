using CommunityToolkit.Mvvm.ComponentModel;

namespace Match_M.Model
{
    /// <summary>
    /// Класс обертка для удобного переключения состояний
    /// </summary>
    public class GameStateService : ObservableObject
    {
        private GameState _currentState;

        public GameState CurrentState
        {
            get => _currentState;
            set => SetProperty(ref _currentState, value);

        }
    }
}
