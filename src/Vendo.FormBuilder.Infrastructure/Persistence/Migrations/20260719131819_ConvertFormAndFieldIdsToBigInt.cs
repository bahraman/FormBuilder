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

                -- Drop FKs that reference Forms/FormFields ids.
                DECLARE @sql nvarchar(max) = N'';
                SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id))
                    + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
                    + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';'
                FROM sys.foreign_keys fk
                WHERE OBJECT_NAME(fk.referenced_object_id) IN (N'Forms', N'FormFields')
                   OR OBJECT_NAME(fk.parent_object_id) IN (N'Forms', N'FormFields', N'FormResponses', N'FormResponseValues', N'FieldOptions', N'FieldValidationRules');
                EXEC sp_executesql @sql;

                -- Forms.Id / ParentFormId
                ALTER TABLE [dbo].[Forms] DROP CONSTRAINT [PK_Forms];
                ALTER TABLE [dbo].[Forms] DROP COLUMN [ParentFormId];
                ALTER TABLE [dbo].[Forms] DROP COLUMN [Id];
                ALTER TABLE [dbo].[Forms] ADD [Id] bigint IDENTITY(1,1) NOT NULL;
                ALTER TABLE [dbo].[Forms] ADD [ParentFormId] bigint NULL;
                ALTER TABLE [dbo].[Forms] ADD CONSTRAINT [PK_Forms] PRIMARY KEY ([Id]);
                ALTER TABLE [dbo].[Forms] ADD CONSTRAINT [FK_Forms_Forms_ParentFormId]
                    FOREIGN KEY ([ParentFormId]) REFERENCES [dbo].[Forms]([Id]);

                -- FormFields.Id / FormId
                ALTER TABLE [dbo].[FormFields] DROP CONSTRAINT [PK_FormFields];
                ALTER TABLE [dbo].[FormFields] DROP COLUMN [FormId];
                ALTER TABLE [dbo].[FormFields] DROP COLUMN [Id];
                ALTER TABLE [dbo].[FormFields] ADD [Id] bigint IDENTITY(1,1) NOT NULL;
                ALTER TABLE [dbo].[FormFields] ADD [FormId] bigint NOT NULL;
                ALTER TABLE [dbo].[FormFields] ADD CONSTRAINT [PK_FormFields] PRIMARY KEY ([Id]);
                ALTER TABLE [dbo].[FormFields] ADD CONSTRAINT [FK_FormFields_Forms_FormId]
                    FOREIGN KEY ([FormId]) REFERENCES [dbo].[Forms]([Id]) ON DELETE CASCADE;

                -- FieldOptions.FormFieldId
                ALTER TABLE [dbo].[FieldOptions] DROP COLUMN [FormFieldId];
                ALTER TABLE [dbo].[FieldOptions] ADD [FormFieldId] bigint NOT NULL;
                ALTER TABLE [dbo].[FieldOptions] ADD CONSTRAINT [FK_FieldOptions_FormFields_FormFieldId]
                    FOREIGN KEY ([FormFieldId]) REFERENCES [dbo].[FormFields]([Id]) ON DELETE CASCADE;

                -- FieldValidationRules.FormFieldId
                ALTER TABLE [dbo].[FieldValidationRules] DROP COLUMN [FormFieldId];
                ALTER TABLE [dbo].[FieldValidationRules] ADD [FormFieldId] bigint NOT NULL;
                ALTER TABLE [dbo].[FieldValidationRules] ADD CONSTRAINT [FK_FieldValidationRules_FormFields_FormFieldId]
                    FOREIGN KEY ([FormFieldId]) REFERENCES [dbo].[FormFields]([Id]) ON DELETE CASCADE;

                -- FormResponses.FormId
                ALTER TABLE [dbo].[FormResponses] DROP COLUMN [FormId];
                ALTER TABLE [dbo].[FormResponses] ADD [FormId] bigint NOT NULL;
                ALTER TABLE [dbo].[FormResponses] ADD CONSTRAINT [FK_FormResponses_Forms_FormId]
                    FOREIGN KEY ([FormId]) REFERENCES [dbo].[Forms]([Id]);

                -- FormResponseValues.FormFieldId
                ALTER TABLE [dbo].[FormResponseValues] DROP COLUMN [FormFieldId];
                ALTER TABLE [dbo].[FormResponseValues] ADD [FormFieldId] bigint NOT NULL;
                ALTER TABLE [dbo].[FormResponseValues] ADD CONSTRAINT [FK_FormResponseValues_FormFields_FormFieldId]
                    FOREIGN KEY ([FormFieldId]) REFERENCES [dbo].[FormFields]([Id]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible key-type conversion.
        }
    }
}
