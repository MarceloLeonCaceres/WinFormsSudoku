using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;

public static class ValueGuard
{
    public static void EnsureValid(int value, string paramName)
    {
        if (value is < SudokuConstants.MinValue or > SudokuConstants.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                $"Value must be between {SudokuConstants.MinValue} and {SudokuConstants.MaxValue}.");
        }
    }
}
