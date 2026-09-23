**PROMPTS.md — Échanges décisifs avec l'IA**

**2026-09-16 — Client REST du frontend**

**Outil / modèle** : GitHub Copilot CLI (gpt-5.6-luna)

**Contexte** : le backend n'est pas encore implémenté, mais les routes REST et les DTO ont été définis avec Boris. Objectif : avancer le frontend Blazor sans attendre l'API, tout en posant des bases de code propres pour la suite du projet.

**Prompt** : « Le backend n'est pas encore fait, mais on a défini le contrat avec mon binôme : routes REST, DTO partagés, flux de tour (voir schéma ci-joint). Je veux démarrer le frontend Blazor sur cette base. Avant de coder, voici quelques consignes générales que je veux que tu respectes pour tout le projet, pas seulement cette tâche : respecte les DTO partagés dans BattleShip.Models et ne les modifie jamais sans qu'on en discute, c'est le contrat entre le front et l'API. Garde un seul niveau de responsabilité par classe : le client HTTP ne doit connaître que le transport, jamais la logique de jeu ni l'état de l'UI. Injecte les dépendances par constructeur plutôt que d'instancier un service en dur, ça facilite les tests et respecte l'inversion de dépendances. Le code doit rester testable, avec un HttpMessageHandler factice pour le client REST par exemple. Nommage en anglais, PascalCase pour les membres publics, pas d'abréviations obscures. Aucune logique métier dans les composants razor, ils orchestrent l'affichage et rien de plus. Gère les erreurs réseau explicitement, pas de try/catch vide. Pour cette tâche précise : crée un client HTTP typé, affiche les deux grilles, envoie les tirs du joueur et relis le statut après chaque tir. Ne simule pas le gRPC tant que son contrat n'est pas disponible, on le fera dans un échange dédié. »

**Réponse résumée** : client HTTP typé (`GameApiClient`) injecté via DI, interface `IGameApiClient` pour permettre le mock en test, affichage des deux grilles dans `Home.razor`, envoi du tir joueur, relecture du statut après chaque action, gestion des codes d'erreur HTTP avec messages différenciés.

**Décision** : acceptée. Cette séparation permet d'avancer en parallèle du travail de Boris sur l'API, sans dépendance bloquante, et pose une base injectable et testable pour la suite.

**Vérification** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj --no-restore` → 22 tests réussis, compilation OK. Ce contrôle ne couvre ni l'API réelle ni CORS.

**Preuve** : commit `38207c5`, `GameApiClient.cs`, `IGameApiClient.cs`, `Home.razor`, ADR 0002.

**2026-09-16 — RPC dédiée au tir de l'IA**

**Outil / modèle** : GitHub Copilot CLI (gpt-5.6-luna)

**Contexte** : le contrat défini avec Boris ne prévoit `TakeShot` que pour le joueur. Il manque un moyen explicite de déclencher le tir de l'ordinateur après un `Miss`, alors que jouer contre l'IA est une contrainte du socle.

**Prompt** : « Le contrat actuel ne couvre que le tir du joueur via TakeShot. On a besoin d'une opération distincte pour le tir de l'IA, pour ne pas ambiguïser qui joue à un instant donné. Je reprends les mêmes principes que sur la tâche précédente : n'altère pas TakeShot, ajoute une RPC séparée TakeOpponentShot, c'est le principe ouvert à l'extension fermé à la modification. Le client gRPC doit être injectable et mockable comme GameApiClient, avec la même logique d'interface. Évite tout couplage entre le composant Blazor et les détails du canal gRPC, passe par le client généré et encapsule-le. Gère les RpcException proprement, avec un message utilisateur clair plutôt qu'une trace technique brute affichée à l'écran. Concrètement : ajoute la RPC TakeOpponentShot dans le proto, génère le client gRPC-Web Blazor, et appelle-la automatiquement après un Miss du joueur. Ne simule pas le comportement serveur, ce sera implémenté côté API par Boris, je veux juste que le front soit prêt à brancher dessus. »

**Réponse résumée** : nouvelle RPC avec `game_id` en entrée et `result/row/col` en sortie, `OpponentGrpcClient` encapsulé derrière une interface, appel côté frontend après chaque `Miss`, relecture de l'état via `GET /games/{gameId}`, gestion des `RpcException` avec message utilisateur générique.

**Décision** : acceptée côté frontend. L'implémentation côté API reste à charge de Boris, on garde une frontière claire entre les deux rôles du tireur.

**Vérification** : `dotnet test` → génération protobuf réussie, 28 tests verts. Ce contrôle ne vérifie pas l'échange réseau réel, l'API n'étant pas encore disponible.

**Preuve** : `Protos/game.proto`, `OpponentGrpcClient.cs`, `IOpponentGrpcClient.cs`, ADR 0003.

**2026-09-17 — Refonte de l'interface (maquette Stitch)**

**Outil / modèle** : Antigravity (Gemini)

**Contexte** : l'application utilisait encore le thème par défaut de Blazor. Une maquette d'interface de commandement naval a été générée sur Google Stitch pour servir de base à la refonte visuelle.

**Prompt** : « Le jeu fonctionne mais l'interface reprend le style par défaut de Blazor, ce qui n'est pas satisfaisant pour la démo finale. Voici une maquette que j'ai générée avec Google Stitch (capture jointe) : un thème sombre, un radar ennemi, ma flotte avec son intégrité, un comparateur de flottes et un journal des tirs. Avant de commencer, quelques contraintes non négociables du projet à respecter : les navires adverses non touchés ne doivent jamais être révélés côté client, même dans les données brutes envoyées par l'API, vérifie GamePrivacyMapper si tu touches à CellDto. Toute évolution des DTO partagés doit rester rétrocompatible ou être documentée dans un ADR. Découpe l'UI en composants réutilisables comme FleetStatusPanel et ShotLogPanel plutôt qu'un Home.razor monolithique. Les tests bUnit existants doivent continuer à passer, ajoute les tests correspondant aux nouveaux composants. Le CSS doit rester centralisé, pas de styles inline dispersés dans les composants. Peux-tu établir un plan avant de coder, pour qu'on soit alignés sur l'architecture ? Si ça te convient tu peux démarrer une fois le plan validé. »

**Réponse résumée** : plan d'architecture proposé et validé, feuille de style tactique centralisée (`app.css`), composants `FleetStatusPanel` et `ShotLogPanel`, `CellDto` enrichi d'un `ShipType` optionnel sans fuite d'information côté adverse, ciblage en deux temps (sélection puis tir).

**Décision** : acceptée. Le mapping `GamePrivacyMapper` a été relu en détail pour s'assurer qu'aucune information sensible n'était exposée avant d'intégrer le changement de DTO.

**Vérification** : `dotnet test` → 49 tests verts. Test manuel dans le navigateur couvrant la séquence complète, tir, riposte de l'IA, journal des tirs.

**Preuve** : commits `c3a621e`, `12e4daa`, enregistrement `tactical_ui_test_1789638490429.webp`.

**2026-09-17 — Validation directe du tir au clic**

**Outil / modèle** : Antigravity (Gemini)

**Contexte** : le ciblage en deux temps ralentissait l'expérience de jeu, et l'animation du réticule provoquait un effet de zoom indésirable.

**Prompt** : « Je souhaite supprimer l'étape de confirmation lors du tir, le clic sur une case doit déclencher directement le tir, sans validation intermédiaire. Fais en sorte que le changement reste localisé au composant concerné, sans casser la structure des composants mise en place précédemment. Retire aussi l'animation reticle-pulse qui provoque un décalage visuel gênant. »

**Réponse résumée** : suppression du bouton de confirmation et de l'état intermédiaire dans `Home.razor`, déclenchement immédiat du tir au clic, retrait de l'animation `reticle-pulse` à l'origine du layout shift.

**Décision** : acceptée. Gain net en réactivité et en fluidité de jeu, sans effet de bord sur les autres composants.

**Vérification** : `dotnet test` → tests verts. Enregistrement `direct_shot_test_1789647690777.webp`.

**Preuve** : commit `92c0be1`.

**2026-09-17 — Placement manuel des navires**

**Outil / modèle** : Antigravity (Gemini)

**Contexte** : les flottes étaient jusque-là placées aléatoirement au démarrage. L'objectif est de permettre au joueur de positionner lui-même ses navires en début de partie, tout en gardant la validation des règles côté serveur.

**Prompt** : « Il faudrait permettre au joueur de placer manuellement ses navires au début de la partie, avec choix de l'orientation, une prévisualisation au survol, et une option de placement aléatoire rapide pour aller plus vite en test. La validation des chevauchements et des débordements doit se faire côté serveur dans GameEngine, jamais uniquement côté client, un utilisateur pourrait contourner l'UI. Utilise FluentValidation pour valider le CreateGameRequest entrant, cohérent avec le reste de l'API. Garde BattleShip.Models indépendant de tout détail HTTP ou JSON. Propose un plan avant d'implémenter, je veux valider l'approche avant que tu génères le code. »

**Réponse résumée** : plan validé, écran de déploiement avec dock de navires, orientation horizontale ou verticale, prévisualisation au survol, placement aléatoire rapide, DTO `CreateGameRequest` et `ShipPlacementDto`, validation métier dans `GameEngine.TrySetupPlayerShips` et validateur `CreateGameRequestValidator` côté API.

**Décision** : acceptée. Quelques ajustements CSS mineurs sur le dock ont été apportés a posteriori par nous-mêmes.

**Vérification** : `dotnet test` → 54 tests verts. Enregistrement `fleet_placement_battle_test_1789654820026.webp`.

**Preuve** : `CreateGameRequestValidator.cs`, `GameEngine.cs`, `Home.razor`.

**2026-09-17 — Correction : sous-marin et croiseur marqués simultanément**

**Outil / modèle** : Antigravity (Gemini)

**Contexte** : le placement d'un sous-marin marquait à tort le croiseur comme placé également, faussant le compteur de progression dans le dock de déploiement.

**Prompt** : « Quand je place un sous-marin, le dock marque aussi le croiseur comme placé, alors qu'un seul navire a réellement été positionné, le compteur affiche 2/5 au lieu de 1/5. Identifie la cause racine avant de corriger, je veux comprendre pourquoi, pas juste un patch qui masque le symptôme. Une fois la cause trouvée, corrige et ajoute un test de régression qui aurait détecté ce bug. »

**Réponse résumée** : cause identifiée, `Destroyer` et `Submarine` partageaient la même valeur numérique (`= 3`) dans l'énumération `ShipType`, rendant `Destroyer == Submarine` vrai au niveau du runtime par un effet d'alias d'énumération. Renumérotation des valeurs, découplage de la longueur via `Ship.GetLength()`, ajout de tests de régression dédiés.

**Décision** : acceptée. Diagnostic confirmé par comparaison directe des valeurs d'enum avant et après correction.

**Vérification** : `dotnet test` → 56/56 tests verts, y compris les deux tests de régression ajoutés. Vérification manuelle complémentaire dans le navigateur.

**Preuve** : `ShipType.cs`, `Ship.cs`, `EngineTests.cs`, `HomeTests.cs`.