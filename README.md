# SQL Query Builder — template

Projeto inicial em **ASP.NET Core Web API (.NET 10)** com **PostgreSQL 17**, preparado para um frontend em HTML, CSS e JavaScript puro, conforme `especificacao.md`.

O projeto ainda não implementa o gerador de SQL nem possui endpoints. A pasta `SQLQueryBuilder.Api` contém a configuração da API, o registro de `NpgsqlDataSource` para acesso ao PostgreSQL e uma política de CORS para desenvolvimento local.

## Requisitos

- Docker com Docker Compose.

## Iniciar

Na raiz do projeto, execute:

```sh
docker compose up --build
```

Esse comando compila e inicia a API em `http://localhost:8080` e o PostgreSQL na porta `5432`. O Compose aguarda o banco ficar pronto antes de iniciar a API. O banco começa vazio; nenhuma tabela ou dado de exemplo é criado pelo template.

Uma resposta **404** ao acessar a raiz da API é esperada, pois não há endpoints ainda.

Para encerrar, use `Ctrl+C`. Para remover os contêineres sem apagar os dados, execute `docker compose down`. Os dados ficam no volume `postgres_data`.

## Configuração local

Os valores padrão de desenvolvimento já permitem iniciar com um comando. Se quiser alterar portas ou credenciais, copie `.env.example` para `.env` e edite os valores antes de subir os contêineres. A variável `ConnectionStrings__DefaultConnection` é montada pelo Compose com esses mesmos valores.

O frontend pode acessar a API a partir das origens listadas em `SQLQueryBuilder.Api/appsettings.json` (`localhost` nas portas 3000, 5173 e 5500; `127.0.0.1` na porta 5500). Para outra origem, ajuste `Cors:AllowedOrigins` nesse arquivo antes de reconstruir a imagem. Sirva os arquivos JS por HTTP; abrir o HTML diretamente com `file://` não corresponde a essas origens.

## Estrutura

```text
SQLQueryBuilder.Api/
  Program.cs           Configuração da API, PostgreSQL e CORS
  appsettings.json     Configurações locais
  SQLQueryBuilder.Api.csproj
SQLQueryBuilder.sln      Solução para build pela CLI ou IDE
Dockerfile             Build e execução da API
compose.yaml           API e PostgreSQL
```

Quando implementar a solução, adicione controllers à API para receber a seleção de colunas, condições e ordenação descritas em `especificacao.md`. O exemplo de entrada em `ideias.txt` também inclui um limite opcional.
