# REPO — Tradução pt-BR

Tradução em **português (do Brasil)** para [R.E.P.O.](https://store.steampowered.com/app/3241660/REPO/) (`The Retrieve, Extract and Profit Operation` => `A Operação de Recuperação, Extração e Lucro`) — interface do jogo, cosméticos vanilla/modded e alguns mods — com liga/desliga por categoria no jogo.

- **Página do mod (Thunderstore):** _em breve_
- **Como funciona, categorias e personalização:** veja o [README do pacote](package/README.md) (o mesmo exibido na Thunderstore).

## Estrutura do repositório

```
src/        Código do plugin BepInEx (configura o XUnity.AutoTranslator, aplica a
            localização nativa, sincroniza os dicionários e expõe os toggles).
package/    Esqueleto do pacote Thunderstore: manifest.json, README, icon e
            plugins/ com os dicionários (.txt) e a localização nativa (.tsv).
```

Os textos da tradução ficam em:
- `package/plugins/Dictionaries/*.txt` — dicionários do XUnity.AutoTranslator
  (formato `Original=Tradução`, um arquivo por categoria/toggle).
- `package/plugins/Localizations/{Menu,HUD,Game}.tsv` — tabelas de localização
  nativa do jogo (formato `CHAVE<TAB>texto`; os nomes dos arquivos e as chaves
  precisam bater com as StringTables do jogo).

## Build

Requisitos: .NET SDK e os assemblies do jogo/BepInEx (não são incluídos no repo).
Copie `Directory.Build.props.example` para `Directory.Build.props` e ajuste os
caminhos da sua máquina (ou defina as variáveis de ambiente `REPO_MANAGED_PATH`
e `BEPINEX_CORE_PATH`). Depois:

```
dotnet build src/RepoTraducaoPTBR.csproj
```

O build copia a DLL para `package/plugins/` (e, se existir, para um profile de
teste local — configurável pela propriedade MSBuild `TestProfileDir`).

Para publicar na Thunderstore: zipar o **conteúdo** de `package/` (manifest.json,
README.md, icon.png e a pasta plugins/).

## Licença

[MIT](LICENSE). Os textos traduzidos também — use à vontade, com crédito.
