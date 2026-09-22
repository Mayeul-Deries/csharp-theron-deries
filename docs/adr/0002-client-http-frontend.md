# ADR 0002 : isoler les appels REST du frontend

## Statut et date
Accepté le 2026-09-16.

## Contexte
Le frontend Blazor doit avancer avant l'implémentation complète de l'API. Les routes REST annoncées sont `POST /games`, `GET /games/{gameId}` et `POST /games/{gameId}/shots`. Le service gRPC de l'IA n'est pas encore disponible dans le dépôt.

## Options envisagées
- Appeler `HttpClient` directement depuis `Home.razor` : rapide, mais couple l'interface au transport et rend les erreurs difficiles à tester.
- Créer un client HTTP dédié : centralise les routes, la sérialisation et les réponses d'erreur.
- Simuler l'IA côté frontend : permettrait une démonstration immédiate, mais falsifierait le contrat gRPC à venir.

## Décision
Créer `GameApiClient` dans `BattleShip.App/Services`, avec une base URL `https://localhost:7091/`, les trois opérations REST et une désérialisation locale minimale de la réponse de tir. Ne pas simuler l'appel gRPC ; l'état `OpponentTurn` est affiché jusqu'à l'intégration du contrat Protobuf réel.

## Conséquences
Le composant de page reste centré sur l'état de l'interface et le client REST porte le transport. Le frontend compile indépendamment du backend final. Une étape ultérieure devra ajouter le client gRPC-Web, la configuration CORS et les tests d'intégration réseau.

## Vérification et réexamen
`dotnet test BattleShip.Tests\BattleShip.Tests.csproj --no-restore` réussit avec 22 tests. Cette vérification ne prouve ni l'accessibilité de l'API, ni la compatibilité CORS, ni l'échange gRPC-Web.

## Références
- Commit `feat(frontend): integrate game REST contract`
- `Ressources Bataille Navale/REVUE-IA.md`
