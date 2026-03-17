using CommunityToolkit.Mvvm.ComponentModel;
using Match_M.Animations;
using System.Diagnostics;

namespace Match_M.Model;
[DebuggerDisplay("Row: {Row} Column: {Column} Shape: {Shape}({(int)Shape}) Bonus: {Bonus} IsSelected: {IsSelected}")]
public class Cell : ObservableObject
{
    private ShapeType _shape;
    private bool _isSelected;
    private ICellAnimation? _animation;
    private BonusType _bonus;

    public int Row { get; }
    public int Column { get; }

    public ShapeType Shape
    {
        get => _shape;
        set => SetProperty(ref _shape, value);
    }

    public ICellAnimation? Animation
    {
        get => _animation;
        set => SetProperty(ref _animation, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// Тип бонуса в ячейке (линия, бомба и т.п.).
    /// </summary>
    public BonusType Bonus
    {
        get => _bonus;
        set => SetProperty(ref _bonus, value);
    }

    public Cell(int row, int column, ShapeType shape)
    {
        Row = row;
        Column = column;
        Shape = shape;
    }
}
