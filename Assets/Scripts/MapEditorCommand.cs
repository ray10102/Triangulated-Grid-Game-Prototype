/*
using CellCorner = TriCell.CellCorner;

public interface ICommand<T>
{
    void Apply(T target);
    void Undo(T target);
}

public class MapEditorTriCellCommand : ICommand<TriCell>
{
    CellCorner[] corners;
    public MapEditorTriCellCommand(CellCorner[] corners) {
        this.corners = corners.Clone();
    }

    public void Apply(TriCell cell) {
        cell.corners.
    }

    public void Undo(TriCell cell) {
        throw new System.NotImplementedException();
    }
}

public class TriCellEditorCommandFactory
{

}
*/