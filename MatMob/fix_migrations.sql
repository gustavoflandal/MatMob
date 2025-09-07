-- Script para marcar migrations de auditoria como aplicadas
-- As tabelas já existem no banco, mas o EF não reconhece as migrations

-- Verificar se o histório de migrations existe
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

-- Marcar as migrations de auditoria como aplicadas
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES 
    ('20250906014217_AddCreatedAtColumnToAuditLogs', '9.0.8'),
    ('20250907041419_AddAuditModuleConfig', '9.0.8');

-- Verificar se a tabela AuditLogs existe e tem a estrutura correta
SELECT COUNT(*) as AuditLogs_Exists FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'MatMob_db' AND TABLE_NAME = 'AuditLogs';

-- Verificar se a tabela AuditModuleConfigs existe
SELECT COUNT(*) as AuditModuleConfigs_Exists FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'MatMob_db' AND TABLE_NAME = 'AuditModuleConfigs';

-- Verificar migrations aplicadas
SELECT * FROM `__EFMigrationsHistory` 
WHERE MigrationId LIKE '%Audit%' 
ORDER BY MigrationId;