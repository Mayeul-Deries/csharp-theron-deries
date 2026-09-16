# Échanges décisifs avec l’IA

## Date et sujet

- Outil / modèle si connu :
- Contexte :
- Prompt réellement utilisé :
- Réponse et hypothèses résumées :
- Décision et justification :
- Scénario ou commande de vérification :
- Résultat attendu, puis résultat observé :
- Erreur que ce contrôle pourrait détecter :
- Preuves reproductibles et limites :

## 2026-09-16 — Première intégration frontend du contrat de jeu

- Outil / modèle si connu : GitHub Copilot CLI, gpt-5.6-luna.
- Contexte : le backend n'est pas encore implémenté, mais Boris a défini les routes REST, l'URL locale et le flux de tour. Le frontend Blazor doit pouvoir être construit indépendamment.
- Prompt réellement utilisé : « On peut commencer le frontend avec le contrat backend prévu ; respecter les DTO partagés, les routes REST, le rafraîchissement après un tir et préparer l'intégration gRPC de l'IA. »
- Réponse et hypothèses résumées : créer un client HTTP typé, conserver les DTO partagés, utiliser une réponse locale minimale pour `{ "result": ... }`, afficher les deux grilles et ne pas simuler l'appel gRPC tant que `game.proto` n'est pas disponible.
- Décision et justification : intégration HTTP acceptée ; l'appel gRPC reste une frontière explicite pour éviter de fabriquer un contrat Protobuf non validé.
- Scénario ou commande de vérification : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj --no-restore`.
- Résultat attendu, puis résultat observé : compilation de `BattleShip.App` et réussite des tests ; 22 tests réussis.
- Erreur que ce contrôle pourrait détecter : DTO ou composant Razor incompatibles, erreur de compilation, régression des tests de `BattleGrid`.
- Preuves reproductibles et limites : commit `feat(frontend): integrate game REST contract`; aucune vérification navigateur ni échange réel avec l'API, car les routes backend ne sont pas encore présentes.

## 2026-09-16 — Vérification isolée du client REST

- Outil / modèle si connu : GitHub Copilot CLI, gpt-5.6-luna.
- Contexte : le client REST frontend doit être vérifié sans attendre le serveur backend.
- Prompt réellement utilisé : « Ajouter des tests du client REST avec un faux `HttpMessageHandler` pour vérifier les routes, les corps JSON, les réponses et les erreurs HTTP. »
- Réponse et hypothèses résumées : quatre scénarios xUnit couvrent la création, la lecture d'une partie, un tir et les statuts HTTP `400`, `404` et `500`.
- Décision et justification : proposition acceptée ; le faux handler rend les tests déterministes et prouve le contrat émis par le navigateur sans dépendance réseau.
- Scénario ou commande de vérification : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj --no-restore`.
- Résultat attendu, puis résultat observé : 28 tests réussis, dont les tests du client REST.
- Erreur que ce contrôle pourrait détecter : route mal construite, mauvais verbe HTTP, coordonnées mal sérialisées, réponse illisible ou perte du code HTTP.
- Preuves reproductibles et limites : fichier `BattleShip.Tests/Frontend/GameApiClientTests.cs`; le test ne prouve pas le comportement réel du serveur ni CORS.
