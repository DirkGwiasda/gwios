using System.Data;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using Npgsql;
using NpgsqlTypes;

namespace GwiOS.Core.Haushaltsbuch.Infrastructure;

/// <summary>
/// Stores Haushaltsbuch categories in the table <c>haushaltsbuch.categories</c> of the GwiOS PostgreSQL database,
/// using plain SQL.
/// </summary>
internal sealed class CategoryPostgresRepository(
    NpgsqlDataSource dataSource,
    ILogger<CategoryPostgresRepository> logger) : ICategoryRepository
{
    private const string CreateTablesSql = """
        CREATE SCHEMA IF NOT EXISTS haushaltsbuch;

        CREATE TABLE IF NOT EXISTS haushaltsbuch.categories
        (
            id        uuid     NOT NULL,
            name      text     NOT NULL,
            type      smallint NOT NULL,
            parent_id uuid     NULL,
            color     text     NULL,
            CONSTRAINT pk_haushaltsbuch_categories PRIMARY KEY (id),
            CONSTRAINT fk_haushaltsbuch_categories_parent FOREIGN KEY (parent_id)
                REFERENCES haushaltsbuch.categories (id)
        );
        """;

    private const string InsertSql = """
        INSERT INTO haushaltsbuch.categories (id, name, type, parent_id, color)
        VALUES (@id, @name, @type, @parent_id, @color)
        """;

    private const string UpdateSql = """
        UPDATE haushaltsbuch.categories
        SET name = @name, type = @type, parent_id = @parent_id, color = @color
        WHERE id = @id
        """;

    private const string SelectAllSql = """
        SELECT id, name, type, parent_id, color
        FROM haushaltsbuch.categories
        ORDER BY type, name
        """;

    private const string DeleteSql = """
        DELETE FROM haushaltsbuch.categories
        WHERE id = @id
        """;

    private readonly NpgsqlDataSource _dataSource = dataSource;
    private readonly ILogger<CategoryPostgresRepository> _logger = logger;

    public async Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(CreateTablesSql);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertAsync(Category category, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(InsertSql);
        AddCategoryParameters(command, category);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(UpdateSql);
        AddCategoryParameters(command, category);
        int updatedRowCount = await command.ExecuteNonQueryAsync(cancellationToken);
        if (updatedRowCount == 0)
        {
            _logger.LogWarning("Category to update is not stored.", CreateCategoryIdData(category.Id));
            throw new CategoryNotFoundException(category.Id);
        }
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(SelectAllSql);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        List<Category> categories = [];
        while (await reader.ReadAsync(cancellationToken))
        {
            categories.Add(ReadCategory(reader));
        }

        return categories;
    }

    public async Task DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(DeleteSql);
        command.Parameters.AddWithValue("id", categoryId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddCategoryParameters(NpgsqlCommand command, Category category)
    {
        command.Parameters.AddWithValue("id", category.Id);
        command.Parameters.AddWithValue("name", category.Name);
        command.Parameters.AddWithValue("type", (short)category.Type);
        command.Parameters.AddWithValue("parent_id", NpgsqlDbType.Uuid, (object?)category.ParentId ?? DBNull.Value);
        command.Parameters.AddWithValue("color", NpgsqlDbType.Text, (object?)category.Color ?? DBNull.Value);
    }

    private static Category ReadCategory(NpgsqlDataReader reader)
        => new()
        {
            Id = reader.GetGuid("id"),
            Name = reader.GetString("name"),
            Type = (CategoryType)reader.GetFieldValue<short>("type"),
            ParentId = reader.IsDBNull("parent_id") ? null : reader.GetGuid("parent_id"),
            Color = reader.IsDBNull("color") ? null : reader.GetString("color")
        };

    private static Dictionary<string, string> CreateCategoryIdData(Guid categoryId)
        => new() { ["CategoryId"] = categoryId.ToString() };
}
