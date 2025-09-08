namespace MatMob.Models.Entities
{
    /// <summary>
    /// Enumeração das permissões padrão do sistema
    /// </summary>
    public static class Permissions
    {
        // Gerenciamento de Usuários
        public const string UserView = "user.view";
        public const string UserCreate = "user.create";
        public const string UserEdit = "user.edit";
        public const string UserDelete = "user.delete";
        public const string UserManageRoles = "user.manage_roles";
        public const string UserManagePermissions = "user.manage_permissions";

        // Gerenciamento de Roles
        public const string RoleView = "role.view";
        public const string RoleCreate = "role.create";
        public const string RoleEdit = "role.edit";
        public const string RoleDelete = "role.delete";
        public const string RoleManagePermissions = "role.manage_permissions";

        // Gerenciamento de Permissões
        public const string PermissionView = "permission.view";
        public const string PermissionCreate = "permission.create";
        public const string PermissionEdit = "permission.edit";
        public const string PermissionDelete = "permission.delete";

        // Ativos
        public const string AtivoView = "ativo.view";
        public const string AtivoCreate = "ativo.create";
        public const string AtivoEdit = "ativo.edit";
        public const string AtivoDelete = "ativo.delete";

        // Ordens de Serviço
        public const string OrdemServicoView = "ordem_servico.view";
        public const string OrdemServicoCreate = "ordem_servico.create";
        public const string OrdemServicoEdit = "ordem_servico.edit";
        public const string OrdemServicoDelete = "ordem_servico.delete";
        public const string OrdemServicoApprove = "ordem_servico.approve";
        public const string OrdemServicoCancel = "ordem_servico.cancel";

        // Técnicos
        public const string TecnicoView = "tecnico.view";
        public const string TecnicoCreate = "tecnico.create";
        public const string TecnicoEdit = "tecnico.edit";
        public const string TecnicoDelete = "tecnico.delete";

        // Equipes
        public const string EquipeView = "equipe.view";
        public const string EquipeCreate = "equipe.create";
        public const string EquipeEdit = "equipe.edit";
        public const string EquipeDelete = "equipe.delete";

        // Peças
        public const string PecaView = "peca.view";
        public const string PecaCreate = "peca.create";
        public const string PecaEdit = "peca.edit";
        public const string PecaDelete = "peca.delete";

        // Dashboard
        public const string DashboardView = "dashboard.view";
        public const string DashboardViewAdvanced = "dashboard.view_advanced";

        // Relatórios
        public const string RelatorioView = "relatorio.view";
        public const string RelatorioCreate = "relatorio.create";
        public const string RelatorioExport = "relatorio.export";

        // Auditoria
        public const string AuditoriaView = "auditoria.view";
        public const string AuditoriaConfig = "auditoria.config";
        public const string AuditoriaIntegrity = "auditoria.integrity";

        // Sistema
        public const string SystemConfig = "system.config";
        public const string SystemMaintenance = "system.maintenance";

        /// <summary>
        /// Retorna todas as permissões do sistema organizadas por categoria
        /// </summary>
        public static Dictionary<string, List<(string Code, string Name, string Description)>> GetAllPermissions()
        {
            return new Dictionary<string, List<(string Code, string Name, string Description)>>
            {
                {
                    "Gerenciamento de Usuários",
                    new List<(string Code, string Name, string Description)>
                    {
                        (UserView, "Visualizar Usuários", "Permite visualizar a lista de usuários"),
                        (UserCreate, "Criar Usuários", "Permite criar novos usuários"),
                        (UserEdit, "Editar Usuários", "Permite editar informações de usuários"),
                        (UserDelete, "Excluir Usuários", "Permite excluir usuários"),
                        (UserManageRoles, "Gerenciar Roles de Usuários", "Permite atribuir/remover roles de usuários"),
                        (UserManagePermissions, "Gerenciar Permissões de Usuários", "Permite atribuir/remover permissões diretas de usuários")
                    }
                },
                {
                    "Gerenciamento de Roles",
                    new List<(string Code, string Name, string Description)>
                    {
                        (RoleView, "Visualizar Roles", "Permite visualizar a lista de roles"),
                        (RoleCreate, "Criar Roles", "Permite criar novos roles"),
                        (RoleEdit, "Editar Roles", "Permite editar informações de roles"),
                        (RoleDelete, "Excluir Roles", "Permite excluir roles"),
                        (RoleManagePermissions, "Gerenciar Permissões de Roles", "Permite atribuir/remover permissões de roles")
                    }
                },
                {
                    "Gerenciamento de Permissões",
                    new List<(string Code, string Name, string Description)>
                    {
                        (PermissionView, "Visualizar Permissões", "Permite visualizar a lista de permissões"),
                        (PermissionCreate, "Criar Permissões", "Permite criar novas permissões"),
                        (PermissionEdit, "Editar Permissões", "Permite editar informações de permissões"),
                        (PermissionDelete, "Excluir Permissões", "Permite excluir permissões")
                    }
                },
                {
                    "Ativos",
                    new List<(string Code, string Name, string Description)>
                    {
                        (AtivoView, "Visualizar Ativos", "Permite visualizar a lista de ativos"),
                        (AtivoCreate, "Criar Ativos", "Permite criar novos ativos"),
                        (AtivoEdit, "Editar Ativos", "Permite editar informações de ativos"),
                        (AtivoDelete, "Excluir Ativos", "Permite excluir ativos")
                    }
                },
                {
                    "Ordens de Serviço",
                    new List<(string Code, string Name, string Description)>
                    {
                        (OrdemServicoView, "Visualizar OS", "Permite visualizar ordens de serviço"),
                        (OrdemServicoCreate, "Criar OS", "Permite criar novas ordens de serviço"),
                        (OrdemServicoEdit, "Editar OS", "Permite editar ordens de serviço"),
                        (OrdemServicoDelete, "Excluir OS", "Permite excluir ordens de serviço"),
                        (OrdemServicoApprove, "Aprovar OS", "Permite aprovar ordens de serviço"),
                        (OrdemServicoCancel, "Cancelar OS", "Permite cancelar ordens de serviço")
                    }
                },
                {
                    "Técnicos",
                    new List<(string Code, string Name, string Description)>
                    {
                        (TecnicoView, "Visualizar Técnicos", "Permite visualizar a lista de técnicos"),
                        (TecnicoCreate, "Criar Técnicos", "Permite criar novos técnicos"),
                        (TecnicoEdit, "Editar Técnicos", "Permite editar informações de técnicos"),
                        (TecnicoDelete, "Excluir Técnicos", "Permite excluir técnicos")
                    }
                },
                {
                    "Equipes",
                    new List<(string Code, string Name, string Description)>
                    {
                        (EquipeView, "Visualizar Equipes", "Permite visualizar a lista de equipes"),
                        (EquipeCreate, "Criar Equipes", "Permite criar novas equipes"),
                        (EquipeEdit, "Editar Equipes", "Permite editar informações de equipes"),
                        (EquipeDelete, "Excluir Equipes", "Permite excluir equipes")
                    }
                },
                {
                    "Peças",
                    new List<(string Code, string Name, string Description)>
                    {
                        (PecaView, "Visualizar Peças", "Permite visualizar a lista de peças"),
                        (PecaCreate, "Criar Peças", "Permite criar novas peças"),
                        (PecaEdit, "Editar Peças", "Permite editar informações de peças"),
                        (PecaDelete, "Excluir Peças", "Permite excluir peças")
                    }
                },
                {
                    "Dashboard",
                    new List<(string Code, string Name, string Description)>
                    {
                        (DashboardView, "Visualizar Dashboard", "Permite visualizar o dashboard básico"),
                        (DashboardViewAdvanced, "Dashboard Avançado", "Permite visualizar métricas avançadas do dashboard")
                    }
                },
                {
                    "Relatórios",
                    new List<(string Code, string Name, string Description)>
                    {
                        (RelatorioView, "Visualizar Relatórios", "Permite visualizar relatórios"),
                        (RelatorioCreate, "Criar Relatórios", "Permite criar novos relatórios"),
                        (RelatorioExport, "Exportar Relatórios", "Permite exportar relatórios")
                    }
                },
                {
                    "Auditoria",
                    new List<(string Code, string Name, string Description)>
                    {
                        (AuditoriaView, "Visualizar Logs de Auditoria", "Permite visualizar logs de auditoria"),
                        (AuditoriaConfig, "Configurar Auditoria", "Permite configurar o sistema de auditoria"),
                        (AuditoriaIntegrity, "Verificar Integridade", "Permite verificar integridade dos logs")
                    }
                },
                {
                    "Sistema",
                    new List<(string Code, string Name, string Description)>
                    {
                        (SystemConfig, "Configurar Sistema", "Permite configurar parâmetros do sistema"),
                        (SystemMaintenance, "Manutenção do Sistema", "Permite executar tarefas de manutenção")
                    }
                }
            };
        }
    }
}