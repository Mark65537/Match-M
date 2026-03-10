namespace Match_M.Model;

//Отвечает за переключение режимов игры, в нем НЕ должно быть DEBUG
public enum GameState
{
    Menu = 0,
    InGame = 10,
    GameOver = 20
}
