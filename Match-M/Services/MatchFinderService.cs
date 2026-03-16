using Match_M.Model;

namespace Match_M.Services;

public class MatchFinderService(Cell[,] cells)
{
    public HashSet<Cell> FindMatches()
    {
        var result = new HashSet<Cell>();

        void Process(IEnumerable<IReadOnlyList<Cell>> runs)
        {
            foreach (var run in runs)
            {
                if (run.Count < GameConstants.MIN_MATCH_LENGTH)
                    continue;

                foreach (var cell in run)
                    result.Add(cell);
            }
        }

        Process(EnumerateHorizontalRuns());
        Process(EnumerateVerticalRuns());

        return result;
    }

    /// <summary>
    /// Проверяет что одинаковые фигуры находяться рядом по горизонтали
    /// </summary>
    /// <returns>Возвращает последовательно список <see cref="Cell"/> по горизонтали</returns>
    private IEnumerable<IReadOnlyList<Cell>> EnumerateHorizontalRuns()
    {
        for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
            foreach (var run in EnumerateRuns(r, 0, 0, 1))
                yield return run;
    }

    /// <summary>
    /// Проверяет что одинаковые фигуры находяться рядом по вертикали
    /// </summary>
    /// <returns>Возвращает последовательно список <see cref="Cell"/> по вертикали</returns>
    private IEnumerable<IReadOnlyList<Cell>> EnumerateVerticalRuns()
    {
        for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
            foreach (var run in EnumerateRuns(0, c, 1, 0))
                yield return run;
    }

    private IEnumerable<IReadOnlyList<Cell>> EnumerateRuns(int startR, int startC, int stepR, int stepC)
    {
        int r = startR;
        int c = startC;

        while (r < GameConstants.BOARD_ROWS && c < GameConstants.BOARD_COLUMNS)
        {
            var start = cells[r, c];
            var shape = start.Shape;

            if (shape == ShapeType.None)
            {
                r += stepR;
                c += stepC;
                continue;
            }

            int len = 1;

            while (true)
            {
                int nr = r + stepR * len;
                int nc = c + stepC * len;

                if (nr >= GameConstants.BOARD_ROWS || nc >= GameConstants.BOARD_COLUMNS)
                    break;

                if (cells[nr, nc].Shape != shape)
                    break;

                len++;
            }

            var run = new List<Cell>(len);

            for (int i = 0; i < len; i++)
                run.Add(cells[r + stepR * i, c + stepC * i]);

            yield return run;

            r += stepR * len;
            c += stepC * len;
        }
    }

}
