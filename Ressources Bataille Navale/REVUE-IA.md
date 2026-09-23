# Revues de propositions IA

## Revue : Stratégie de recherche et de chasse de l'IA adverse

**Proposition examinée**  
Implémentation d'une stratégie de recherche en damier (checkerboard) combinée à une stratégie de chasse sur cases adjacentes dans `AiOpponentService.cs`.

**Hypothèse à vérifier**  
L'IA adverse doit être plus performante et tactique qu'un générateur de tirs purement aléatoire, tout en garantissant des coordonnées strictement valides et non déjà ciblées.

**Expérience**  
1. Commande : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj --filter FullyQualifiedName~AITests`.
2. Scénario : Positionner un navire sur la grille et mesurer le nombre de tirs nécessaires pour le couler après le premier impact.
3. Contrôle : Vérifier que l'IA ne génère jamais de tir hors bornes `[0, 9]` et ne répète jamais une coordonnée déjà frappée.

**Observation**  
Le code implémenté dans `AiOpponentService.cs` réduit l'espace de recherche en ciblant prioritairement les cases où `(row + col) % 2 == 0`, puis bascule instantanément en mode chasse en sondant les cases cardinales adjacentes dès qu'une case est touchée. Les tests xUnit dans `AITests.cs` passent à 100 %.

**Décision et justification**  
Acceptée. Élève substantiellement la qualité et l'ambition du jeu par rapport à un simple bot aléatoire, tout en restant robuste et sans dépendances externes.

**Preuves et limites**  
- Commit `132b59b`, `BattleShip.Models/AI/AiOpponentService.cs`, `BattleShip.Tests/AITests.cs`.
- Limite : L'IA ne déduit pas encore l'orientation précise (axe horizontal/vertical) après 2 touches consécutives, ce qui constitue une piste d'amélioration ultérieure.

---

## Revue : Architecture Hybride de communication (REST + gRPC-Web)

**Proposition examinée**  
Utiliser une architecture mixte : endpoints Minimal APIs REST pour la gestion de l'état de jeu (`GET /games/{id}`, `POST /games`) et gRPC-Web pour les actions de tir à faible latence (`TakeOpponentShot`).

**Hypothèse à vérifier**  
La cohabitation de REST et de gRPC-Web dans le même pipeline ASP.NET Core (.NET 10) est fluide et ne provoque aucun conflit de routage ni de blocage CORS dans Blazor WebAssembly.

**Expérience**  
1. Commande : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`.
2. Vérification réseau : Lancer l'API et le client Blazor, exécuter une séquence complète de création de partie via REST suivie d'une riposte gRPC-Web.
3. Contrôle des en-têtes CORS : Vérifier la présence des en-têtes `Grpc-Status`, `Grpc-Message` et l'autorisation de l'origine `http://localhost:5270`.

**Observation**  
Le middleware `app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true })` permet aux requêtes gRPC-Web d'atteindre `GameGrpcService` sans proxy externe, tandis que les Minimal APIs répondent immédiatement aux requêtes HTTP JSON standards.

**Décision et justification**  
Acceptée. Formalisée et détaillée dans l'ADR 0004.

**Preuves et limites**  
- Commits `132b59b`, `1f6db62`, `ca3a7a0`.
- Fichiers `BattleShip.API/Program.cs`, `BattleShip.API/Grpc/GameGrpcService.cs`, `docs/adr/0004-architecture-hybride.md`.

---

## Revue : Sécurité du masquage des informations et anti-triche

**Proposition examinée**  
Utilisation de la méthode de projection `GamePrivacyMapper.ToStatusDto` pour masquer strictement les navires adverses non découverts avant d'envoyer le statut de la partie au client.

**Hypothèse à vérifier**  
Le client WebAssembly ne doit en aucun cas recevoir les coordonnées réelles des navires ennemis vivants dans la charge utile JSON de `GET /games/{gameId}`, empêchant toute triche par inspection du DOM ou des paquets réseau.

**Expérience**  
1. Commande : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj --filter FullyQualifiedName~PrivacyTests`.
2. Résultat attendu : Les cellules de l'adversaire contenant des navires intacts doivent être converties en `CellState.Empty` et leur type de navire doit être omis (`ShipType = null`).

**Observation**  
Tous les tests de `PrivacyTests.cs` sont verts. Seuls les navires coulés ou les cases touchées révèlent leur statut au joueur.

**Décision et justification**  
Acceptée. Respecte scrupuleusement le principe de moindre privilège et les exigences de sécurité du sujet.

**Preuves et limites**  
- Commit `c3a621e`, `BattleShip.Models/Privacy/GamePrivacyMapper.cs`, `BattleShip.Tests/PrivacyTests.cs`.

---

## Revue : Client HTTP frontend typé basé sur le contrat REST

**Proposition examinée**  
Ajouter `BattleShip.App/Services/GameApiClient.cs` et brancher `Home.razor` sur `POST /games`, `GET /games/{gameId}` et `POST /games/{gameId}/shots`.

**Hypothèse à vérifier**  
Le frontend Blazor peut manipuler les DTO partagés et appeler les routes prévues avec une gestion propre des erreurs HTTP (`400`, `404`, `500`).

**Expérience**  
Commande : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`. Résultat attendu : compilation de Blazor et passage des tests unitaires de service.

**Observation**  
La compilation a réussi et l'ensemble des tests du client REST passent. La séparation claire permet de découpler les composants Razor de la couche transport.

**Décision et justification**  
Acceptée. Formalisée dans l'ADR 0002.

**Preuves et limites**  
- Commit `38207c5`, `BattleShip.App/Services/GameApiClient.cs`, `BattleShip.App/Pages/Home.razor`, `docs/adr/0002-client-http-frontend.md`.

---

## Revue : RPC dédiée au tour de l'IA (TakeOpponentShot)

**Proposition examinée**  
Ajouter la méthode `rpc TakeOpponentShot (OpponentShotRequest) returns (OpponentShotResponse);` dans `Protos/game.proto` et l'appeler depuis `OpponentGrpcClient` lors du tour de l'ordinateur.

**Hypothèse à vérifier**  
Une RPC dédiée évite toute ambiguïté sur l'initiateur du tir par rapport à `TakeShot` (qui représente le tir du joueur humain), tout en fournissant les coordonnées frappées pour la synchronisation visuelle.

**Expérience**  
Commande : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`. Résultat attendu : génération du stub C# Protobuf, compilation de l'API et du frontend Blazor, maintien de la suite de tests au vert.

**Observation**  
Le contrat gRPC est compilé de manière bidirectionnelle (client dans `BattleShip.App`, serveur dans `BattleShip.API`). L'appel gRPC-Web renvoie le résultat et les coordonnées `(Row, Col)` calculées par l'IA.

**Décision et justification**  
Acceptée. Validée côté frontend (commit `1ed630a`) et côté backend (commit `132b59b`).

**Preuves et limites**  
- `Protos/game.proto`, `BattleShip.API/Grpc/GameGrpcService.cs`, `BattleShip.App/Services/OpponentGrpcClient.cs`, `docs/adr/0003-rpc-tir-adversaire.md`.

---

## Revue : Refonte tactique de l'UI et placement manuel de flotte

**Proposition examinée**  
Refonte complète de l'application Blazor d'après la maquette Google Stitch :
1. Thème sombre tactique HUD militaire (`BattleShip.App/wwwroot/css/app.css`).
2. Grilles radar avec repères cartographiques et élimination du layout shift (`BattleGrid.razor`).
3. Panneau de suivi des flottes alliée/adverse (`FleetStatusPanel.razor`) et journal chronologique des tirs (`ShotLogPanel.razor`).
4. Écran interactif de déploiement manuel de flotte avec rotation, survol dynamique cyan/rouge, placement aléatoire et réinitialisation.

**Hypothèse à vérifier**  
1. L'interface offre un niveau de finition et d'ergonomie irréprochable sans latence ni instabilité visuelle.
2. La validation serveur (`CreateGameRequestValidator`) rejette tout placement invalide (chevauchement, hors grille, taille incorrecte).
3. Les 54 tests unitaires et bUnit passent au vert.

**Expérience**  
1. Tests automatisés : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`.
2. Test interactif dans le navigateur avec `browser_subagent` : simulation d'un placement manuel complet, bascule d'orientation, combat en direct et détection de victoire/défaite.

**Observation**  
54 tests réussis sur 54. Le design HUD réactif est parfaitement fluide et l'ensemble des règles métier de déploiement sont respectées.

**Décision et justification**  
Acceptée. Apporte une réelle plus-value d'ambition et d'ergonomie au projet.

**Preuves et limites**  
- Commits `12e4daa`, `92c0be1`, `dfdd749`, `1911ae3`.
- `BattleShip.App/Pages/Home.razor`, `BattleShip.App/Components/`, `BattleShip.Tests/Frontend/`.
- Enregistrements vidéo : `tactical_ui_test_1789638490429.webp`, `fleet_placement_battle_test_1789654820026.webp`.

---

## Revue : Résolution du conflit d'alias d'énumération ShipType (Submarine vs Destroyer)

**Proposition examinée**  
1. Attribution de valeurs entières uniques aux membres de l'énumération `BattleShip.Models/Enums/ShipType.cs` (`Carrier = 1`, `Battleship = 2`, `Destroyer = 3`, `Submarine = 4`, `TorpedoBoat = 5`).
2. Découplage de la longueur du navire de sa valeur ordinale via `Ship.GetLength(ShipType type)`.
3. Sécurisation de l'état du dock de navires dans `Home.razor`.

**Hypothèse à vérifier**  
En C#, deux membres d'une énumération partageant la même valeur (`Destroyer = 3` et `Submarine = 3`) sont considérés comme identiques par le runtime (`Destroyer == Submarine` vaut `true`). Cette collision provoquait le marquage erroné des deux navires comme « Placé » dès que l'un d'eux était posé. Des valeurs uniques résolvent ce bug tout en conservant leur longueur respective de 3 cases.

**Expérience**  
1. Tests unitaires : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`.
2. Tests de non-régression ajoutés : `GameEngine_TrySetupPlayerShips_ShouldRejectTwoSubmarinesInsteadOfDestroyer` (`EngineTests.cs`) et `PlacingSubmarine_ShouldNotMarkDestroyerAsPlaced` (`HomeTests.cs`).
3. Vérification navigateur : Placer un Sous-marin et vérifier que seul le Sous-marin passe à `✓ Placé` tandis que le Croiseur reste `À placer`.

**Observation**  
56 tests réussis sur 56. Le bug de sélection croisée est définitivement corrigé sans aucun effet de bord sur le reste du domaine.

**Décision et justification**  
Acceptée. Solution canonique et pérenne respectant les principes SOLID.

**Preuves et limites**  
- Commit `02f9bd4`, `ShipType.cs`, `Ship.cs`, `GameEngine.cs`, `Home.razor`, `EngineTests.cs`, `HomeTests.cs`.
- Vidéo de vérification : `submarine_destroyer_fix_test_1789657015807.webp`.
