# Como importar esses CSVs no Miro

## Metodo: Copiar do Excel/Google Sheets e colar no Miro

### Passo a passo:

1. Abra o arquivo CSV no Excel ou Google Sheets
2. Selecione APENAS a coluna de texto (coluna B - Texto/Regra)
3. Copie (Ctrl+C)
4. Va para o Miro, clique no board
5. Cole (Ctrl+V)
6. O Miro vai perguntar: Criar sticky notes? -> Confirme
7. Cada celula vira um sticky note separado
8. Mude a cor dos stickies conforme a legenda abaixo

## Legenda de cores

| Arquivo | Cor no Miro | Elemento |
|---------|-------------|----------|
| 01-eventos-dominio-laranja.csv | LARANJA | Eventos de Dominio |
| 02-comandos-azul.csv | AZUL | Comandos |
| 03-agregados-amarelo.csv | AMARELO (grande) | Agregados |
| 04-politicas-lilas.csv | LILAS/ROXO | Politicas/Regras |
| 05-atores-amarelo-pequeno.csv | AMARELO (pequeno) | Atores |
| 06-linguagem-ubiqua.csv | VERDE | Glossario |
| 07-bounded-contexts.csv | Cores variadas | Context Map |
| 08-timeline-fluxo-os.csv | Referencia completa | Timeline OS |
| 09-timeline-fluxo-estoque.csv | Referencia completa | Timeline Estoque |

## Ordem sugerida de montagem no Miro

1. Crie 6 FRAMES no board (secoes)
2. Importe 07 (Bounded Contexts) -> monte o Context Map com shapes
3. Importe 08 (Timeline OS) -> use como guia pra montar da esquerda pra direita
4. Importe 09 (Timeline Estoque) -> mesma logica
5. Importe 06 (Linguagem Ubiqua) -> crie uma tabela ou cards
6. Monte os agregados manualmente baseado no doc de modelagem

## Dica importante

Os arquivos 08 e 09 (timelines) sao REFERENCIAS COMPLETAS.
Use-os como guia para posicionar os sticky notes na ordem correta.
Cada linha tem: Ator -> Comando -> Evento -> Agregado -> Politica

Na pratica, no Miro voce vai:
- Colar os EVENTOS (laranja) em linha horizontal
- Colocar os COMANDOS (azul) ACIMA de cada evento correspondente
- Colocar os AGREGADOS (amarelo) ABAIXO
- Colocar as POLITICAS (lilas) ENTRE eventos que se conectam
- Colocar os ATORES (amarelo pequeno) ACIMA dos comandos

## Separador dos CSVs

Os CSVs usam ponto-e-virgula (;) como separador.
Se o Excel nao separar automaticamente, use Dados -> Texto para Colunas -> Delimitado -> Ponto e virgula
