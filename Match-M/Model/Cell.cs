using CommunityToolkit.Mvvm.ComponentModel;

namespace Match_M.Model;
public class Cell : ObservableObject
{
    private ShapeType _shape;
    private bool _isSelected;

    public ShapeType Shape
    {
        get => _shape;
        set => SetProperty(ref _shape, value);
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
