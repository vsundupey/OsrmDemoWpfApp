using System;
using System.Windows.Controls;

namespace OsrmRoutesEditor.Helpers
{
    public static class DataGridExtensions
    {
        public static void SelectRowByIndex(this DataGrid dataGrid, int rowIndex)
        {
            if (!dataGrid.SelectionUnit.Equals(DataGridSelectionUnit.FullRow))
                throw new ArgumentException("The SelectionUnit of the DataGrid must be set to FullRow.");

            if (rowIndex < 0 || rowIndex > (dataGrid.Items.Count - 1))
                throw new ArgumentException(string.Format("{0} is an invalid row index.", rowIndex));

            dataGrid.SelectedItems.Clear();
            /* set the SelectedItem property */
            object item = dataGrid.Items[rowIndex]; // = Product X
            dataGrid.SelectedItem = item;

            DataGridRow row;

            if (dataGrid.Items.Count > (rowIndex + 1))
                row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex + 1) as DataGridRow;
            else
            {
                row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            }


            if (row == null)
            {
                /* bring the data item (Product object) into view
                 * in case it has been virtualized away */
                dataGrid.ScrollIntoView(item);
                row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            }

            //TODO: Retrieve and focus a DataGridCell object
        }
    }
}
