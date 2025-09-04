namespace MatMob.Services
{
    /// <summary>
    /// Constantes para as ações de auditoria
    /// </summary>
    public static class AuditActions
    {
        // Operações CRUD básicas
        public const string CREATE = "CREATE";
        public const string VIEW = "VIEW";
        public const string UPDATE = "UPDATE";
        public const string DELETE = "DELETE";

        // Autenticação e autorização
        public const string LOGIN = "LOGIN";
        public const string LOGIN_FAILED = "LOGIN_FAILED";
        public const string LOGOUT = "LOGOUT";
        public const string UNAUTHORIZED_ACCESS = "UNAUTHORIZED_ACCESS";

        // Operações de importação/exportação
        public const string EXPORT = "EXPORT";
        public const string IMPORT = "IMPORT";

        // Aprovações e rejeições
        public const string APPROVE = "APPROVE";
        public const string REJECT = "REJECT";

        // Operações de sistema
        public const string SYSTEM_ERROR = "SYSTEM_ERROR";
        public const string CONFIGURATION_CHANGE = "CONFIGURATION_CHANGE";
        public const string BACKUP = "BACKUP";
        public const string RESTORE = "RESTORE";

        // Operações específicas do domínio
        public const string SCHEDULE = "SCHEDULE";
        public const string CANCEL = "CANCEL";
        public const string COMPLETE = "COMPLETE";
        public const string ASSIGN = "ASSIGN";
        public const string UNASSIGN = "UNASSIGN";
    }

    /// <summary>
    /// Constantes para as categorias de auditoria
    /// </summary>
    public static class AuditCategory
    {
        public const string DATA_ACCESS = "DATA_ACCESS";
        public const string DATA_MODIFICATION = "DATA_MODIFICATION";
        public const string AUTHENTICATION = "AUTHENTICATION";
        public const string AUTHORIZATION = "AUTHORIZATION";
        public const string BUSINESS_PROCESS = "BUSINESS_PROCESS";
        public const string SYSTEM_ADMINISTRATION = "SYSTEM_ADMINISTRATION";
        public const string REPORTING = "REPORTING";
        public const string CONFIGURATION = "CONFIGURATION";
        public const string SECURITY = "SECURITY";
        public const string COMPLIANCE = "COMPLIANCE";
    }

    /// <summary>
    /// Constantes para os níveis de severidade
    /// </summary>
    public static class AuditSeverity
    {
        public const string DEBUG = "DEBUG";
        public const string INFO = "INFO";
        public const string WARNING = "WARNING";
        public const string ERROR = "ERROR";
        public const string CRITICAL = "CRITICAL";
    }
}