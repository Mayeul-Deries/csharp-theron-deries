# Échanges décisifs avec l'IA

## 2026-09-15 — Modélisation du domaine métier et tests unitaires

**Outil / modèle** : GitHub Copilot CLI, Claude 3.5 Sonnet.

**Contexte** : Initialisation du projet. Le moteur de jeu de la bataille navale doit gérer deux grilles de 10x10, le placement de 5 types de navires canoniques, l'intégrité sans débordement ni chevauchement, l'historique des tirs et les états de victoire, le tout isolable de tout transport web.

**Prompt** : « Conçois le modèle de domaine en C# 10 dans BattleShip.Models avec des entités et records immuables (Coordinate, Ship, Grid, GameEngine), et écris une suite de tests unitaires xUnit complète validant tous les invariants. »

**Réponse résumée** : Création d'un domaine riche : `Coordinate` (borné [0,9]), `Ship` (calcul géométrique des cases selon l'orientation et la longueur propre), `Grid` (gestion de collection et vérification d'invariants), et `GameEngine` (états de partie `NotStarted`, `PlayerTurn`, `OpponentTurn`, `PlayerWon`, `OpponentWon`).

**Décision** : Acceptée. Offre une base robuste, découplée du web et testable unitairement avec xUnit.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`. Résultat attendu : 18 tests réussis validant les règles métier fondamentales. Résultat observé : 18 tests verts.

**Preuve** : Commit `4a975f3 1st Commit : Models and Tests`, `BattleShip.Models/Domain/`, `BattleShip.Tests/EngineTests.cs` et ADR 0001.

---

## 2026-09-16 — Architecture Hybride REST + gRPC-Web & Configuration CORS

**Outil / modèle** : Google Antigravity, Gemini.

**Contexte** : Le sujet demande une communication fluide entre Blazor WebAssembly et l'API .NET 10, combinant des opérations REST de gestion d'état et un échange binaire gRPC-Web pour l'action de tir.

**Prompt** : « Nous voulons une architecture hybride : REST pour le statut (/games), gRPC-Web pour le tir. Comment configurer le middleware gRPC-Web, l'exposition des headers gRPC et la politique CORS pour que Blazor WebAssembly puisse appeler les deux sans blocage ? »

**Réponse résumée** : Configuration de `builder.Services.AddGrpc()`, `app.UseCors(...)` avec exposition explicite des en-têtes `Grpc-Status`, `Grpc-Message`, `Grpc-Encoding`, `Grpc-Accept-Encoding`, `app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true })`, et mapping de service `app.MapGrpcService<GameGrpcService>().EnableGrpcWeb()`.

**Décision** : Acceptée. Permet la coexistence transparente de Minimal APIs REST et d'endpoints gRPC-Web sur le même port HTTP.

**Vérification** : `dotnet build` au vert, validation manuelle via `api.http` et tests d'intégration.

**Preuve** : Commit `132b59b`, `1f6db62`, `BattleShip.API/Program.cs` et ADR 0004.

---

## 2026-09-16 — Stratégie avancée de l'IA adverse (Checkerboard & Chasse)

**Outil / modèle** : Google Antigravity, Gemini.

**Contexte** : Un tir purement aléatoire de l'adversaire nuit à l'intérêt du jeu et ne respecte pas les critères d'ambition du barème.

**Prompt** : « Comment rendre l'IA adverse plus tactique ? Implémente une stratégie en damier (checkerboard) pour la phase de recherche et une logique de chasse ciblant les cases adjacentes après un tir touché. »

**Réponse résumée** : Création de `AiOpponentService` implémentant une machine à états de ciblage : en mode chasse (s'il existe des cases touchées dont le navire n'est pas coulé), l'IA sonde les 4 cases cardinales voisines valides ; en mode recherche, elle filtre les cases selon la parité `(row + col) % 2 == 0`.

**Décision** : Acceptée. Augmente significativement le niveau tactique de l'IA tout en garantissant un calcul déterministe et rapide.

**Vérification** : Tests unitaires dans `BattleShip.Tests/AITests.cs` simulant la traque d'un navire touché. Résultat : l'IA trouve et coule le navire en un nombre de coups réduit.

**Preuve** : Commit `132b59b`, `BattleShip.Models/AI/AiOpponentService.cs`, `BattleShip.Tests/AITests.cs`.

---

## 2026-09-16 — Intégration frontend du contrat REST

**Outil / modèle** : GitHub Copilot CLI, gpt-5.6-luna.

**Contexte** : Le backend n'est pas encore finalisé par Boris, mais le binôme a défini les routes REST, l'URL locale et le flux de tour. Le frontend Blazor doit avancer sans modifier les DTO partagés.

**Prompt** : « On peut commencer le frontend avec le contrat backend prévu ; respecter les DTO partagés, les routes REST, le rafraîchissement après un tir et préparer l'intégration gRPC de l'IA. »

**Réponse résumée** : Créer un client HTTP typé (`GameApiClient`), afficher les deux grilles, envoyer les tirs joueur et relire le statut après chaque tir. Ne pas simuler le gRPC tant que son contrat n'est pas disponible.

**Décision** : Acceptée. Cette séparation permet de construire et tester le frontend indépendamment du serveur, tout en conservant une frontière claire pour l'IA.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj --no-restore`. Résultat attendu : compilation de l'application et tests verts. Résultat observé : 22 tests réussis.

**Preuve** : Commit `38207c5 feat(frontend): integrate game REST contract`, `BattleShip.App/Services/GameApiClient.cs`, `BattleShip.App/Pages/Home.razor` et ADR 0002.

---

## 2026-09-16 — RPC dédiée au tour de l'IA (TakeOpponentShot)

**Outil / modèle** : GitHub Copilot CLI, gpt-5.6-luna.

**Contexte** : Le contrat initial décrivait `GameService.TakeShot` uniquement pour le tir joueur et ne déclenchait pas le tour de l'IA. Le frontend doit pourtant pouvoir exécuter une partie complète contre l'ordinateur après un tir manqué.

**Prompt** : « Ajouter une RPC dédiée `TakeOpponentShot`, générer le client Blazor gRPC-Web et l'appeler après un `Miss`, sans simuler le serveur. »

**Réponse résumée** : Conserver `TakeShot` pour le joueur et ajouter `TakeOpponentShot` dans `Protos/game.proto` (avec `game_id` en entrée et `result`, `row`, `col` en sortie). Après l'appel gRPC-Web, le frontend relit `GET /games/{gameId}` pour synchroniser l'affichage.

**Décision** : Acceptée. Une RPC dédiée évite d'ambiguïser l'acteur du tir et maintient une séparation claire des responsabilités.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`. Résultat attendu : génération protobuf, compilation Blazor et maintien des tests verts. Résultat observé : 28 tests réussis.

**Preuve** : Commits `0e0f6e8` et `1ed630a feat(frontend): integrate opponent grpc client`, `Protos/game.proto`, `BattleShip.App/Services/OpponentGrpcClient.cs` et ADR 0003.

---

## 2026-09-17 — Refonte tactique de l'UI d'après maquette Google Stitch

**Outil / modèle** : Antigravity, Gemini.

**Contexte** : Le skin initial de l'application Blazor est le template par défaut. Une maquette générée sur Google Stitch présente une interface de commandement naval sombre (Radar Ennemi, Votre Flotte avec intégrité, État de la flotte Alliés vs Ennemis, et Journal des tirs).

**Prompt** : « Ok niquel. Bon actuellement mon jeu est très moche, c'est le skin de base de blazor et j'ai envie de l'améliorer. J'ai demandé à google stitch de générer une maquette que je te transmets en photo, et je fais appel à toi pour l'implémenter car tu es bon en frontend. Si ca te convient tu peux démarrer. »

**Réponse résumée** : Établir un plan d'architecture respectant les contraintes du cours (anti-triche, contrats DTO, bUnit), enrichir `CellDto` avec `ShipType` optionnel (sans fuite d'information adverse), créer un design system tactique CSS complet (`app.css`), ajouter les composants `FleetStatusPanel` et `ShotLogPanel`, implémenter le ciblage et valider avec 49 tests unitaires et bUnit au vert.

**Décision** : Acceptée. La refonte transforme radicalement l'attrait visuel et la finition du projet tout en garantissant la maintenabilité et le respect strict des règles métier.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`. Résultat attendu : 49 tests passés avec succès. Test interactif dans le navigateur avec `browser_subagent` validant la séquence complète d'engagement, la riposte gRPC de l'IA et le journal.

**Preuve** : Commits `c3a621e`, `12e4daa`, enregistrement vidéo navigateur `tactical_ui_test_1789638490429.webp`, et revue dans `REVUE-IA.md`.

---

## 2026-09-17 — Validation directe du tir au clic et stabilisation visuelle

**Outil / modèle** : Antigravity, Gemini.

**Contexte** : Après la mise en place du skin tactique, le joueur demande de supprimer l'étape de sélection préalable / bouton de confirmation pour valider le tir immédiatement au clic, et de corriger l'effet de zoom/dézoom oscillant provoqué par l'animation CSS `reticle-pulse`.

**Prompt** : « Je ne veux pas de confirmation lors du clic d'une case, je veux que ca valide directe »

**Réponse résumée** : Retrait du bouton de confirmation et de l'état intermédiaire dans `Home.razor`, déclenchement immédiat de `FireShotAsync` au clic, élimination de `animation: reticle-pulse` et des crochets textuels `[ ⊙ ]` pour garantir un layout shift nul.

**Décision** : Acceptée. Améliore fortement la réactivité du gameplay.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`. Enregistrement de session navigateur `direct_shot_test_1789647690777.webp`.

**Preuve** : Commit `92c0be1`, tests bUnit validant le déclenchement immédiat.

---

## 2026-09-17 — Placement manuel des navires par le joueur au début de partie

**Outil / modèle** : Antigravity, Gemini.

**Contexte** : Permettre au joueur de positionner manuellement ses 5 navires au lancement du jeu ou au redémarrage, avec choix d'orientation, prévisualisation interactive, dock de flotte, options aléatoires rapides et validation d'intégrité côté serveur.

**Prompt** : « il faudrait faire en sorte que le joueur puisse placer ses bateaux au début de la partie »

**Réponse résumée** : Modélisation de `CreateGameRequest` et `ShipPlacementDto` avec sérialisation enum `JsonStringEnumConverter`. Ajout de `TrySetupPlayerShips` dans `GameEngine` et validateur FluentValidation `CreateGameRequestValidator`. Écran interactif de déploiement dans `Home.razor` avec survol en cyan/rouge, toggle d'orientation, placement aléatoire et réinitialisation. Couverture de tests étendue à 54 tests au vert.

**Décision** : Acceptée. Offre une expérience stratégique complète au joueur avant le combat.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj` (54 réussis). Enregistrement vidéo navigateur `fleet_placement_battle_test_1789654820026.webp` et capture `final_game_state_1789655157917.png`.

**Preuve** : Commits `dfdd749` et `1911ae3`, `CreateGameRequest.cs`, `CreateGameRequestValidator.cs`, `GameEngine.cs`, `Home.razor`.

---

## 2026-09-17 — Résolution de la collision d'alias d'énumération entre Sous-marin et Croiseur

**Outil / modèle** : Antigravity, Gemini.

**Contexte** : Lors du placement manuel de la flotte, placer un Sous-marin entraînait l'affichage simultané du Sous-marin et du Croiseur (Destroyer) comme « Placé » dans le dock, avec un compteur erroné.

**Prompt** : « quand je place un sous-marin ca m'en place un mais ca note les 2 comme placés, sauf qu'en réalité j'en ai placé qu'un sur deux »

**Réponse résumée** : Identification de la cause racine dans l'énumération C# `ShipType` où `Destroyer = 3` et `Submarine = 3` partageaient la même valeur entière sous-jacente, créant un alias indistinguable à l'exécution pour l'opérateur d'égalité, les collections de hachage et la réflexion. Attribution de valeurs entières distinctes uniques (`Carrier = 1`, `Battleship = 2`, `Destroyer = 3`, `Submarine = 4`, `TorpedoBoat = 5`), dissociation de la taille en cases via la méthode `Ship.GetLength()`, sécurisation de l'isolation du dock dans `Home.razor`, ajout de tests unitaires et bUnit (56 tests au total, 100% verts).

**Décision** : Acceptée. Règle définitivement la collision d'alias tout en conservant les dimensions des navires (3 cases pour chacun) et le respect des règles métier.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj` (56/56 réussis). Test interactif dans le navigateur avec enregistrement vidéo confirmant l'indépendance stricte du Sous-marin et du Croiseur.

**Preuve** : Commit `02f9bd4`, `ShipType.cs`, `Ship.cs`, `GameEngine.cs`, `Home.razor`, `EngineTests.cs`, `HomeTests.cs`.

---

## 2026-09-22 — Résolution post-merge CORS/HTTPS et masquage du bandeau d'erreur Blazor

**Outil / modèle** : Antigravity, Gemini.

**Contexte** : Après le merge de la branche `feat-frontend` sur `main`, deux anomalies subsistaient : échec de redirection HTTPS en mode développement HTTP pur (`http://localhost:5282`) bloquant les appels du frontend (`http://localhost:5270`), et affichage permanent de la bannière `#blazor-error-ui` en bas de page dès le chargement initial.

**Prompt** : « Tout semble bien fonctionner mais juste j'ai une erreur "An unhandled error has occurred. [Reload]" en bas de la page ou pendant le chargement mais ca n'empeche pas de fonctionner en soit juste c'est un peu chiant et ca fait tache »

**Réponse résumée** :
1. Dans `BattleShip.API/Program.cs`, suppression de `app.UseHttpsRedirection()` en profil HTTP local et ajout de l'origine frontend `http://localhost:5270` dans la politique CORS.
2. Dans `BattleShip.App/wwwroot/css/app.css`, ajout de la règle `#blazor-error-ui { display: none; ... }` avec style sombre tactique (l'élément `<div>` Blazor n'avait aucun CSS de masquage par défaut suite à la refonte HUD).

**Décision** : Acceptée. Rétablit le bon fonctionnement réseau de l'API et nettoie parfaitement l'interface utilisateur.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj` (56 tests réussis), validation navigateur en mode HTTP.

**Preuve** : Commit `ca3a7a0`, `BattleShip.API/Program.cs`, `BattleShip.App/wwwroot/css/app.css`, `BattleShip.App/wwwroot/index.html`.
