namespace Lattice.Drawing;

public interface IDirtyable
{
    bool IsDirtyable { get; set; }

    bool IsDirty { get; }

    void Invalidate();

    void ClearDirty();
}