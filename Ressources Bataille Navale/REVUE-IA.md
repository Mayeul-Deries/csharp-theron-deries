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

