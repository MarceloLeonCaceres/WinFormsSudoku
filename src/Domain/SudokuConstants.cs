using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;

/// <summary>Dimensiones y rango de valores del Sudoku clásico. Única fuente de estas constantes.</summary>
public static class SudokuConstants
{
    public const int Size = 9;
    public const int BoxSize = 3;
    public const int CellCount = Size * Size;
    public const int MinValue = 1;
    public const int MaxValue = Size;
}
