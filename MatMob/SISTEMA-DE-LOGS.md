## Estrutura da tabela AuditLogs 

-- MatMob_db.AuditLogs definition

CREATE TABLE `AuditLogs` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UserId` varchar(255) DEFAULT NULL,
  `UserName` varchar(255) DEFAULT NULL,
  `IpAddress` varchar(45) DEFAULT NULL,
  `UserAgent` varchar(500) DEFAULT NULL,
  `Action` varchar(50) NOT NULL,
  `EntityName` varchar(100) DEFAULT NULL,
  `EntityId` int(11) DEFAULT NULL,
  `PropertyName` varchar(100) DEFAULT NULL,
  `OldValue` text,
  `NewValue` text,
  `OldData` text,
  `NewData` text,
  `Description` varchar(1000) DEFAULT NULL,
  `Context` varchar(200) DEFAULT NULL,
  `Severity` varchar(20) NOT NULL,
  `Category` varchar(50) DEFAULT NULL,
  `AdditionalData` text,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `Duration` bigint(20) DEFAULT NULL,
  `Success` tinyint(1) NOT NULL,
  `ErrorMessage` varchar(2000) DEFAULT NULL,
  `StackTrace` text,
  `SessionId` varchar(255) DEFAULT NULL,
  `CorrelationId` varchar(255) DEFAULT NULL,
  `HttpMethod` varchar(10) DEFAULT NULL,
  `RequestUrl` varchar(500) DEFAULT NULL,
  `HttpStatusCode` int(11) DEFAULT NULL,
  `PermanentRetention` tinyint(1) NOT NULL,
  `ExpirationDate` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_AuditLogs_Action` (`Action`),
  KEY `IX_AuditLogs_CorrelationId` (`CorrelationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `AuditModuleConfigs` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Module` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Process` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Enabled` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` timestamp(6) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_AuditModuleConfigs_Module_Process` (`Module`,`Process`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

1. Requisitos Fundamentais do sistema de captura de Logs para auditorias.

Imutabilidade: Os logs devem ser somente de leitura e não podem ser alterados ou excluídos após a criação. Isso garante a confiabilidade do registro para fins de auditoria. Tecnologias como blockchain ou hashes criptográficos podem ser usadas para reforçar essa imutabilidade.

Timestamp Preciso: Cada entrada de log deve ter um carimbo de tempo (timestamp) preciso e sincronizado com um servidor de tempo confiável (como NTP). Isso é crucial para reconstruir a sequência exata de eventos.

Identificação do Usuário: É fundamental registrar o ID do usuário que realizou a ação. Em casos de sistemas automatizados, o log deve identificar o processo ou a aplicação responsável.

Detalhe da Ação: O log deve descrever a ação realizada, como "criar", "editar", "excluir", "aprovar", "iniciar", "finalizar", etc.

Objeto da Ação: O log precisa identificar o ativo, a ordem de serviço, o local, a peça ou qualquer outro objeto que foi afetado pela ação. Por exemplo, "Ordem de Serviço #12345" ou "Ativo 'Bomba Centrífuga 01'".

Dados Antes e Depois (Before/After): Para ações de modificação, é crucial registrar o estado original do dado e o novo estado. Por exemplo, se o status de uma ordem de serviço foi alterado de "Pendente" para "Em Andamento", o log deve registrar ambas as informações.

Localização e Endereço IP: Registrar o endereço IP de onde a ação foi iniciada pode ajudar a identificar acessos não autorizados ou anômalos.

2. O que deve ser registrado (Eventos Críticos)
Os logs de auditoria em um sistema de manutenção de ativos devem cobrir, no mínimo, as seguintes categorias:

a) Eventos de Acesso e Autenticação
Login bem-sucedido e falhas: Registrar tentativas de login, incluindo usuário, IP, e se a tentativa foi bem-sucedida ou falhou.

Logout: Registrar o momento em que o usuário sai do sistema.

Alteração de senha: Registrar quando uma senha é alterada e por qual usuário.

Bloqueio de conta: Registrar quando uma conta é bloqueada por tentativas de login incorretas.

b) Eventos de Dados Mestres
Criação, edição ou exclusão de ativos: O log deve detalhar quem criou, editou ou excluiu um ativo, e quais campos foram alterados (ex: nome do ativo, número de série, localização).

Alteração de dados de localização: Mudanças em locais de ativos, áreas de trabalho, etc.

Modificação de listas de peças e estoque: Qualquer alteração no inventário, incluindo a adição, remoção ou ajuste de quantidades.

c) Eventos de Manutenção (Ordem de Serviço)
Criação, modificação, aprovação e fechamento de Ordens de Serviço (OS): Acompanhar o ciclo de vida completo de uma OS.

Atribuição de OS: Registrar quando uma OS é atribuída a um técnico ou equipe.

Alteração de status: Acompanhar todas as mudanças de status de uma OS (ex: de "Programada" para "Em Andamento").

Registro de atividades: Registrar as atividades e os comentários adicionados por técnicos na OS.

Consumo de peças: Registrar as peças retiradas do estoque para uma OS específica.

Registro de leituras de medidores: Capturar o momento e o usuário que registrou a leitura de um medidor.

d) Eventos de Configuração e Administração
Alterações de permissões de usuário: Quem alterou e para qual usuário as permissões foram modificadas.

Alteração de fluxos de trabalho: Modificações nos fluxos de aprovação de ordens de serviço.

Criação e edição de checklists ou procedimentos padrão.

3. Arquitetura e Tecnologia do Sistema
Um sistema de logs eficiente não é apenas sobre o que é registrado, mas também sobre como é armazenado e acessado.

Coletor de Logs (Log Collector): Pode ser um serviço ou biblioteca integrada ao sistema de manutenção que captura eventos e os envia para um sistema de armazenamento centralizado. Isso garante que os logs não sejam perdidos mesmo que o aplicativo principal falhe.

Sistema de Armazenamento de Logs:

Banco de Dados: Usar um banco de dados dedicado (como PostgreSQL, MongoDB ou Cassandra) para armazenar os logs é uma boa prática.

Serviços de Logs Centralizados: Soluções como ELK Stack (Elasticsearch, Logstash, Kibana), Splunk ou Grafana Loki são ideais. Elas oferecem escalabilidade, alta disponibilidade e ferramentas de busca e visualização poderosas.

Interface de Auditoria e Relatórios: Deve haver uma interface de usuário dedicada que permita aos auditores:

Pesquisar logs: Filtrar por data, usuário, tipo de evento, ativo, etc.

Visualizar logs: Exibir os logs de forma clara e legível.

Gerar relatórios: Criar relatórios de auditoria para um período de tempo ou para um ativo específico.

Alertas: Configurar alertas para eventos suspeitos (ex: login de um usuário de um IP incomum).

4. Boas Práticas e Considerações Adicionais
Política de Retenção de Logs: Definir por quanto tempo os logs devem ser armazenados (ex: 5 anos, 7 anos) para cumprir com as normas regulatórias.

Segurança dos Logs: Os logs em si são dados sensíveis. Eles devem ser protegidos contra acesso não autorizado e adulteração. O armazenamento deve ser criptografado e o acesso restrito.

Performance: A geração de logs deve ser assíncrona para não impactar o desempenho do sistema principal. O envio e o processamento de logs devem ser feitos em segundo plano.

Conformidade (Compliance): O sistema deve ser projetado para atender a normas como ISO 27001 (Segurança da Informação) e outras regulamentações específicas do setor (ex: farmacêutico, energia).

Telas Essenciais para o Gerenciamento de Logs de Auditoria
Para um gerenciamento de captura de logs eficaz, o sistema não deve apenas registrar os dados, mas também torná-los acessíveis e compreensíveis para os auditores e administradores. As telas a seguir são fundamentais para isso:

1. Tela de Pesquisa e Visualização de Logs (Dashboard de Auditoria)
Esta é a tela principal e a mais importante. Ela serve como o centro de comando para a análise de logs.

Filtros Avançados: Deve permitir a pesquisa de logs por múltiplos critérios, como:

Período de Tempo: Filtros pré-definidos (últimas 24h, 7 dias, 30 dias) e um seletor de data customizado.

Usuário: Pesquisa por nome ou ID de usuário para rastrear atividades específicas.

Tipo de Evento: Filtros para eventos de login, criação/edição de ativos, alterações em ordens de serviço, etc.

Objeto de Ação: Pesquisa por ID de ordem de serviço, nome do ativo ou ID da peça.

Endereço IP: Para investigar acessos de locais suspeitos.

Tabela de Logs: A tabela deve exibir as informações de forma clara e organizada, com colunas para:

Timestamp: Carimbo de tempo exato da ação.

Usuário: Nome ou ID do usuário que executou a ação.

Ação: Descrição da ação (ex: "Criar", "Editar Status", "Excluir").

Objeto: Objeto afetado (ex: "Ordem de Serviço #12345").

Detalhes: Um campo com mais detalhes, como os dados "antes" e "depois" da alteração.

Visualização Detalhada: Ao clicar em um log na tabela, o sistema deve abrir uma tela de detalhes com todas as informações disponíveis sobre aquele evento, incluindo o JSON completo do log, se aplicável, para uma análise aprofundada.

Gráficos e Estatísticas: Gráficos simples que mostram a frequência de eventos ao longo do tempo ou o tipo de evento mais comum podem ajudar a identificar anomalias rapidamente.

2. Tela de Relatórios de Auditoria
Esta tela permite a geração de relatórios formais para conformidade e análise periódica.

Modelos de Relatório: Oferecer modelos pré-definidos para relatórios comuns, como "Relatório de Atividade por Usuário" ou "Relatório de Alterações em Ativos Críticos".

Exportação: A capacidade de exportar os relatórios em formatos como PDF ou CSV é crucial para compartilhamento e arquivamento.

Agendamento: Opção para agendar a geração de relatórios periodicamente (diário, semanal, mensal) e enviá-los por e-mail para os responsáveis.

3. Tela de Configuração e Políticas de Retenção
Esta tela é para os administradores do sistema e garante que a captura de logs esteja alinhada com as políticas da empresa e regulamentações.

Configuração de Retenção: Definir o período de armazenamento dos logs (ex: 5 anos). O sistema deve alertar o administrador quando os logs estiverem próximos da data de expiração.

Configuração de Eventos: Habilitar ou desabilitar a captura de logs para determinados tipos de eventos. Por exemplo, a capacidade de desativar o registro de eventos de baixa prioridade para otimizar o armazenamento.

Regras de Alerta: Criar regras de alerta para eventos suspeitos. Por exemplo, "Alertar quando um usuário tentar acessar uma conta mais de 5 vezes sem sucesso" ou "Alertar quando um ativo crítico for excluído".


1. Layout da Tela de Pesquisa e Visualização de Logs
O objetivo desta tela é ser um painel de controle central para a análise de logs.

Top (Barra de Ações e Filtros)

Barra de Pesquisa Global: Um campo de busca simples e proeminente para procurar por palavras-chave em todos os logs.

Filtros de Data: Um seletor de data com opções rápidas ("Últimas 24h", "Últimos 7 dias", "Mês Atual") e um seletor de calendário para datas customizadas.

Filtros Dropdown: Dropdowns para filtrar por Usuário, Tipo de Evento (Login, Criação de Ativo, Alteração de OS, etc.) e Objeto de Ação (Ativo, Ordem de Serviço, etc.).

Botão "Exportar Relatório": Um botão claro para iniciar o processo de geração de relatórios.

Área Central (Gráficos e Estatísticas)

Gráfico de Linhas/Barras: Um gráfico que mostra a atividade de logs ao longo do tempo. Isso ajuda a identificar picos de atividade ou períodos de inatividade incomuns.

Gráfico de Pizza/Donut: Um gráfico que exibe a distribuição de tipos de eventos (por exemplo, 40% das ações foram de edição, 20% de criação).

Área Inferior (Tabela de Logs)

Tabela de Dados: Uma tabela limpa e paginada que lista os logs. As colunas devem ser redimensionáveis e ordenáveis.

Colunas Sugeridas: Timestamp, Usuário, Tipo de Ação, Objeto Afetado, e uma coluna de Detalhes que pode conter um ícone para expandir e ver mais informações.

Visualização Detalhada: Ao clicar em uma linha da tabela, um painel lateral ou uma janela modal deve se abrir, mostrando todos os dados do log em um formato amigável, como uma lista de chaves e valores (ex: Campo: 'Status', Valor Anterior: 'Pendente', Novo Valor: 'Em Andamento').

2. Layout da Tela de Relatórios de Auditoria
Esta tela deve ser focada na geração e visualização de relatórios formais.

Barra Lateral de Opções

Modelos de Relatório: Uma lista de modelos pré-definidos (ex: "Relatório de Atividade de Janeiro", "Alterações no Ativo X").

Opções de Exportação: Botões para exportar o relatório nos formatos mais comuns, como PDF, CSV e JSON.

Opção de Agendamento: Um botão para configurar a recorrência do relatório (diário, semanal, mensal) e o e-mail de destino.

Área Central (Visualização do Relatório)

Visualização Prévia: Uma área que mostra uma prévia do relatório com base nos filtros e modelos selecionados. Isso permite ao usuário verificar o conteúdo antes de exportar.

Sumário do Relatório: Um breve resumo com as principais estatísticas, como o número total de eventos, o usuário mais ativo e o tipo de evento mais frequente no período.

Tabela de Dados Completa: A tabela com todos os logs que se encaixam nos critérios do relatório.

3. Layout da Tela de Configuração e Políticas
Esta tela deve ser acessível apenas a administradores e deve ser organizada para evitar erros de configuração.

Seções com Títulos Claros: Usar títulos para separar as diferentes áreas de configuração, como:

Política de Retenção: Uma seção com um campo numérico e um dropdown ("Dias", "Meses", "Anos") para definir por quanto tempo os logs serão mantidos. Deve haver um texto explicativo sobre o impacto dessa política.

Regras de Alerta: Uma tabela que lista as regras de alerta ativas. Cada linha deve ter um botão para "Editar" ou "Excluir". Acima da tabela, deve haver um botão "Adicionar Nova Regra".

Tipos de Evento: Uma lista de checkboxes ou botões de alternância (toggles) para cada tipo de evento (ex: "Logins", "Alterações de Senha", "Exclusão de Ativos"). Isso permite que os administradores ativem ou desativem a captura de logs para eventos específicos.

Botão "Salvar Alterações": Um botão bem visível no final da tela para confirmar e aplicar todas as mudanças.

O design dessas telas deve seguir as melhores práticas de UX, com cores neutras e contrastantes, ícones intuitivos e uma hierarquia visual clara, garantindo que a complexa tarefa de auditoria se torne mais simples e eficiente.

Interface Intuitiva: O layout e a navegação das telas devem ser simples, mesmo para um usuário que não é um especialista em TI. A busca e a visualização de logs devem ser tão fáceis quanto usar um motor de busca na internet.