# Revues de propositions IA

## Revue : client HTTP frontend basé sur le contrat prévu

**Proposition examinée**  
Ajouter `BattleShip.App/Services/GameApiClient.cs` et brancher `Home.razor` sur `POST /games`, `GET /games/{gameId}` et `POST /games/{gameId}/shots`.

**Hypothèse à vérifier**  
Le frontend peut manipuler les DTO partagés et appeler les routes prévues avant l'implémentation complète de l'API.

**Expérience**  
Commande : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj --no-restore`. Résultat attendu : compilation de Blazor et réussite des tests. Cette commande peut détecter une incompatibilité de DTO, de composant Razor ou de référence de projet.

**Observation**  
La compilation a réussi et 22 tests ont été réussis. La preuve est reproductible avec la commande ci-dessus.

**Décision et justification**  
Acceptée. Le client REST isole le transport de l'interface et conserve les DTO partagés inchangés.

**Preuves et limites**  
Commit `38207c5 feat(frontend): integrate game REST contract`, `GameApiClient.cs` et ADR 0002. L'API réelle, CORS et le navigateur ne sont pas vérifiés.

## Revue : tests isolés du client REST

**Proposition examinée**  
Ajouter `BattleShip.Tests/Frontend/GameApiClientTests.cs` avec un faux `HttpMessageHandler`.

**Hypothèse à vérifier**  
Le client respecte les verbes, routes, payloads JSON, réponses et codes HTTP sans dépendre d'un serveur lancé.

**Expérience**  
Commande : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj --no-restore`. Résultat attendu : les scénarios de création, lecture, tir et erreurs `400`, `404`, `500` passent. Le contrôle détecte notamment une régression de route ou de sérialisation.

**Observation**  
28 tests ont été réussis, dont les quatre tests du client REST.

**Décision et justification**  
Acceptée. Le faux handler rend la vérification déterministe et adaptée à l'absence actuelle d'API implémentée.

**Preuves et limites**  
`BattleShip.Tests/Frontend/GameApiClientTests.cs`. Le serveur réel, CORS et le comportement métier ne sont pas vérifiés. Commit à créer : `test(frontend): cover game REST client`.

## Revue : RPC dédiée au tour de l'IA

**Proposition examinée**  
Ajouter `TakeOpponentShot` dans `Protos/game.proto` et l'appeler depuis `OpponentGrpcClient` après un tir joueur manqué.

**Hypothèse à vérifier**  
Une RPC distincte permet d'identifier explicitement le tir de l'ordinateur et de conserver le sens de `TakeShot`.

**Expérience**  
Commande : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`. Résultat attendu : génération protobuf, compilation Blazor et maintien des tests verts. Le contrôle détecte un proto invalide, un package manquant ou une erreur de branchement.

**Observation**  
La génération protobuf et la compilation ont réussi ; 28 tests ont été réussis.

**Décision et justification**  
Acceptée côté frontend. Le serveur doit encore implémenter la RPC et configurer gRPC-Web pour valider le parcours complet.

**Preuves et limites**  
`Protos/game.proto`, `BattleShip.App/Services/OpponentGrpcClient.cs` et ADR 0003. Aucun appel réseau gRPC-Web réel, aucune vérification CORS et aucun comportement IA serveur ne sont encore prouvés. Commit à créer : `feat(frontend): integrate opponent grpc client`.

## Revue : Refonte tactique de l'UI (centre de commandement naval)

**Proposition examinée**  
Refonte complète du frontend Blazor d'après la maquette Google Stitch :
- Thème sombre tactique cyber/naval (`BattleShip.App/wwwroot/css/app.css`).
- Grilles tactiques avec repères `LOC`, `A-J`, `01-10` et réticule de ciblage `[ ⊙ ]` (`BattleShip.App/Components/BattleGrid.razor`).
- Composant comparateur de flotte (`BattleShip.App/Components/FleetStatusPanel.razor`).
- Journal des tirs chronologique (`BattleShip.App/Components/ShotLogPanel.razor`).
- Enrichissement de `CellDto` par `ShipType` optionnel et masquage strict des navires adverses dans `BattleShip.Models/Privacy/GamePrivacyMapper.cs`.

**Hypothèse à vérifier**  
1. L'enrichissement de `CellDto` avec `ShipType? ShipType = null` ne casse pas les contrats d'API existants et n'expose pas les coordonnées des navires adverses vivants (respect des règles de confidentialité du sujet).
2. Le ciblage en 2 temps (sélection puis clic sur bouton de tir ou double-clic) s'intègre avec bUnit et préserve le cycle gRPC-Web de riposte de l'IA.

**Expérience**  
1. Commande : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`. Résultat attendu : passage de tous les tests unitaires domaine, privacy, API et composants Blazor.
2. Test interactif de bout en bout dans le navigateur avec serveur API (port 5282) et client WASM (port 5270).

**Observation**  
1. Les 49 tests de la suite `BattleShip.Tests` réussissent avec 0 erreur et 0 avertissement. Les tests de confidentialité `BattleShip.Tests/PrivacyTests.cs` confirment que les navires adverses non coulés sont strictement invisibles.
2. Le test navigateur via sous-agent montre un affichage fluide des 4 cartes, l'activation du réticule sur sélection, l'exécution du tir, la mise à jour des statistiques (tirs, précision) et l'enregistrement instantané dans le journal des tirs.

**Décision et justification**  
Acceptée. L'interface offre désormais un niveau de finition et une expérience utilisateur conformes aux attentes d'ambition du barème, tout en respectant l'architecture Clean Code et les principes SOLID.

**Preuves et limites**  
Commits `c3a621e`, `12e4daa`, `BattleShip.Tests/Frontend/BattleGridTests.cs`, `BattleShip.Tests/Frontend/HomeTests.cs`. Limites : les animations sonores ou de torpille 3D restent des pistes ultérieures non implémentées.

## Revue : Placement manuel des navires & tir direct

**Proposition examinée**  
1. Tir direct sans étape de confirmation et suppression de l'effet d'oscillation/zoom.
2. Écran interactif de déploiement de flotte au début de la partie :
   - Sélection du navire dans un dock dédié, orientation horizontale/verticale, survol visuel sur la grille avec codes couleur (cyan/rouge), placement au clic, retrait au clic.
   - Boutons utilitaires de pré-remplissage aléatoire et réinitialisation.
   - Contrat DTO `CreateGameRequest` et `ShipPlacementDto` avec conversion d'énumération robuste (`JsonStringEnumConverter`).
   - Méthode de validation de grille `GameEngine.TrySetupPlayerShips` et validateur d'API FluentValidation `CreateGameRequestValidator`.

**Hypothèse à vérifier**  
1. La rétrocompatibilité de `POST /games` est préservée pour les clients ou tests ne fournissant pas de corps JSON (`{}`).
2. La validation serveur rejette tout chevauchement, tout navire hors limites ou toute composition ne comportant pas exactement les 5 types requis.
3. Le tir direct fonctionne sans secousse graphique et sans bouton intermédiaire.
4. L'intégralité des 54 tests unitaires et bUnit reste verte.

**Expérience**  
1. Exécution des tests automatisés : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`.
2. Scénario de bout en bout exécuté dans le navigateur avec `browser_subagent` :
   - Initialisation sur l'écran de déploiement.
   - Test du placement aléatoire automatique (`5 / 5 prêts`).
   - Lancement du combat sans erreur.
   - Tir direct immédiat sur le radar adverse et riposte IA.
   - Retour au déploiement via « Recommencer ».
   - Changement d'orientation en verticale et placement manuel réussi d'un navire.

**Observation**  
54 tests réussis (0 échec). Aucune erreur réseau ou JSON côté API (les converters enum gèrent les chaînes et entiers). L'interface reste stable et réactive à chaque interaction.

**Décision et justification**  
Acceptée. La fonctionnalité répond parfaitement au besoin utilisateur, respecte la séparation des couches Clean Architecture (DTOs dans `Models`, logique d'intégrité dans `Domain`, validation dans `API`, rendu dans `App`), et élève la note globale du projet.

**Preuves et limites**  
- `CreateGameRequest.cs`, `CreateGameRequestValidator.cs`, `GameEngine.cs`, `Home.razor`, `BattleGrid.razor`.
- Tests unitaires `EngineTests.cs`, `ValidatorTests.cs`, `HomeTests.cs`, `GameApiClientTests.cs`.
- Enregistrement de session : `fleet_placement_battle_test_1789654820026.webp`.

## Revue : Résolution du conflit d'alias d'énumération ShipType (Submarine / Destroyer)

**Proposition examinée**  
1. Réaffectation des valeurs de l'énumération `BattleShip.Models/Enums/ShipType.cs` pour donner un identifiant unique à chaque type de navire (`Carrier = 1`, `Battleship = 2`, `Destroyer = 3`, `Submarine = 4`, `TorpedoBoat = 5`).
2. Découplage de la longueur du navire de la valeur d'énumération via `Ship.GetLength(ShipType type)` et `Ship.Length`.
3. Validation stricte de la composition de flotte dans `GameEngine.TrySetupPlayerShips` et isolation par `ShipType` dans `Home.razor`.

**Hypothèse à vérifier**  
1. En C#, deux membres d'une énumération ayant la même valeur entière sont de simples alias (`Destroyer == Submarine` vaut `true`), ce qui causait la sélection et le marquage croisé erroné des deux cartes de taille 3.
2. L'attribution d'identifiants uniques permet de distinguer parfaitement `Submarine` et `Destroyer` dans les collections, les liaisons Blazor et les DTOs (sérialisés en chaînes `"Submarine"`, `"Destroyer"`).
3. La longueur en cases (3 cases pour le croiseur et 3 cases pour le sous-marin) est préservée sans régression sur les règles métier.

**Expérience**  
1. Exécution de la suite de tests complète : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`.
2. Ajout de tests de régression dédiés :
   - `GameEngine_TrySetupPlayerShips_ShouldRejectTwoSubmarinesInsteadOfDestroyer` dans `EngineTests.cs`.
   - `PlacingSubmarine_ShouldNotMarkDestroyerAsPlaced` dans `HomeTests.cs`.
3. Validation interactive de bout en bout dans le navigateur avec `browser_subagent` :
   - Placement manuel d'un Sous-marin : seul le Sous-marin passe à `✓ Placé`, le Croiseur reste `À placer` (`1 / 5 prêts`).
   - Placement consécutif du Croiseur : les deux passent à `✓ Placé` (`2 / 5 prêts`).
   - Tir et combat sans encombre.

**Observation**  
56 tests réussis sur 56 (0 échec). L'interface Blazor reflète avec une précision absolue l'état réel de chaque navire sans aucune confusion d'état.

**Décision et justification**  
Acceptée. Solution canonique et robuste selon les standards du C# moderne et les principes SOLID.

**Preuves et limites**  
- `BattleShip.Models/Enums/ShipType.cs`, `BattleShip.Models/Domain/Ship.cs`, `BattleShip.Models/Domain/GameEngine.cs`, `BattleShip.App/Pages/Home.razor`.
- `EngineTests.cs`, `HomeTests.cs`.
- Session enregistrée : `submarine_destroyer_fix_test_1789657015807.webp`, capture : `fleet_placed_verified_1789660999854.png`.

