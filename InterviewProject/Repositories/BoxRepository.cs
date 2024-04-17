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

    public async Task BulkInsertAsync(IReadOnlyList<Box> boxes)
    {
        try
        {
            (var boxesTable, var contentsTable) = CreateDataTablesFromBoxes(boxes);
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            await BulkInsertBoxAsync(connection, (SqlTransaction)transaction, boxesTable);
            await BulkInsertContent(connection, (SqlTransaction)transaction, contentsTable);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static async Task BulkInsertBoxAsync(SqlConnection connection, SqlTransaction transaction, DataTable boxes)
    {
        using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);

        bulkCopy.DestinationTableName = "Box";
        bulkCopy.ColumnMappings.Add("SupplierIdentifier", "SupplierIdentifier");
        bulkCopy.ColumnMappings.Add("Identifier", "Identifier");

        await bulkCopy.WriteToServerAsync(boxes);
    }

    private static async Task BulkInsertContent(SqlConnection connection, SqlTransaction transaction, DataTable contentsTable)
    {
        using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);

        bulkCopy.DestinationTableName = "Content";
        bulkCopy.ColumnMappings.Add("Identifier", "Identifier");
        bulkCopy.ColumnMappings.Add("PoNumber", "PoNumber");
        bulkCopy.ColumnMappings.Add("Isbn", "Isbn");
        bulkCopy.ColumnMappings.Add("Quantity", "Quantity");

        await bulkCopy.WriteToServerAsync(contentsTable);
    }

    private static (DataTable BoxesTable, DataTable ContentsTable) CreateDataTablesFromBoxes(IReadOnlyList<Box> boxes)
    {
        if (boxes == null || boxes.Count == 0) throw new Exception("Box can't be null");

        var boxesTable = new DataTable("Box");
        boxesTable.Columns.Add("SupplierIdentifier", typeof(string));
        boxesTable.Columns.Add("Identifier", typeof(string));

        var contentsTable = new DataTable("Content");
        // Content should have foreign key to Box (Identifier)
        contentsTable.Columns.Add("Identifier", typeof(string));
        contentsTable.Columns.Add("PoNumber", typeof(string));
        contentsTable.Columns.Add("Isbn", typeof(string));
        contentsTable.Columns.Add("Quantity", typeof(int));

        foreach (var box in boxes)
        {
            var boxRow = boxesTable.NewRow();
            boxRow["SupplierIdentifier"] = box.SupplierIdentifier;
            boxRow["Identifier"] = box.Identifier;
            boxesTable.Rows.Add(boxRow);

            foreach (var content in box.Contents)
            {
                var contentRow = contentsTable.NewRow();
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