using Match_M.Model;

namespace Match_M.Services;

/// <summary>
/// Сервис, отвечающий за обработку бонусов (Line‑бонусы и т.п.).
/// Инкапсулирует всю логику активации и создания бонусов.
/// </summary>
public class BonusService(Cell[,] cells)
{
    //IMPORTANT: Эти константы не должны быть в GameConstants чтобы можно было полностью исключить Бонусы
    /// <summary>
    /// Количество совпадений для создания бонуса Line.
    /// </summary>
    private const int LINE_BONUS_LENGTH = 4;
    /// <summary>
    /// Минимальное Количество совпадений для создания бонуса Bomb.
    /// </summary>
    private const int BOMB_BONUS_LENGTH = 5;

    /// <summary>
    /// Формирует итоговый набор ячеек для очистки на основе найденных совпадений,
    /// с учётом активации уже существующих бонусов и создания новых бонусов.
    /// </summary>
    /// <param name="matches">Набор ячеек, входящих в текущие совпадения.</param>
    /// <param name="lastMovedCell">Последняя сдвинутая ячейка (для создания бонуса).</param>
    /// <param name="horizontalRuns">Горизонтальные последовательности одинаковых фигур.</param>
    /// <param name="verticalRuns">Вертикальные последовательности одинаковых фигур.</param>
    /// <returns>Набор ячеек, которые должны быть очищены на данном шаге.</returns>
    public HashSet<Cell> PrepareCellsToClear(
        HashSet<Cell> matches,
        Cell? lastMovedCell,
        IEnumerable<IReadOnlyList<Cell>> horizontalRuns,
        IEnumerable<IReadOnlyList<Cell>> verticalRuns)
    {
        var cellsToClear = new HashSet<Cell>(matches);

        ActivateLineBonuses(matches, cellsToClear);

        if (lastMovedCell is not null)
        {
            CreateLineBonuses(matches, cellsToClear, lastMovedCell, horizontalRuns, verticalRuns);
        }

        return cellsToClear;
    }

    /// <summary>
    /// Активирует уже существующие Line‑бонусы (HLine / VLine), попавшие в матч:
    /// добавляет к очистке всю строку или столбец, включая саму ячейку‑бонус.
    /// </summary>
    private void ActivateLineBonuses(HashSet<Cell> matches, HashSet<Cell> cellsToClear)
    {
        foreach (var cell in matches)
        {
            switch (cell.Bonus)
            {
                case BonusType.HLine:
                    for (int c = 0; c < GameConstants.BOARD_COLUMNS; c++)
                        cellsToClear.Add(cells[cell.Row, c]);
                    break;

                case BonusType.VLine:
                    for (int r = 0; r < GameConstants.BOARD_ROWS; r++)
                        cellsToClear.Add(cells[r, cell.Column]);
                    break;
            }
        }
    }

    /// <summary>
    /// Ищет комбинации ровно из четырёх одинаковых фигур, которые включают
    /// последний сдвинутый элемент, и превращает этот элемент в бонус Line.
    /// Сам бонус из очистки исключается (остается на поле).
    /// </summary>
    private void CreateLineBonuses(
        HashSet<Cell> matches,
        HashSet<Cell> cellsToClear,
        Cell lastMovedCell,
        IEnumerable<IReadOnlyList<Cell>> horizontalRuns,
        IEnumerable<IReadOnlyList<Cell>> verticalRuns)
    {
        var last = lastMovedCell;

        // По горизонтали
        foreach (var run in horizontalRuns)
        {
            if (run.Count != LINE_BONUS_LENGTH)
                continue;

            if (run.Contains(last) && matches.IsSupersetOf(run))
            {
                // Бонус создаётся в ячейке, которую двигали последней
                last.Bonus = BonusType.HLine;

                // Не удаляем бонус с поля
                cellsToClear.Remove(last);
                return;
            }
        }

        // По вертикали
        foreach (var run in verticalRuns)
        {
            if (run.Count != LINE_BONUS_LENGTH)
                continue;

            if (run.Contains(last) && matches.IsSupersetOf(run))
            {
                last.Bonus = BonusType.VLine;

                cellsToClear.Remove(last);
                return;
            }
        }
    }
}

