using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace pxdArchiverCE.Controls
{
    // https://stackoverflow.com/a/55485136
    public class PxdDataGrid : DataGrid
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_isDraggingSelection")]
        private static extern ref bool Get_isDraggingSelection(DataGrid dataGrid);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "EndDragging")]
        private static extern void CallEndDragging(DataGrid dataGrid);


        // DataGrid.OnMouseMove() handles the click-drag cell selection
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!Get_isDraggingSelection(this))
            {
                CallEndDragging(this);
            }
        }
    }
}
