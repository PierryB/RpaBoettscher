# RPA BOETTSCHER

## Introdução

O desenvolvimento tecnológico contínuo tem aumentado a demanda por automação de processos repetitivos em diversas áreas. A Automação de Processos Robóticos (RPA) surge como uma solução eficaz, permitindo que "robôs" de software simulem ações humanas em interfaces digitais para executar uma ampla gama de tarefas sem intervenção humana. Este projeto visa o desenvolvimento e implementação de soluções RPA para otimizar processos em ambientes corporativos.

## Objetivo

Desenvolver uma solução RPA customizada que permita aos usuários gerar relatórios atualizados da Tabela FIPE e realizar downloads de boletos em PDF diretamente do site da instituição de ensino superior Católica SC.

## Funcionalidades

- Geração de Relatório da Tabela FIPE: Robô capaz de acessar os dados mais recentes da Tabela FIPE, gerar um relatório personalizado e disponibilizá-lo como um arquivo Excel.
- Download de Boletos de Mensalidade: Automatiza o processo de login no portal da faculdade, localiza o boleto de mensalidade mais recente e o baixa em formato PDF.
- Integração Web-to-RPA: Uma API robusta que facilita a comunicação entre a página web e os robôs RPA, permitindo aos usuários acionar as automatizações desejadas com simples cliques.

## Stack Tecnológica
- Linguagens: JavaScript, C#
- Front-end: React.js / Next.js
- Back-end: Node.js / C#
- Automação: Puppeteer
- CI/CD: GitHub Actions / Vercel
- Segurança: Auth0 para autenticação, AWS IAM, HTTPS

## Arquitetura

O projeto utiliza uma máquina local para executar os RPA's. A comunicação entre a interface web e os RPA's é realizada via API RESTful. A autenticação é gerenciada pelo Auth0. Os resultados são disponibilizados para o usuário através da interface web, após o processamento no back-end.

## Próximos Passos

1) Desenvolvimento do RPA de Faturas PDF Católica ✅
2) Desenvolvimento do RPA de Histórica da Tabela Fipe ✅
3) Implementação da Interface Web e API ✅
4) Integração e teste com API e AWS ✅
5) Documentação completa e deploy final ✅

## Referências

- React.js - https://react.dev/
- Next.js - https://nextjs.org/
- Node.js - https://nodejs.org/
- Puppeteer - https://pptr.dev/
- GitHub - https://github.com/
- AWS EC2 - https://aws.amazon.com/ec2/
- Auth0 - https://auth0.com/
- SonarQube - https://sonarcloud.io/

## Repositório Front
- https://github.com/PierryB/SiteRpa
## Repositório API
- https://github.com/PierryB/ApiRpa
