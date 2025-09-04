using Microsoft.EntityFrameworkCore;
using MatMob.Data;

var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseMySql("Server=localhost;Port=3306;Database=MatMob_db;Uid=root;Pwd=root;CharSet=utf8mb4;", ServerVersion.AutoDetect("Server=localhost;Port=3306;Database=MatMob_db;Uid=root;Pwd=root;CharSet=utf8mb4;"))
    .Options);

try
{
    // Remover a entrada problemática do histórico de migrações
    await context.Database.ExecuteSqlRawAsync(
        "DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20250904004729_AddAuditLogTable'");

    Console.WriteLine("Entrada problemática removida do histórico de migrações!");
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao remover entrada do histórico: {ex.Message}");
}