# Échanges décisifs avec l'IA

## 2026-09-16 — Intégration frontend du contrat REST

**Outil / modèle** : GitHub Copilot CLI, gpt-5.6-luna.

**Contexte** : le backend n'est pas encore implémenté, mais le binôme a défini les routes REST, l'URL locale et le flux de tour. Le frontend Blazor doit avancer sans modifier les DTO partagés.

**Prompt** : « On peut commencer le frontend avec le contrat backend prévu ; respecter les DTO partagés, les routes REST, le rafraîchissement après un tir et préparer l'intégration gRPC de l'IA. »

**Réponse résumée** : créer un client HTTP typé, afficher les deux grilles, envoyer les tirs joueur et relire le statut après chaque tir. Ne pas simuler le gRPC tant que son contrat n'est pas disponible.

**Décision** : acceptée. Cette séparation permet de construire et tester le frontend indépendamment du serveur, tout en conservant une frontière claire pour l'IA.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj --no-restore`. Résultat attendu : compilation de l'application et tests verts. Résultat observé : 22 tests réussis. Ce contrôle ne vérifie ni l'API réelle ni CORS.

**Preuve** : commit `38207c5 feat(frontend): integrate game REST contract`, `BattleShip.App/Services/GameApiClient.cs`, `BattleShip.App/Pages/Home.razor` et ADR 0002.

## 2026-09-16 — RPC dédiée au tour de l'IA

**Outil / modèle** : GitHub Copilot CLI, gpt-5.6-luna.

**Contexte** : le contrat fourni par le binôme décrit `GameService.TakeShot` comme un tir joueur. Il ne permet pas de déclencher le tir de l'ordinateur après un `Miss`, alors que la partie contre l'IA est obligatoire.

**Prompt** : « Ajouter une RPC dédiée `TakeOpponentShot`, générer le client Blazor gRPC-Web et l'appeler après un `Miss`, sans simuler le serveur. »

**Réponse résumée** : conserver `TakeShot` pour le joueur et ajouter `TakeOpponentShot`, avec `game_id` en entrée et `result`, `row`, `col` en sortie. Après l'appel, le frontend relit `GET /games/{gameId}`.

**Décision** : adaptée puis acceptée côté frontend. Une RPC dédiée évite d'ambiguïser l'acteur du tir. Boris doit encore implémenter cette RPC côté API.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`. Résultat attendu : génération protobuf, compilation et tests verts. Résultat observé : 28 tests réussis. Ce contrôle ne vérifie pas l'échange réseau réel.

**Preuve** : `Protos/game.proto`, `BattleShip.App/Services/OpponentGrpcClient.cs`, ADR 0003 et le commit à créer `feat(frontend): integrate opponent grpc client`.
