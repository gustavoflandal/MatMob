# Sistema Gestor para Manutenção de Ativos - Sistema de Gestão de Ativos de Mobilidade Urbana

## 📋 Sobre o Projeto

O **Sistema Gestor para Manutenção de Ativos** é um sistema completo de gestão de ativos de mobilidade urbana desenvolvido em ASP.NET Core 9. O sistema permite gerenciar semáforos, radares, câmeras de monitoramento e outros equipamentos urbanos, incluindo manutenção, estoque de peças e equipes técnicas.

## 🚀 Tecnologias Utilizadas

- **Backend**: ASP.NET Core 9 (MVC)
- **Banco de Dados**: MySQL 8.0+
- **ORM**: Entity Framework Core
- **Autenticação**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, HTML5, CSS3, JavaScript
- **Gráficos**: Chart.js
- **Ícones**: Font Awesome 6

## 📦 Funcionalidades

### Módulo de Ativos
- ✅ Cadastro completo de ativos (semáforos, radares, câmeras, etc.)
- ✅ Controle de status (Ativo, Em Manutenção, Inativo)
- ✅ Localização e informações técnicas
- ✅ Histórico de manutenções

### Módulo de Manutenção
- ✅ Criação e gestão de Ordens de Serviço (OS)
- ✅ Controle de status das OS (Aberta, Em Andamento, Concluída)
- ✅ Cálculo de custos e tempo gasto
- ✅ Associação com equipes e técnicos

### Módulo de Pessoal
- ✅ Cadastro de técnicos com especialização
- ✅ Gestão de equipes de manutenção
- ✅ Alocação de recursos para OS

### Módulo de Estoque
- ✅ Controle de peças e componentes
- ✅ Alertas de estoque baixo
- ✅ Movimentação de entrada/saída
- ✅ Integração com OS

### Dashboard e Relatórios
- ✅ Dashboard interativo com KPIs
- ✅ Gráficos de status das OS
- ✅ Alertas automáticos
- ✅ Custos por período

## 🛠️ Instalação

### Pré-requisitos
- .NET 9 SDK
- MySQL Server 8.0+
- Visual Studio 2022 ou VS Code

### Passo a Passo

1. **Clone o repositório**
```bash
git clone <repository-url>
cd MatMob
```

2. **Configure o banco de dados**
   - Crie um banco MySQL chamado `MatMob_db`
   - Atualize a connection string no `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=MatMob_db;Uid=root;Pwd=root;CharSet=utf8mb4;"
  }
}
```

3. **Instale as dependências**
```bash
dotnet restore
```

4. **Execute as migrações**
```bash
dotnet ef database update
```

5. **Execute o projeto**
```bash
dotnet run
```

6. **Acesse o sistema**
   - URL: `https://localhost:7000` ou `http://localhost:5000`
   - Usuário padrão: `admin@matmob.com`
   - Senha padrão: `Admin123!`

## 👥 Perfis de Usuário

### Administrador
- Acesso total ao sistema
- Pode excluir registros
- Gerencia usuários e permissões

### Gestor
- Acesso a todos os módulos
- Não pode excluir registros críticos
- Foca em gestão e relatórios

### Técnico
- Acesso limitado
- Pode criar e atualizar OS
- Visualiza informações de ativos e estoque

## 📱 Interface

O sistema possui interface responsiva que se adapta a diferentes dispositivos:
- **Desktop**: Layout completo com sidebar
- **Tablet**: Menu colapsível
- **Mobile**: Interface otimizada para toque

## 🎨 Capturas de Tela

### Dashboard Principal
- KPIs em tempo real
- Gráficos interativos
- Sistema de alertas

### Gestão de Ativos
- Lista com filtros avançados
- Formulários intuitivos
- Visualização detalhada

### Ordens de Serviço
- Workflow completo
- Controle de status
- Cálculo automático de custos

## 🔧 Configuração Avançada

### Variáveis de Ambiente
```json
{
  "ApplicationSettings": {
    "ApplicationName": "MatMob - Sistema de Gestão de Ativos",
    "Version": "1.0.0",
    "Environment": "Development"
  }
}
```

### Logs
O sistema utiliza o logging nativo do ASP.NET Core:
- **Information**: Operações normais
- **Warning**: Alertas do sistema
- **Error**: Erros e exceções

## 📊 Arquitetura

```
MatMob/
├── Controllers/          # Controllers MVC
├── Models/
│   ├── Entities/        # Entidades do domínio
│   └── ViewModels/      # ViewModels para as views
├── Views/               # Views Razor
├── Services/            # Serviços de negócio
├── Data/                # Contexto do EF Core
├── Extensions/          # Extensões e helpers
└── wwwroot/            # Arquivos estáticos
```

## 🚀 Deploy

### Desenvolvimento
```bash
dotnet run --environment Development
```

### Produção
```bash
dotnet publish -c Release
```

## 📈 Roadmap

- [ ] Módulo de relatórios avançados (PDF/Excel)
- [ ] Integração com APIs de mapas
- [ ] Notificações push
- [ ] App mobile
- [ ] Dashboard em tempo real com SignalR

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

## 📞 Suporte

Para suporte técnico ou dúvidas sobre o sistema:
- Email: suporte@matmob.com
- Documentação: [Wiki do Projeto]
- Issues: [GitHub Issues]

---

**MatMob** - Transformando a gestão de ativos urbanos 🏙️
