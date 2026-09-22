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

## 2026-09-17 — Refonte tactique de l'UI d'après maquette Google Stitch

**Outil / modèle** : Antigravity, Gemini.

**Contexte** : Le skin initial de l'application Blazor est le template par défaut de Microsoft. Une maquette générée sur Google Stitch présente une interface de commandement naval sombre (Radar Ennemi, Votre Flotte avec intégrité, État de la flotte Alliés vs Ennemis, et Journal des tirs).

**Prompt** : « Ok niquel. Bon actuellement mon jeu est très moche, c'est le skin de base de blazor et j'ai envie de l'améliorer. J'ai demandé à google stitch de générer une maquette que je te transmets en photo, et je fais appelle à toi pour l'implémenter car tu es bon en frontend. Si ca te convient tu peux démarer. »

**Réponse résumée** : Établir un plan d'architecture respectant les contraintes du cours (anti-triche, contrats DTO, bUnit), enrichir `CellDto` avec `ShipType` optionnel (sans fuite d'information adverse), créer un design system tactique CSS complet (`app.css`), ajouter les composants `FleetStatusPanel` et `ShotLogPanel`, implémenter le ciblage en 2 temps (sélection de coordonnée avec réticule `[ ⊙ ]` puis tir via `TIRER SUR [COORD]`), et valider avec 49 tests unitaires et bUnit au vert.

**Décision** : Acceptée. La refonte transforme radicalement l'attrait visuel et la finition du projet tout en garantissant la maintenabilité et le respect strict des règles métier.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`. Résultat attendu : 49 tests passés avec succès. Test interactif dans le navigateur avec `browser_subagent` validant la séquence complète d'engagement, la riposte gRPC de l'IA et le journal.

**Preuve** : Commits `c3a621e`, `12e4daa`, enregistrement vidéo navigateur `tactical_ui_test_1789638490429.webp`, et revue dans `REVUE-IA.md`.

## 2026-09-17 — Validation directe du tir au clic et stabilisation visuelle

**Outil / modèle** : Antigravity, Gemini.

**Contexte** : Après la mise en place du skin tactique, le joueur demande de supprimer l'étape de sélection préalable / bouton de confirmation pour valider le tir immédiatement au clic, et de corriger l'effet de zoom/dézoom oscillant provoqué par l'animation CSS `reticle-pulse`.

**Prompt** : « Je ne veux pas de confirmation lors du clic d'une case, je veux que ca valide directe »

**Réponse résumée** : Retrait du bouton de confirmation et de l'état intermédiaire dans `Home.razor`, déclenchement immédiat de `FireShotAsync` au clic, élimination de `animation: reticle-pulse` et des crochets textuels `[ ⊙ ]` pour garantir un layout shift nul.

**Décision** : Acceptée. Améliore fortement la réactivité du gameplay.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`. Enregistrement de session navigateur `direct_shot_test_1789647690777.webp`.

**Preuve** : Commit `92c0be1`, tests bUnit validant le déclenchement immédiat.

## 2026-09-17 — Placement manuel des navires par le joueur au début de partie

**Outil / modèle** : Antigravity, Gemini.

**Contexte** : Permettre au joueur de positionner manuellement ses 5 navires au lancement du jeu ou au redémarrage, avec choix d'orientation, prévisualisation interactive, dock de flotte, options aléatoires rapides et validation d'intégrité côté serveur.

**Prompt** : « il faudrait faire en sorte que le joueur puisse placer ses bateaux au début de la partie »

**Réponse résumée** : Création d'un plan d'implémentation complet approuvé par l'utilisateur. Modélisation de `CreateGameRequest` et `ShipPlacementDto` avec sérialisation enum `JsonStringEnumConverter`. Ajout de `TrySetupPlayerShips` dans `GameEngine` et validateur FluentValidation `CreateGameRequestValidator`. Écran interactif de déploiement dans `Home.razor` avec survol en cyan/rouge, toggle d'orientation, placement aléatoire et réinitialisation. Couverture de tests à 54 tests au vert.

**Décision** : Acceptée. Offre une expérience stratégique complète au joueur avant le combat.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj` (54 réussis). Enregistrement vidéo navigateur `fleet_placement_battle_test_1789654820026.webp` et capture `final_game_state_1789655157917.png`.

**Preuve** : `CreateGameRequest.cs`, `CreateGameRequestValidator.cs`, `GameEngine.cs`, `Home.razor`, tests unitaires et bUnit.

## 2026-09-17 — Résolution de la collision d'alias d'énumération entre Sous-marin et Croiseur

**Outil / modèle** : Antigravity, Gemini.

**Contexte** : Lors du placement manuel de la flotte, placer un Sous-marin entraînait l'affichage simultané du Sous-marin et du Croiseur (Destroyer) comme « Placé » dans le dock, avec un compteur erroné.

**Prompt** : « quand je place un sous-marin ca m'en place un mais ca note les 2 comme placés, sauf qu'en réalité j'en ai placé qu'un sur deux »

**Réponse résumée** : Identification de la cause racine dans l'énumération C# `ShipType` où `Destroyer = 3` et `Submarine = 3` partageaient la même valeur entière sous-jacente, créant un alias indistinguable à l'exécution pour l'opérateur d'égalité, les collections de hachage et la réflexion. Attribution de valeurs entières distinctes uniques (`Carrier = 1`, `Battleship = 2`, `Destroyer = 3`, `Submarine = 4`, `TorpedoBoat = 5`), dissociation de la taille en cases via la méthode `Ship.GetLength()`, sécurisation de l'isolation du dock dans `Home.razor`, ajout de tests unitaires et bUnit (56 tests au total, 100% verts).

**Décision** : Acceptée. Règle définitivement la collision d'alias tout en conservant les dimensions des navires (3 cases pour chacun) et le respect des règles métier.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj` (56/56 réussis). Test interactif dans le navigateur avec enregistrement vidéo confirmant l'indépendance stricte du Sous-marin et du Croiseur.

**Preuve** : `ShipType.cs`, `Ship.cs`, `GameEngine.cs`, `Home.razor`, `EngineTests.cs`, `HomeTests.cs`, vidéo `submarine_destroyer_fix_test_1789657015807.webp`.

