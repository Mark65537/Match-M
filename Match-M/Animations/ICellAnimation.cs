namespace Match_M.Animations;

public interface ICellAnimation
{
    AnimationType Type { get; }
    TimeSpan Duration { get; }
}
