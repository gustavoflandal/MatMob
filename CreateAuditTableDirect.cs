using MySqlConnector;

var connectionString = "Server=localhost;Port=3306;Database=MatMob_db;Uid=root;Pwd=root;CharSet=utf8mb4;";

try
{
    using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();

    // Verificar se a tabela já existe
    using var checkCommand = new MySqlCommand(
        "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'MatMob_db' AND table_name = 'AuditLogs'",
        connection);

    var tableExists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

    if (tableExists == 0)
    {
        // Criar a tabela AuditLogs
        var createTableSql = @"
            CREATE TABLE AuditLogs (
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
                CreatedAt timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
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
        ";

        using var createCommand = new MySqlCommand(createTableSql, connection);
        await createCommand.ExecuteNonQueryAsync();

        // Criar os índices
        var indexCommands = new[]
        {
            "CREATE INDEX IX_AuditLogs_Action ON AuditLogs (Action);",
            "CREATE INDEX IX_AuditLogs_CorrelationId ON AuditLogs (CorrelationId);",
            "CREATE INDEX IX_AuditLogs_EntityName ON AuditLogs (EntityName);",
            "CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs (CreatedAt);",
            "CREATE INDEX IX_AuditLogs_UserId ON AuditLogs (UserId);"
        };

        foreach (var indexSql in indexCommands)
        {
            using var indexCommand = new MySqlCommand(indexSql, connection);
            await indexCommand.ExecuteNonQueryAsync();
        }

        Console.WriteLine("Tabela AuditLogs criada com sucesso!");
    }
    else
    {
        Console.WriteLine("Tabela AuditLogs já existe.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Erro: {ex.Message}");
}