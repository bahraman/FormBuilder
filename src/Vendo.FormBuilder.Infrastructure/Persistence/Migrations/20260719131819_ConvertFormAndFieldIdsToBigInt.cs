using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendo.FormBuilder.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Converts Form/FormField primary keys and related FKs from uniqueidentifier to bigint IDENTITY.
    /// Existing Guid values cannot be preserved; form graph tables are cleared before rebuild.
    /// </remarks>
    public partial class ConvertFormAndFieldIdsToBigInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                -- Clear dependent data (Guid keys cannot convert to bigint).
                DELETE FROM [dbo].[FormResponseValues];
                DELETE FROM [dbo].[FormResponses];
                DELETE FROM [dbo].[FieldOptions];
                DELETE FROM [dbo].[FieldValidationRules];
                DELETE FROM [dbo].[FormFields];
                DELETE FROM [dbo].[Forms];

                -- Drop all foreign keys on/to the affected tables.
                DECLARE @fkSql nvarchar(max) = N'';
                SELECT @fkSql = @fkSql + N'ALTER TABLE '
                    + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.'
                    + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
                    + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';'
                FROM sys.foreign_keys fk
                WHERE OBJECT_NAME(fk.referenced_object_id) IN (N'Forms', N'FormFields')
                   OR OBJECT_NAME(fk.parent_object_id) IN (
                        N'Forms', N'FormFields', N'FormResponses',
                        N'FormResponseValues', N'FieldOptions', N'FieldValidationRules');
                IF LEN(@fkSql) > 0 EXEC sp_executesql @fkSql;

                -- Drop non-PK indexes that depend on columns we are about to replace.
                DECLARE @ixSql nvarchar(max) = N'';
                SELECT @ixSql = @ixSql + N'DROP INDEX '
                    + QUOTENAME(i.name) + N' ON '
                    + QUOTENAME(OBJECT_SCHEMA_NAME(i.object_id)) + N'.'
                    + QUOTENAME(OBJECT_NAME(i.object_id)) + N';'
                FROM sys.indexes i
                INNER JOIN sys.index_columns ic
                    ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                INNER JOIN sys.columns c
                    ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                WHERE i.is_primary_key = 0
                  AND i.is_unique_constraint = 0
                  AND i.name IS NOT NULL
                  AND (
                        (OBJECT_NAME(i.object_id) = N'Forms'
                            AND c.name IN (N'Id', N'ParentFormId'))
                     OR (OBJECT_NAME(i.object_id) = N'FormFields'
                            AND c.name IN (N'Id', N'FormId'))
                     OR (OBJECT_NAME(i.object_id) = N'FieldOptions'
                            AND c.name = N'FormFieldId')
                     OR (OBJECT_NAME(i.object_id) = N'FieldValidationRules'
                            AND c.name = N'FormFieldId')
                     OR (OBJECT_NAME(i.object_id) = N'FormResponses'
                            AND c.name = N'FormId')
                     OR (OBJECT_NAME(i.object_id) = N'FormResponseValues'
                            AND c.name = N'FormFieldId')
                  );
                IF LEN(@ixSql) > 0 EXEC sp_executesql @ixSql;

                -- Forms.Id / ParentFormId
                ALTER TABLE [dbo].[Forms] DROP CONSTRAINT [PK_Forms];
                ALTER TABLE [dbo].[Forms] DROP COLUMN [ParentFormId];
                ALTER TABLE [dbo].[Forms] DROP COLUMN [Id];
                ALTER TABLE [dbo].[Forms] ADD [Id] bigint IDENTITY(1,1) NOT NULL;
                ALTER TABLE [dbo].[Forms] ADD [ParentFormId] bigint NULL;
                ALTER TABLE [dbo].[Forms] ADD CONSTRAINT [PK_Forms] PRIMARY KEY ([Id]);
                ALTER TABLE [dbo].[Forms] ADD CONSTRAINT [FK_Forms_Forms_ParentFormId]
                    FOREIGN KEY ([ParentFormId]) REFERENCES [dbo].[Forms]([Id]);
                CREATE INDEX [IX_Forms_ParentFormId] ON [dbo].[Forms] ([ParentFormId]);

                -- FormFields.Id / FormId
                ALTER TABLE [dbo].[FormFields] DROP CONSTRAINT [PK_FormFields];
                ALTER TABLE [dbo].[FormFields] DROP COLUMN [FormId];
                ALTER TABLE [dbo].[FormFields] DROP COLUMN [Id];
                ALTER TABLE [dbo].[FormFields] ADD [Id] bigint IDENTITY(1,1) NOT NULL;
                ALTER TABLE [dbo].[FormFields] ADD [FormId] bigint NOT NULL;
                ALTER TABLE [dbo].[FormFields] ADD CONSTRAINT [PK_FormFields] PRIMARY KEY ([Id]);
                ALTER TABLE [dbo].[FormFields] ADD CONSTRAINT [FK_FormFields_Forms_FormId]
                    FOREIGN KEY ([FormId]) REFERENCES [dbo].[Forms]([Id]) ON DELETE CASCADE;
                CREATE INDEX [IX_FormFields_FormId_DisplayOrder]
                    ON [dbo].[FormFields] ([FormId], [DisplayOrder]);
                CREATE UNIQUE INDEX [IX_FormFields_FormId_Name]
                    ON [dbo].[FormFields] ([FormId], [Name])
                    WHERE [IsDeleted] = 0;

                -- FieldOptions.FormFieldId
                ALTER TABLE [dbo].[FieldOptions] DROP COLUMN [FormFieldId];
                ALTER TABLE [dbo].[FieldOptions] ADD [FormFieldId] bigint NOT NULL;
                ALTER TABLE [dbo].[FieldOptions] ADD CONSTRAINT [FK_FieldOptions_FormFields_FormFieldId]
                    FOREIGN KEY ([FormFieldId]) REFERENCES [dbo].[FormFields]([Id]) ON DELETE CASCADE;
                CREATE UNIQUE INDEX [IX_FieldOptions_FormFieldId_Value]
                    ON [dbo].[FieldOptions] ([FormFieldId], [Value])
                    WHERE [IsDeleted] = 0;

                -- FieldValidationRules.FormFieldId
                ALTER TABLE [dbo].[FieldValidationRules] DROP COLUMN [FormFieldId];
                ALTER TABLE [dbo].[FieldValidationRules] ADD [FormFieldId] bigint NOT NULL;
                ALTER TABLE [dbo].[FieldValidationRules] ADD CONSTRAINT [FK_FieldValidationRules_FormFields_FormFieldId]
                    FOREIGN KEY ([FormFieldId]) REFERENCES [dbo].[FormFields]([Id]) ON DELETE CASCADE;
                CREATE UNIQUE INDEX [IX_FieldValidationRules_FormFieldId_RuleType]
                    ON [dbo].[FieldValidationRules] ([FormFieldId], [RuleType])
                    WHERE [IsDeleted] = 0;

                -- FormResponses.FormId
                ALTER TABLE [dbo].[FormResponses] DROP COLUMN [FormId];
                ALTER TABLE [dbo].[FormResponses] ADD [FormId] bigint NOT NULL;
                ALTER TABLE [dbo].[FormResponses] ADD CONSTRAINT [FK_FormResponses_Forms_FormId]
                    FOREIGN KEY ([FormId]) REFERENCES [dbo].[Forms]([Id]);
                CREATE INDEX [IX_FormResponses_FormId] ON [dbo].[FormResponses] ([FormId]);

                -- FormResponseValues.FormFieldId
                ALTER TABLE [dbo].[FormResponseValues] DROP COLUMN [FormFieldId];
                ALTER TABLE [dbo].[FormResponseValues] ADD [FormFieldId] bigint NOT NULL;
                ALTER TABLE [dbo].[FormResponseValues] ADD CONSTRAINT [FK_FormResponseValues_FormFields_FormFieldId]
                    FOREIGN KEY ([FormFieldId]) REFERENCES [dbo].[FormFields]([Id]);
                CREATE INDEX [IX_FormResponseValues_FormFieldId]
                    ON [dbo].[FormResponseValues] ([FormFieldId]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible key-type conversion.
        }
    }
}
