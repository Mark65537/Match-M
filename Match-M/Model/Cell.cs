using CommunityToolkit.Mvvm.ComponentModel;
using Match_M.Animations;

namespace Match_M.Model;
public class Cell : ObservableObject
{
    private ShapeType _shape;
    private bool _isSelected;
    private AnimationType _animation;

    public ShapeType Shape
    {
        get => _shape;
        set => SetProperty(ref _shape, value);
    }

    public AnimationType Animation
    {
        get => _animation;
        set => SetProperty(ref _animation, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public int Row { get; }
    public int Column { get; }

    public Cell(int row, int column, ShapeType shape)
    {
        Row = row;
        Column = column;
        Shape = shape;
    }
}
