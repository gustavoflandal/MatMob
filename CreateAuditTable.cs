using Microsoft.EntityFrameworkCore;
using MatMob.Data;

var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseMySql("Server=localhost;Port=3306;Database=MatMob_db;Uid=root;Pwd=root;CharSet=utf8mb4;", ServerVersion.AutoDetect("Server=localhost;Port=3306;Database=MatMob_db;Uid=root;Pwd=root;CharSet=utf8mb4;"))
    .Options);

try
{
    // Criar a tabela AuditLogs diretamente
    await context.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS AuditLogs (
            Id int NOT NULL AUTO_INCREMENT,
            UserId varchar(255) CHARACTER SET utf8mb4 NULL,
            UserName varchar(255) CHARACTER SET utf8mb4 NULL,
            IpAddress varchar(45) CHARACTER SET utf8mb4 NULL,
            UserAgent varchar(500) CHARACTER SET utf8mb4 NULL,
            Action varchar(50) CHARACTER SET utf8mb4 NOT NULL,
            EntityName varchar(100) CHARACTER SET utf8mb4 NULL,
            EntityId int NULL,
            PropertyName varchar(100) CHARACTER SET utf8mb4 NULL,
            OldValue TEXT CHARACTER SET utf8mb4 NULL,
            NewValue TEXT CHARACTER SET utf8mb4 NULL,
            OldData TEXT CHARACTER SET utf8mb4 NULL,
            NewData TEXT CHARACTER SET utf8mb4 NULL,
            Description varchar(1000) CHARACTER SET utf8mb4 NULL,
            Context varchar(200) CHARACTER SET utf8mb4 NULL,
            Severity varchar(20) CHARACTER SET utf8mb4 NOT NULL,
            Category varchar(50) CHARACTER SET utf8mb4 NULL,
            AdditionalData TEXT CHARACTER SET utf8mb4 NULL,
            Timestamp timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
            Duration bigint NULL,
            Success tinyint(1) NOT NULL,
            ErrorMessage varchar(2000) CHARACTER SET utf8mb4 NULL,
            StackTrace TEXT CHARACTER SET utf8mb4 NULL,
            SessionId varchar(255) CHARACTER SET utf8mb4 NULL,
            CorrelationId varchar(255) CHARACTER SET utf8mb4 NULL,
            HttpMethod varchar(10) CHARACTER SET utf8mb4 NULL,
            RequestUrl varchar(500) CHARACTER SET utf8mb4 NULL,
            HttpStatusCode int NULL,
            PermanentRetention tinyint(1) NOT NULL,
            ExpirationDate datetime(6) NULL,
            CONSTRAINT PK_AuditLogs PRIMARY KEY (Id)
        ) CHARACTER SET=utf8mb4;
    ");

    // Criar os índices
    await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_AuditLogs_Action ON AuditLogs (Action);");
    await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_AuditLogs_CorrelationId ON AuditLogs (CorrelationId);");
    await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_AuditLogs_EntityName ON AuditLogs (EntityName);");
    await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_AuditLogs_Timestamp ON AuditLogs (Timestamp);");
    await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_AuditLogs_UserId ON AuditLogs (UserId);");

    Console.WriteLine("Tabela AuditLogs criada com sucesso!");
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao criar tabela: {ex.Message}");
}