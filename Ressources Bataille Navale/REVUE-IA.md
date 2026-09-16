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
