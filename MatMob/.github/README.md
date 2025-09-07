
# PROMPT DE SISTEMA.

Você é um desenvolvedor sênior de software com 20 anos de experiência, especializado em desenvolvimento web com as mais recentes tecnologias Microsoft. Seu papel é atuar como um mentor e arquiteto de soluções para um projeto que utiliza .NET 9, ASP.NET Core, Razor Pages/Views, Bootstrap 5, SQL Server, HTML5 e CSS3. Voce domina vários idiomas, porém nesse projeto voce deve utilizar o portugues do Brasil.

Sua responsabilidade é guiar o usuário na criação de um aplicativo web robusto e escalável. Antes de qualquer resposta ou sugestão, você deve perguntar e obter o máximo de contexto sobre o projeto, incluindo, mas não se limitando a:

1.  **Objetivo do Projeto:** Qual é o propósito principal da aplicação? (Ex: e-commerce, sistema de gerenciamento de conteúdo, painel de BI, etc.)
2.  **Público-Alvo:** Quem são os usuários finais? Isso afeta a experiência do usuário (UX) e o design de interface (UI).
3.  **Funcionalidades-Chave:** Quais são as principais features que a aplicação deve ter? (Ex: CRUD de usuários, carrinho de compras, sistema de login/autenticação, etc.)
4.  **Estrutura de Dados:** Quais entidades e tabelas são necessárias para a base de dados? Qual é o relacionamento entre elas?
5.  **Requisitos Não-Funcionais:** Há requisitos de performance, segurança, acessibilidade ou escalabilidade específicos? (Ex: "O site deve suportar 1000 usuários simultâneos", "A aplicação deve ser acessível para pessoas com deficiência", etc.)
6.  **Nível de Experiência do Usuário:** Qual o seu nível de familiaridade com as tecnologias (.NET, SQL, HTML, CSS)? Isso ajuda a moldar a profundidade das explicações.

Com base nas respostas do usuário, você deve:

* **Validar e Sugerir a Arquitetura:** Propor uma arquitetura de projeto clara e bem-definida, como a arquitetura em camadas (Data Access Layer, Business Logic Layer, Presentation Layer) ou o padrão de repositório, explicando os benefícios de cada uma.
* **Detalhar o Stack Tecnológico:** Fornecer exemplos de código e explicações detalhadas sobre como as tecnologias se integram. Explique como o **ASP.NET Core** lida com as requisições HTTP, como o **Razor** renderiza o HTML dinamicamente, como o **Bootstrap** facilita o design responsivo, e como o **SQL Server** gerencia a persistência dos dados.
* **Abordar Boas Práticas:** Insistir em boas práticas de desenvolvimento, como o uso de **Injeção de Dependência**, o princípio **SOLID**, versionamento de código, e testes unitários. Explique por que essas práticas são cruciais para a manutenção e escalabilidade do projeto.
* **Resolver Problemas Específicos:** Quando o usuário apresentar um problema, forneça não apenas a solução, mas também a explicação sobre o "porquê" da solução, ligando-a aos conceitos de design e arquitetura.





# 📋 DOCUMENTAÇÃO COMPLETA - MatMob
## Sistema de Gestão de Ativos de Mobilidade Urbana

---

## 📚 ÍNDICE

1. [Visão Geral](#visão-geral)
2. [Tecnologias Utilizadas](#tecnologias-utilizadas)
3. [Arquitetura do Sistema](#arquitetura-do-sistema)
4. [Requisitos do Sistema](#requisitos-do-sistema)
5. [Instalação e Configuração](#instalação-e-configuração)
6. [Estrutura do Projeto](#estrutura-do-projeto)
7. [Funcionalidades](#funcionalidades)
8. [Guia de Uso](#guia-de-uso)
9. [API e Endpoints](#api-e-endpoints)
10. [Banco de Dados](#banco-de-dados)
11. [Segurança](#segurança)
12. [Deploy e Produção](#deploy-e-produção)
13. [Manutenção](#manutenção)
14. [FAQ](#faq)

---

## 🎯 VISÃO GERAL

O **MatMob** é um sistema completo de gestão de ativos de mobilidade urbana desenvolvido em ASP.NET Core 9. O sistema permite gerenciar equipamentos como semáforos, radares, câmeras de monitoramento, sensores de tráfego e outros dispositivos urbanos, incluindo suas manutenções, estoque de peças e equipes técnicas.

### Principais Objetivos
- ✅ Centralizar o controle de ativos urbanos
- ✅ Automatizar processos de manutenção
- ✅ Otimizar gestão de estoque
- ✅ Fornecer relatórios e KPIs em tempo real
- ✅ Melhorar eficiência operacional

---

## 🛠️ TECNOLOGIAS UTILIZADAS

### Backend
- **Framework**: ASP.NET Core 9.0
- **Linguagem**: C# 12
- **Padrão**: MVC (Model-View-Controller)
- **ORM**: Entity Framework Core 9.0
- **Banco de Dados**: MySQL 8.0+
- **Autenticação**: ASP.NET Core Identity

### Frontend
- **UI Framework**: Bootstrap 5.3
- **JavaScript**: Vanilla JS + Chart.js
- **CSS**: CSS3 + Bootstrap customizado
- **Ícones**: Font Awesome 6.4
- **Responsividade**: Mobile-first design

### Ferramentas de Desenvolvimento
- **IDE**: Visual Studio 2022 / VS Code
- **Controle de Versão**: Git
- **Containerização**: Docker + Docker Compose
- **Package Manager**: NuGet

---

## 🏗️ ARQUITETURA DO SISTEMA

### Padrão MVC
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│     VIEW        │    │   CONTROLLER    │    │     MODEL       │
│                 │    │                 │    │                 │
│ - Dashboard     │◄──►│ - Dashboard     │◄──►│ - Entities      │
│ - Ativos        │    │ - Ativos        │    │ - ViewModels    │
│ - OrdemServico  │    │ - OrdemServico  │    │ - Services      │
│ - Tecnicos      │    │ - Tecnicos      │    │ - Data Context  │
│ - Equipes       │    │ - Equipes       │    │                 │
│ - Pecas         │    │ - Pecas         │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Camadas do Sistema
1. **Presentation Layer** (Views/Controllers)
2. **Business Logic Layer** (Services)
3. **Data Access Layer** (Entity Framework)
4. **Database Layer** (MySQL)

---

## 💻 REQUISITOS DO SISTEMA

### Requisitos Mínimos
- **Sistema Operacional**: Windows 10+ / Linux / macOS
- **.NET**: SDK 9.0 ou superior
- **Banco de Dados**: MySQL 8.0+ ou MariaDB 10.5+
- **Memória RAM**: 4GB mínimo (8GB recomendado)
- **Espaço em Disco**: 2GB livres
- **Navegador**: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+

### Requisitos para Desenvolvimento
- **Visual Studio 2022** (Community ou superior) OU **VS Code**
- **MySQL Workbench** (opcional, para administração do banco)
- **Git** para controle de versão
- **Docker Desktop** (opcional, para containerização)

---

## 🚀 INSTALAÇÃO E CONFIGURAÇÃO

### Passo 1: Pré-requisitos
```bash
# Verificar versão do .NET
dotnet --version

# Deve retornar 9.0.x ou superior
```

### Passo 2: Clonar o Projeto
```bash
git clone <repository-url>
cd MatMob
```

### Passo 3: Configurar Banco de Dados

#### 3.1 Instalar MySQL
- Baixe e instale o MySQL Server 8.0+
- Configure usuário root com senha

#### 3.2 Criar Banco de Dados
```sql
CREATE DATABASE MatMob_db;
CREATE USER 'matmob_user'@'localhost' IDENTIFIED BY 'senha_segura';
GRANT ALL PRIVILEGES ON MatMob_db.* TO 'matmob_user'@'localhost';
FLUSH PRIVILEGES;
```

#### 3.3 Configurar Connection String
Edite o arquivo `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=MatMob_db;Uid=matmob_user;Pwd=senha_segura;CharSet=utf8mb4;"
  }
}
```

### Passo 4: Executar Migrações
```bash
cd MatMob
dotnet restore
dotnet ef database update
```

### Passo 5: Executar o Sistema
```bash
dotnet run
```

### Passo 6: Acessar o Sistema
- **URL**: https://localhost:7000 ou http://localhost:5000
- **Usuário**: admin@matmob.com
- **Senha**: Admin123!

---

## 📁 ESTRUTURA DO PROJETO

```
MatMob/
├── Controllers/              # Controllers MVC
│   ├── DashboardController.cs
│   ├── AtivosController.cs
│   ├── OrdensServicoController.cs
│   ├── TecnicosController.cs
│   ├── EquipesController.cs
│   └── PecasController.cs
├── Models/
│   ├── Entities/            # Entidades do domínio
│   │   ├── Ativo.cs
│   │   ├── OrdemServico.cs
│   │   ├── Tecnico.cs
│   │   ├── Equipe.cs
│   │   ├── Peca.cs
│   │   └── PlanoManutencao.cs
│   └── ViewModels/          # ViewModels
│       └── DashboardViewModel.cs
├── Views/                   # Views Razor
│   ├── Dashboard/
│   ├── Ativos/
│   ├── OrdensServico/
│   ├── Tecnicos/
│   ├── Equipes/
│   ├── Pecas/
│   └── Shared/
├── Services/                # Serviços de negócio
│   ├── IDashboardService.cs
│   └── DashboardService.cs
├── Data/                    # Contexto EF Core
│   └── ApplicationDbContext.cs
├── Extensions/              # Extensões
│   └── EnumExtensions.cs
├── wwwroot/                # Arquivos estáticos
│   ├── css/
│   ├── js/
│   └── lib/
├── Migrations/             # Migrações EF Core
├── Properties/
├── appsettings.json        # Configurações
├── Program.cs             # Ponto de entrada
├── Dockerfile            # Container Docker
├── docker-compose.yml    # Orquestração
└── README.md            # Documentação básica
```

---

## ⚙️ FUNCIONALIDADES

### 🏢 Módulo de Ativos
**Objetivo**: Gerenciar equipamentos de mobilidade urbana

**Funcionalidades**:
- ✅ Cadastro de ativos (semáforos, radares, câmeras, sensores)
- ✅ Controle de status (Ativo, Em Manutenção, Inativo, Descartado)
- ✅ Localização geográfica
- ✅ Informações técnicas (número de série, data de instalação)
- ✅ Histórico de manutenções
- ✅ Filtros avançados de busca
- ✅ Relatórios por tipo e status

**Telas**:
- Lista de ativos com filtros
- Cadastro/edição de ativo
- Detalhes do ativo
- Histórico de manutenções

### 🔧 Módulo de Manutenção
**Objetivo**: Gerenciar ordens de serviço e manutenções

**Funcionalidades**:
- ✅ Criação de Ordens de Serviço (OS)
- ✅ Workflow de status (Aberta → Em Andamento → Concluída)
- ✅ Tipos de serviço (Preventiva, Corretiva, Instalação, Inspeção)
- ✅ Priorização (Baixa, Média, Alta, Crítica)
- ✅ Cálculo de custos e tempo gasto
- ✅ Associação com equipes e técnicos
- ✅ Controle de peças utilizadas
- ✅ Relatórios de produtividade

**Telas**:
- Lista de OS com filtros
- Criação/edição de OS
- Detalhes da OS
- Atualização de status

### 👥 Módulo de Pessoal
**Objetivo**: Gerenciar técnicos e equipes

**Funcionalidades**:
- ✅ Cadastro de técnicos
- ✅ Especialização e competências
- ✅ Controle de status (Ativo, Inativo, Férias, Licença)
- ✅ Formação de equipes
- ✅ Alocação para OS
- ✅ Agenda de trabalho
- ✅ Relatórios de produtividade

**Telas**:
- Lista de técnicos
- Cadastro/edição de técnico
- Gestão de equipes
- Agenda de equipe

### 📦 Módulo de Estoque
**Objetivo**: Controlar peças e componentes

**Funcionalidades**:
- ✅ Cadastro de peças
- ✅ Controle de quantidade em estoque
- ✅ Estoque mínimo e alertas
- ✅ Movimentações (Entrada, Saída, Ajuste)
- ✅ Integração com OS
- ✅ Relatórios de consumo
- ✅ Gestão de fornecedores

**Telas**:
- Lista de peças
- Cadastro/edição de peça
- Movimentações de estoque
- Alertas de estoque baixo

### 📊 Dashboard e Relatórios
**Objetivo**: Fornecer visão gerencial

**Funcionalidades**:
- ✅ KPIs em tempo real
- ✅ Gráficos interativos
- ✅ Alertas automáticos
- ✅ Custos por período
- ✅ Produtividade de equipes
- ✅ Status de ativos
- ✅ Exportação de relatórios

**Métricas Principais**:
- Total de ativos por status
- OS abertas/em andamento/concluídas
- Custo mensal de manutenção
- Peças com estoque baixo
- Técnicos e equipes ativas

---

## 👤 GUIA DE USO

### Primeiro Acesso
1. Acesse https://localhost:7000
2. Faça login com: admin@matmob.com / Admin123!
3. Explore o dashboard principal
4. Configure usuários adicionais se necessário

### Perfis de Usuário

#### 👑 Administrador
- **Acesso**: Total ao sistema
- **Permissões**: 
  - Criar/editar/excluir todos os registros
  - Gerenciar usuários e permissões
  - Acessar todos os relatórios
  - Configurações do sistema

#### 🎯 Gestor
- **Acesso**: Módulos operacionais
- **Permissões**:
  - Criar/editar ativos, OS, técnicos, equipes
  - Não pode excluir registros críticos
  - Acessar relatórios gerenciais
  - Aprovar OS de alto valor

#### 🔧 Técnico
- **Acesso**: Limitado
- **Permissões**:
  - Visualizar ativos e OS
  - Atualizar status de OS
  - Registrar peças utilizadas
  - Criar OS simples

### Fluxo de Trabalho Típico

#### 1. Cadastro de Ativo
1. Menu **Ativos** → **Novo Ativo**
2. Preencher informações básicas
3. Definir localização e status
4. Salvar

#### 2. Criação de OS
1. Menu **Manutenção** → **Nova OS**
2. Selecionar ativo
3. Definir tipo e prioridade
4. Descrever problema
5. Alocar equipe/técnico
6. Salvar

#### 3. Execução de Manutenção
1. Técnico acessa OS
2. Atualiza status para "Em Andamento"
3. Registra peças utilizadas
4. Registra tempo gasto
5. Descreve solução aplicada
6. Finaliza OS

---

## 🔗 API E ENDPOINTS

### Estrutura de Rotas

#### Dashboard
- `GET /Dashboard` - Página principal
- `GET /Dashboard/GetAlertas` - Buscar alertas (AJAX)

#### Ativos
- `GET /Ativos` - Listar ativos
- `GET /Ativos/Create` - Formulário de criação
- `POST /Ativos/Create` - Criar ativo
- `GET /Ativos/Details/{id}` - Detalhes do ativo
- `GET /Ativos/Edit/{id}` - Formulário de edição
- `POST /Ativos/Edit/{id}` - Atualizar ativo
- `GET /Ativos/Delete/{id}` - Confirmar exclusão
- `POST /Ativos/Delete/{id}` - Excluir ativo
- `GET /Ativos/GerarNumeroSerie` - Gerar número série (AJAX)

#### Ordens de Serviço
- `GET /OrdensServico` - Listar OS
- `GET /OrdensServico/Create` - Formulário de criação
- `POST /OrdensServico/Create` - Criar OS
- `GET /OrdensServico/Details/{id}` - Detalhes da OS
- `GET /OrdensServico/Edit/{id}` - Formulário de edição
- `POST /OrdensServico/Edit/{id}` - Atualizar OS
- `POST /OrdensServico/UpdateStatus/{id}` - Atualizar status

#### Técnicos
- `GET /Tecnicos` - Listar técnicos
- `GET /Tecnicos/Create` - Formulário de criação
- `POST /Tecnicos/Create` - Criar técnico
- `GET /Tecnicos/Details/{id}` - Detalhes do técnico
- `GET /Tecnicos/Edit/{id}` - Formulário de edição
- `POST /Tecnicos/Edit/{id}` - Atualizar técnico
- `GET /Tecnicos/Agenda/{id}` - Agenda do técnico

#### Equipes
- `GET /Equipes` - Listar equipes
- `GET /Equipes/Create` - Formulário de criação
- `POST /Equipes/Create` - Criar equipe
- `GET /Equipes/Details/{id}` - Detalhes da equipe
- `GET /Equipes/Edit/{id}` - Formulário de edição
- `POST /Equipes/Edit/{id}` - Atualizar equipe
- `POST /Equipes/AdicionarTecnico` - Adicionar técnico à equipe
- `POST /Equipes/RemoverTecnico` - Remover técnico da equipe

#### Peças
- `GET /Pecas` - Listar peças
- `GET /Pecas/Create` - Formulário de criação
- `POST /Pecas/Create` - Criar peça
- `GET /Pecas/Details/{id}` - Detalhes da peça
- `GET /Pecas/Edit/{id}` - Formulário de edição
- `POST /Pecas/Edit/{id}` - Atualizar peça
- `POST /Pecas/MovimentarEstoque` - Movimentar estoque
- `GET /Pecas/EstoqueBaixo` - Peças com estoque baixo
- `GET /Pecas/GerarCodigo` - Gerar código (AJAX)

---

## 🗄️ BANCO DE DADOS

### Modelo de Dados

#### Tabelas Principais

**Ativos**
```sql
CREATE TABLE Ativos (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nome VARCHAR(100) NOT NULL,
    Tipo VARCHAR(50) NOT NULL,
    NumeroSerie VARCHAR(100) UNIQUE,
    Localizacao VARCHAR(200) NOT NULL,
    DataInstalacao DATETIME(6),
    Status INT NOT NULL,
    Descricao VARCHAR(500),
    DataCadastro DATETIME(6) NOT NULL,
    UltimaAtualizacao DATETIME(6)
);
```

**OrdensServico**
```sql
CREATE TABLE OrdensServico (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    NumeroOS VARCHAR(20) NOT NULL UNIQUE,
    TipoServico INT NOT NULL,
    Prioridade INT NOT NULL,
    Status INT NOT NULL,
    DescricaoProblema VARCHAR(1000) NOT NULL,
    SolucaoAplicada VARCHAR(1000),
    DataAbertura DATETIME(6) NOT NULL,
    DataInicio DATETIME(6),
    DataConclusao DATETIME(6),
    TempoGastoHoras DECIMAL(5,2),
    CustoEstimado DECIMAL(10,2),
    CustoReal DECIMAL(10,2),
    AtivoId INT NOT NULL,
    EquipeId INT,
    TecnicoResponsavelId INT,
    FOREIGN KEY (AtivoId) REFERENCES Ativos(Id),
    FOREIGN KEY (EquipeId) REFERENCES Equipes(Id),
    FOREIGN KEY (TecnicoResponsavelId) REFERENCES Tecnicos(Id)
);
```

**Tecnicos**
```sql
CREATE TABLE Tecnicos (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nome VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Telefone VARCHAR(20),
    Celular VARCHAR(20),
    Especializacao VARCHAR(100) NOT NULL,
    DataContratacao DATETIME(6),
    Status INT NOT NULL,
    Observacoes VARCHAR(500),
    DataCadastro DATETIME(6) NOT NULL
);
```

### Relacionamentos
- **Ativo** → **OrdemServico** (1:N)
- **Ativo** → **PlanoManutencao** (1:N)
- **Equipe** ↔ **Tecnico** (N:N via EquipeTecnico)
- **OrdemServico** → **ItemOrdemServico** (1:N)
- **Peca** → **MovimentacaoEstoque** (1:N)

### Índices Importantes
- `IX_Ativos_NumeroSerie` (UNIQUE)
- `IX_OrdensServico_NumeroOS` (UNIQUE)
- `IX_Tecnicos_Email` (UNIQUE)
- `IX_Pecas_Codigo` (UNIQUE)

---

## 🔒 SEGURANÇA

### Autenticação
- **ASP.NET Core Identity** para gestão de usuários
- **Hash de senhas** com algoritmos seguros
- **Lockout** após tentativas falhadas
- **Confirmação de email** (configurável)

### Autorização
- **Role-based** (Administrador, Gestor, Técnico)
- **Claims-based** para permissões granulares
- **Atributos de autorização** nos controllers
- **Proteção CSRF** em formulários

### Proteções Implementadas
- ✅ **SQL Injection**: Entity Framework com parâmetros
- ✅ **XSS**: Encoding automático do Razor
- ✅ **CSRF**: Anti-forgery tokens
- ✅ **HTTPS**: Redirecionamento forçado
- ✅ **Validação**: Server-side e client-side

### Configurações de Segurança
```csharp
// Program.cs
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
});
```

---

## 🚀 DEPLOY E PRODUÇÃO

### Deploy com IIS (Windows)

#### 1. Publicar Aplicação
```bash
dotnet publish -c Release -o ./publish
```

#### 2. Configurar IIS
- Instalar ASP.NET Core Runtime
- Criar Application Pool (.NET Core)
- Configurar site no IIS
- Copiar arquivos publicados

#### 3. Configurar appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=MatMob_db;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

### Deploy com Docker

#### 1. Build da Imagem
```bash
docker build -t matmob:latest .
```

#### 2. Executar com Docker Compose
```bash
docker-compose up -d
```

#### 3. Configurar Reverse Proxy (Nginx)
```nginx
server {
    listen 80;
    server_name matmob.exemplo.com;
    
    location / {
        proxy_pass http://localhost:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### Deploy na Nuvem (Azure)

#### 1. Azure App Service
- Criar App Service (.NET 9)
- Configurar connection string
- Deploy via GitHub Actions

#### 2. Azure Database for MySQL
- Criar instância MySQL
- Configurar firewall
- Executar migrações

---

## 🔧 MANUTENÇÃO

### Backup do Banco de Dados
```bash
# Backup diário
mysqldump -u root -p MatMob_db > backup_$(date +%Y%m%d).sql

# Restaurar backup
mysql -u root -p MatMob_db < backup_20250101.sql
```

### Logs do Sistema
- **Localização**: `/logs/` ou Event Viewer (Windows)
- **Níveis**: Information, Warning, Error
- **Rotação**: Diária (configurável)

### Monitoramento
- **Application Insights** (Azure)
- **Health Checks** endpoint: `/health`
- **Mét
 estáricas** de performance

### Atualizações
```bash
# Atualizar dependências
dotnet list package --outdated
dotnet add package <PackageName>

# Aplicar migrações
dotnet ef database update
```

---

## ❓ FAQ

### P: Como alterar a senha do usuário admin?
**R**: Acesse Identity → Manage → Change Password ou use o UserManager programaticamente.

### P: Como adicionar novos tipos de ativos?
**R**: Edite o enum no modelo Ativo.cs e adicione na view Create/Edit.

### P: Como configurar email para notificações?
**R**: Configure SMTP no appsettings.json e implemente IEmailSender.

### P: Como fazer backup automatizado?
**R**: Configure um job no cron (Linux) ou Task Scheduler (Windows) para executar mysqldump.

### P: Como adicionar novos camposnas tabelas?
**R**: 
1. Edite o modelo (Entidade)
2. Crie nova migração: `dotnet ef migrations add AddNewField`
3. Aplique: `dotnet ef database update`
4. Atualize as views

### P: Sistema está lento, como otimizar?
**R**: 
- Verifique índices do banco
- Implemente cache (Redis)
- Otimize queries N+1
- Configure connection pooling

### P: Como integrar com sistemas externos?
**R**: Crie controllers de API, implemente DTOs e configure CORS se necessário.

---

## 📞 SUPORTE

### Contato
- **Email**: suporte@matmob.com
- **Telefone**: (11) 1234-5678
- **Horário**: Segunda a Sexta, 8h às 18h

### Recursos
- **Wiki**: [Documentação Online]
- **Issues**: [GitHub Issues]
- **Fórum**: [Comunidade]

### Níveis de Suporte
- **Básico**: Email (48h resposta)
- **Profissional**: Email + Telefone (24h resposta)
- **Enterprise**: Suporte dedicado (4h resposta)

---



## 🎯 ROADMAP

### Versão 1.1 (Q2 2025)
- [ ] Módulo de relatórios avançados (PDF/Excel)
- [ ] Integração com APIs de mapas
- [ ] Notificações push em tempo real
- [ ] Dashboard mobile responsivo

### Versão 1.2 (Q3 2025)
- [ ] API REST completa
- [ ] Aplicativo mobile (React Native)
- [ ] Integração IoT para monitoramento
- [ ] Machine Learning para manutenção preditiva

### Versão 2.0 (Q4 2025)
- [ ] Microserviços architecture
- [ ] Multi-tenancy
- [ ] Integração com sistemas ERP
- [ ] Analytics avançado

---

**MatMob v1.0** - Sistema de Gestão de Ativos de Mobilidade Urbana
*Desenvolvido com ❤️ em ASP.NET Core 9*

---
*Última atualização: Janeiro 2025*

