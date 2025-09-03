using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MatMob.Data;

namespace MatMob
{
    public static class DiagnosticoLogin
    {
        public static async Task VerificarUsuarioAdmin(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            Console.WriteLine("=== DIAGNÓSTICO DO LOGIN ADMIN ===");

            try
            {
                // Verificar conexão com banco
                var canConnect = await context.Database.CanConnectAsync();
                Console.WriteLine($"✓ Conexão com banco: {(canConnect ? "OK" : "FALHOU")}");

                // Verificar se as tabelas existem
                var users = await context.Users.CountAsync();
                var roles = await context.Roles.CountAsync();
                Console.WriteLine($"✓ Total de usuários: {users}");
                Console.WriteLine($"✓ Total de roles: {roles}");

                // Listar todos os usuários
                var allUsers = await context.Users.ToListAsync();
                Console.WriteLine("\n--- USUÁRIOS NO BANCO ---");
                foreach (var user in allUsers)
                {
                    Console.WriteLine($"ID: {user.Id}");
                    Console.WriteLine($"Email: {user.Email}");
                    Console.WriteLine($"UserName: {user.UserName}");
                    Console.WriteLine($"EmailConfirmed: {user.EmailConfirmed}");
                    Console.WriteLine($"PasswordHash: {user.PasswordHash?[..20]}...");
                    Console.WriteLine($"NormalizedEmail: {user.NormalizedEmail}");
                    Console.WriteLine($"NormalizedUserName: {user.NormalizedUserName}");
                    Console.WriteLine("---");
                }

                // Verificar roles
                var allRoles = await context.Roles.ToListAsync();
                Console.WriteLine("\n--- ROLES NO BANCO ---");
                foreach (var role in allRoles)
                {
                    Console.WriteLine($"Nome: {role.Name} | NormalizedName: {role.NormalizedName}");
                }

                // Verificar UserRoles
                var userRoles = await context.UserRoles.ToListAsync();
                Console.WriteLine($"\n--- USER ROLES ---");
                Console.WriteLine($"Total de UserRoles: {userRoles.Count}");

                // Tentar encontrar o usuário admin especificamente
                var adminEmail = "admin@matmob.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                
                Console.WriteLine($"\n--- TESTE ESPECÍFICO DO ADMIN ---");
                if (adminUser != null)
                {
                    Console.WriteLine($"✓ Usuário admin encontrado: {adminUser.Email}");
                    
                    // Testar senha
                    var passwordTest = await userManager.CheckPasswordAsync(adminUser, "Admin123!");
                    Console.WriteLine($"✓ Teste de senha 'Admin123!': {(passwordTest ? "SUCESSO" : "FALHOU")}");
                    
                    // Verificar roles do admin
                    var adminRoles = await userManager.GetRolesAsync(adminUser);
                    Console.WriteLine($"✓ Roles do admin: {string.Join(", ", adminRoles)}");

                    // Verificar se pode fazer login
                    var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<IdentityUser>>();
                    var loginResult = await signInManager.CheckPasswordSignInAsync(adminUser, "Admin123!", false);
                    Console.WriteLine($"✓ Teste de login: {loginResult.Succeeded}");
                    
                    if (!loginResult.Succeeded)
                    {
                        Console.WriteLine($"  - IsLockedOut: {loginResult.IsLockedOut}");
                        Console.WriteLine($"  - RequiresTwoFactor: {loginResult.RequiresTwoFactor}");
                        Console.WriteLine($"  - IsNotAllowed: {loginResult.IsNotAllowed}");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Usuário admin NÃO encontrado!");
                    
                    // Tentar criar o usuário admin novamente
                    Console.WriteLine("Tentando criar usuário admin...");
                    
                    var newAdminUser = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };
                    
                    var createResult = await userManager.CreateAsync(newAdminUser, "Admin123!");
                    Console.WriteLine($"Criação do usuário: {(createResult.Succeeded ? "SUCESSO" : "FALHOU")}");
                    
                    if (!createResult.Succeeded)
                    {
                        foreach (var error in createResult.Errors)
                        {
                            Console.WriteLine($"  - Erro: {error.Description}");
                        }
                    }
                    else
                    {
                        // Adicionar role de administrador
                        var addRoleResult = await userManager.AddToRoleAsync(newAdminUser, "Administrador");
                        Console.WriteLine($"Adição da role: {(addRoleResult.Succeeded ? "SUCESSO" : "FALHOU")}");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("=== FIM DO DIAGNÓSTICO ===");
        }
    }
}
