using Match_M.Model;
using System.Diagnostics;

namespace Match_M.Services;

public class BoardDebugService(Cell[,] cells)
{
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
}
