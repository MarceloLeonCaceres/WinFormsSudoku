using Domain;
using Sudoku.WinForms.Controls.BoardControl;

namespace Sudoku.WinForms.Tests.Controls.BoardControl;

public sealed class BoardGeometryTests
{
    [Fact]
    public void Create_WhenWidthIsGreaterThanHeight_CentersSquareBoard()
    {
        var geometry = BoardGeometry.Create(new SizeF(1000, 800));

        Assert.Equal(new RectangleF(100, 0, 800, 800), geometry.Bounds);
        Assert.Equal(800f / SudokuConstants.Size, geometry.CellSize);
    }

    [Fact]
    public void Create_WhenHeightIsGreaterThanWidth_CentersSquareBoard()
    {
        var geometry = BoardGeometry.Create(new SizeF(800, 1000));

        Assert.Equal(new RectangleF(0, 100, 800, 800), geometry.Bounds);
    }

    [Fact]
    public void Create_WhenAvailableSizeIsEmpty_ReturnsDefaultGeometry()
    {
        var geometry = BoardGeometry.Create(SizeF.Empty);

        Assert.Equal(RectangleF.Empty, geometry.Bounds);
        Assert.Equal(0f, geometry.CellSize);
    }

    [Fact]
    public void GetCellBounds_ReturnsCorrectCellRectangle()
    {
        var geometry = BoardGeometry.Create(new SizeF(900, 900));

        var bounds = geometry.GetCellBounds(new Position(2, 3));

        var cellSize = 100f;

        Assert.Equal(
            new RectangleF(300, 200, cellSize, cellSize),
            bounds);
    }

    [Fact]
    public void HitTest_WhenPointIsInsideCell_ReturnsItsPosition()
    {
        var geometry = BoardGeometry.Create(new SizeF(900, 900));

        var result = geometry.HitTest(new PointF(350, 250));

        Assert.Equal(new Position(2, 3), result);
    }

    [Fact]
    public void HitTest_WhenPointIsOnTopLeftBoundary_ReturnsFirstCell()
    {
        var geometry = BoardGeometry.Create(new SizeF(900, 900));

        var result = geometry.HitTest(new PointF(0, 0));

        Assert.Equal(new Position(0, 0), result);
    }

    [Fact]
    public void HitTest_WhenPointIsOnRightBoundary_ReturnsNull()
    {
        var geometry = BoardGeometry.Create(new SizeF(900, 900));

        var result = geometry.HitTest(new PointF(900, 450));

        Assert.Null(result);
    }

    [Fact]
    public void HitTest_WhenPointIsOnBottomBoundary_ReturnsNull()
    {
        var geometry = BoardGeometry.Create(new SizeF(900, 900));

        var result = geometry.HitTest(new PointF(450, 900));

        Assert.Null(result);
    }

    [Fact]
    public void HitTest_WhenPointIsOutsideBoard_ReturnsNull()
    {
        var geometry = BoardGeometry.Create(new SizeF(1000, 800));

        var result = geometry.HitTest(new PointF(50, 400));

        Assert.Null(result);
    }

    [Fact]
    public void HitTest_WhenGeometryIsNotInitialized_ReturnsNull()
    {
        var geometry = default(BoardGeometry);

        var result = geometry.HitTest(new PointF(0, 0));

        Assert.Null(result);
    }
}

