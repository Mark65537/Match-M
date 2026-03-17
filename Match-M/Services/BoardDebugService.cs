using Match_M.Model;
using System.Diagnostics;

namespace Match_M.Services;

public class BoardDebugService(Cell[,] cells)
{
    [Conditional("DEBUG")]
    public void SetBoardWithOneMatche()
    {
        int[,] data =
        {
            {1,2,3,4,5,1,2,3},//1
            {2,3,3,4,5,1,2,1},//2
            {2,4,5,1,2,3,4,2},//3
            {2,2,3,4,5,1,2,3},//4
            {3,2,3,4,5,1,2,3},//5
            {4,3,4,5,1,2,3,1},//6
            {5,2,3,4,5,1,2,3},//7
            {1,2,3,4,5,1,2,3}//8
        };

        ConvertToCells(data);
    }
    /// <summary>
    /// Устанавливает <see cref="cells"/> в состояние без матчей.
    /// </summary>
    [Conditional("DEBUG")]
    public void SetBoardWithoutMatches()
    {
        ShapeType[] shapes = Enum.GetValues<ShapeType>();
        int shapesCount = shapes.Length - 1;//не учитываем пустую ячейку

        for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
        {
            for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
            {
                int index = ((r + c) % shapesCount) + 1;
                var shape = shapes[index];

                cells[r, c] = new Cell(r, c, shape);
            }
        }
    }

    [Conditional("DEBUG")]
    public void ValidateBoardHasNoMatches()
    {
        for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
        {
            for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
            {
                var shape = cells[r, c].Shape;

                if (c >= 2 &&
                    cells[r, c - 1].Shape == shape &&
                    cells[r, c - 2].Shape == shape)
                {
                    throw new Exception($"Horizontal match at {r},{c}");
                }

                if (r >= 2 &&
                    cells[r - 1, c].Shape == shape &&
                    cells[r - 2, c].Shape == shape)
                {
                    throw new Exception($"Vertical match at {r},{c}");
                }
            }
        }
    }

    /// <summary>
    /// Prints the board to debug output: shape number (0–5) and Line bonuses (↔ HLine, ↨ VLine);
    /// Bomb is shown as "B" only (no shape, no color).
    /// </summary>
    [Conditional("DEBUG")]
    public void PrintBoard()
    {
        for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
        {
            var line = "";
            for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
            {
                var cell = cells[r, c];
                string cellStr = cell.Bonus switch
                {
                    BonusType.Bomb => "B",
                    BonusType.HLine => (int)cell.Shape + "↔",
                    BonusType.VLine => (int)cell.Shape + "↨",
                    _ => ((int)cell.Shape).ToString()
                };
                line += cellStr + (c < GameConstants.BOARD_COLUMNS - 1 ? " " : "");
            }
            Debug.WriteLine(line);
        }
        Debug.WriteLine("");
    }

    public void ConvertToCells(int[,] source)
    {
        int rows = source.GetLength(0);
        int cols = source.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var shape = (ShapeType)source[r, c];
                cells[r, c] = new Cell(r, c, shape);
            }
        }
    }
}
