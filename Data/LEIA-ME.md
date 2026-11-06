# Dados do site

Os arquivos desta pasta alimentam o site. Para atualizar um exame, uma unidade ou um convênio,
edite o JSON e publique de novo. Não é preciso mexer em código.

| Arquivo | Conteúdo |
|---|---|
| `exames.json` | Catálogo de exames (nome, código, material, método, preparo, prazo, categoria) |
| `unidades.json` | Unidades de atendimento, endereços, horários e facilidades |
| `convenios.json` | Convênios aceitos e caminho do logotipo em `wwwroot/img/convenios` |

> **Atenção:** os 10 exames de `exames.json` vieram dos dados de exemplo do site antigo.
> Antes de publicar, o preparo, o método e o prazo de cada exame precisam ser conferidos
> pelo responsável técnico do laboratório.

Valores aceitos:

- `categoria`: `Sangue`, `Urina`, `Fezes`, `Imagem`
- `tipoPreparo`: `SemPreparo`, `Jejum`, `Especial`
