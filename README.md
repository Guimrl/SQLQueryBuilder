# SQL Query Builder

Gerador de consultas `SELECT` para Banco de Dados. Escolha uma tabela, selecione colunas, adicione condições e ordenação e copie o SQL gerado. A aplicação **não executa** a consulta.

## Instalação

Requisitos: Docker

1. Na raiz do projeto, execute `docker compose up --build`.
2. Abra **http://localhost:8080** no navegador.
3. Selecione uma tabela para montar a consulta.

Para encerrar, use `Ctrl+C`. `docker compose down` remove os contêineres e preserva os dados no volume `postgres_data`.

## Usar a interface

1. **Origem:** selecione uma tabela. A aplicação busca as colunas disponíveis automaticamente.
2. **Colunas:** mantenha “Todas as colunas” para `SELECT *` ou desmarque a opção e escolha uma ou mais colunas.
3. **Condições:** adicione filtros. Os operadores oferecidos dependem do tipo da coluna. Vários filtros são combinados com `AND`. Para `IS NULL` e `IS NOT NULL`, nenhum valor é necessário.
4. **Ordenação:** adicione uma ou mais colunas. A ordem das linhas na interface define a prioridade do `ORDER BY`.
5. Clique em **Gerar consulta SQL**, confira o resultado e use **Copiar SQL**.

Exemplo de resultado:

```sql
SELECT "id", "nome" FROM "clientes" WHERE "nome" ILIKE '%Ana%' AND "id" > 10 ORDER BY "nome" ASC;
```

## API

| Endpoint                              | Função                                                 |
| ------------------------------------- | ------------------------------------------------------ |
| `GET /api/Tables`                     | Lista os nomes das tabelas públicas em `name`.         |
| `GET /api/Columns?tableName=clientes` | Lista `columns`, cada uma com `name` e `type`.         |
| `POST /api/Queries/Generate`          | Recebe as escolhas e retorna o SQL como string JSON.   |
| `POST /api/Database/Import`           | Importa um arquivo de dump PostgreSQL (Compose local). |

Exemplo de requisição:

```json
{
  "table": "clientes",
  "columns": ["id", "nome"],
  "where": [
    { "column": "nome", "operator": "contains", "value": "Ana" },
    { "column": "id", "operator": "greaterThan", "value": 10 }
  ],
  "orderBy": [{ "column": "nome", "direction": "ASC" }]
}
```

## Configuração

As portas padrão são `8080` para a aplicação e `5432` para o PostgreSQL. Copie `.env.example` para `.env` se precisar alterar portas ou credenciais. O Compose passa a conexão do banco à API via `ConnectionStrings__DefaultConnection`.

O frontend é servido pela própria API, então não precisa de servidor separado nem configuração de CORS. As origens de desenvolvimento externo permitidas ficam em [`appsettings.json`](src/SQLQueryBuilder.WebApi/appsettings.json).

## Estrutura

```text
src/SQLQueryBuilder.WebApi/wwwroot/  Frontend em HTML, CSS e JavaScript
src/SQLQueryBuilder.WebApi/          Endpoints e hospedagem da interface
src/SQLQueryBuilder.Application/     Validação e geração de SQL
src/SQLQueryBuilder.Infra/           Consulta aos metadados do PostgreSQL
examples/demo.sql                   Tabela de exemplo
compose.yaml                        API e PostgreSQL
```

