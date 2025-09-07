using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatMob.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Verificar e adicionar campos em falta na tabela AuditLogs
            migrationBuilder.Sql(@"
                SET @sql = (SELECT IF(
                    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                     WHERE TABLE_SCHEMA = 'MatMob_db' 
                     AND TABLE_NAME = 'AuditLogs' 
                     AND COLUMN_NAME = 'ContentHash') = 0,
                    'ALTER TABLE AuditLogs ADD COLUMN ContentHash varchar(64) CHARACTER SET utf8mb4 NULL',
                    'SELECT ''Column ContentHash already exists'''
                ));
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @sql = (SELECT IF(
                    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                     WHERE TABLE_SCHEMA = 'MatMob_db' 
                     AND TABLE_NAME = 'AuditLogs' 
                     AND COLUMN_NAME = 'PreviousHash') = 0,
                    'ALTER TABLE AuditLogs ADD COLUMN PreviousHash varchar(64) CHARACTER SET utf8mb4 NULL',
                    'SELECT ''Column PreviousHash already exists'''
                ));
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @sql = (SELECT IF(
                    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                     WHERE TABLE_SCHEMA = 'MatMob_db' 
                     AND TABLE_NAME = 'AuditLogs' 
                     AND COLUMN_NAME = 'SequenceNumber') = 0,
                    'ALTER TABLE AuditLogs ADD COLUMN SequenceNumber bigint NOT NULL DEFAULT 0',
                    'SELECT ''Column SequenceNumber already exists'''
                ));
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @sql = (SELECT IF(
                    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                     WHERE TABLE_SCHEMA = 'MatMob_db' 
                     AND TABLE_NAME = 'AuditLogs' 
                     AND COLUMN_NAME = 'IntegrityVerified') = 0,
                    'ALTER TABLE AuditLogs ADD COLUMN IntegrityVerified tinyint(1) NOT NULL DEFAULT 1',
                    'SELECT ''Column IntegrityVerified already exists'''
                ));
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Criar tabela AuditModuleConfigs se não existir
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS AuditModuleConfigs (
                    Id int NOT NULL AUTO_INCREMENT,
                    Module varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    Process varchar(100) CHARACTER SET utf8mb4 NULL,
                    Enabled tinyint(1) NOT NULL DEFAULT 1,
                    CreatedAt datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                    UpdatedAt datetime(6) NULL,
                    CONSTRAINT PK_AuditModuleConfigs PRIMARY KEY (Id)
                ) CHARACTER SET=utf8mb4;
            ");

            // Adicionar índices de forma condicional
            migrationBuilder.Sql(@"
                SET @sql = (SELECT IF(
                    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                     WHERE TABLE_SCHEMA = 'MatMob_db' 
                     AND TABLE_NAME = 'AuditLogs' 
                     AND INDEX_NAME = 'IX_AuditLogs_Action') = 0,
                    'CREATE INDEX IX_AuditLogs_Action ON AuditLogs (Action)',
                    'SELECT ''Index IX_AuditLogs_Action already exists'''
                ));
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @sql = (SELECT IF(
                    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                     WHERE TABLE_SCHEMA = 'MatMob_db' 
                     AND TABLE_NAME = 'AuditLogs' 
                     AND INDEX_NAME = 'IX_AuditLogs_CreatedAt') = 0,
                    'CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs (CreatedAt)',
                    'SELECT ''Index IX_AuditLogs_CreatedAt already exists'''
                ));
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @sql = (SELECT IF(
                    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                     WHERE TABLE_SCHEMA = 'MatMob_db' 
                     AND TABLE_NAME = 'AuditLogs' 
                     AND INDEX_NAME = 'IX_AuditLogs_UserId') = 0,
                    'CREATE INDEX IX_AuditLogs_UserId ON AuditLogs (UserId)',
                    'SELECT ''Index IX_AuditLogs_UserId already exists'''
                ));
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @sql = (SELECT IF(
                    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS 
                     WHERE TABLE_SCHEMA = 'MatMob_db' 
                     AND TABLE_NAME = 'AuditModuleConfigs' 
                     AND INDEX_NAME = 'IX_AuditModuleConfigs_Module_Process') = 0,
                    'CREATE UNIQUE INDEX IX_AuditModuleConfigs_Module_Process ON AuditModuleConfigs (Module, Process)',
                    'SELECT ''Index IX_AuditModuleConfigs_Module_Process already exists'''
                ));
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "AuditModuleConfigs");
        }
    }
}
