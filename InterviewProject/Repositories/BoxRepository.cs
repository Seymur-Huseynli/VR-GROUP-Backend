using InterviewProject.Models;
using System.Data;
using System.Data.SqlClient;

namespace InterviewProject.Repositories;

public class BoxRepository
{
    private readonly string _connectionString;

    public BoxRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task BulkInsert(IReadOnlyList<Box> boxes)
    {
        (DataTable? BoxesTable, DataTable? ContentsTable) = CreateDataTablesFromBoxes(boxes);

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var transaction = await connection.BeginTransactionAsync())
            {
                using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, (SqlTransaction)transaction))
                {
                    bulkCopy.DestinationTableName = "Boxes";
                    await bulkCopy.WriteToServerAsync(BoxesTable);

                    bulkCopy.DestinationTableName = "Contents";
                    await bulkCopy.WriteToServerAsync(ContentsTable);
                }
                transaction.Commit();
            }
        }
    }

    public static (DataTable? BoxesTable, DataTable? ContentsTable) CreateDataTablesFromBoxes(IReadOnlyList<Box> boxes)
    {
        if (boxes == null || boxes.Count == 0) return (null, null);

        DataTable boxesTable = new DataTable("Boxes");
        boxesTable.Columns.Add("SupplierIdentifier", typeof(string));
        boxesTable.Columns.Add("Identifier", typeof(string));

        DataTable contentsTable = new DataTable("Contents");
        // Content should have foreign key to Box (Identifier)
        contentsTable.Columns.Add("Identifier", typeof(string));
        contentsTable.Columns.Add("PoNumber", typeof(string));
        contentsTable.Columns.Add("Isbn", typeof(string));
        contentsTable.Columns.Add("Quantity", typeof(int));

        foreach (var box in boxes)
        {
            DataRow boxRow = boxesTable.NewRow();
            boxRow["SupplierIdentifier"] = box.SupplierIdentifier;
            boxRow["Identifier"] = box.Identifier;
            boxesTable.Rows.Add(boxRow);

            foreach (var content in box.Contents)
            {
                DataRow contentRow = contentsTable.NewRow();
                // Content should have foreign key to Box (Identifier)
                contentRow["Identifier"] = box.Identifier;
                contentRow["PoNumber"] = content.PoNumber;
                contentRow["Isbn"] = content.Isbn;
                contentRow["Quantity"] = content.Quantity;
                contentsTable.Rows.Add(contentRow);
            }
        }

        return (boxesTable, contentsTable);
    }
}