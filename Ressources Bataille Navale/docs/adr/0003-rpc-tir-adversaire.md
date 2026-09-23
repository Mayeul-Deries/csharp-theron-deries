# ADR 0003 : dédier une RPC au tir de l'adversaire

## Statut et date
Accepté le 2026-09-16

## Contexte
Le contrat gRPC existant décrit `GameService.TakeShot`, mais cette méthode représente le tir du joueur et ne déclenche pas le tour de l'IA. Le frontend doit pourtant pouvoir exécuter une partie complète contre l'ordinateur après un tir manqué.

## Options envisagées
- Faire évoluer `TakeShot` pour choisir implicitement l'acteur : ambiguïté métier et risque de casser les clients existants.
- Ajouter une RPC `TakeOpponentShot` dédiée : intention explicite et séparation claire des responsabilités.
- Faire jouer l'IA dans la réponse HTTP du tir joueur : contredit le flux backend annoncé et mélange deux transports.

## Décision
Conserver `TakeShot` pour le tir joueur et ajouter :

```proto
rpc TakeOpponentShot (OpponentShotRequest) returns (OpponentShotResponse);
```

La requête contient `game_id`. La réponse contient le résultat du tir et ses coordonnées (`result`, `row`, `col`). Le frontend rafraîchit ensuite `GET /games/{gameId}`.

## Conséquences
Le frontend peut déclencher explicitement le tour de l'IA via gRPC-Web. Le backend doit ajouter l'implémentation de la RPC, enregistrer gRPC-Web et conserver le même fichier `Protos/game.proto`. Une évolution ultérieure pourra ajouter l'état de partie à la réponse si le GET final devient inutile.

## Vérification et réexamen
Le client gRPC généré compile avec `dotnet test BattleShip.Tests\BattleShip.Tests.csproj`. L'échange réel reste à vérifier après l'implémentation serveur et la configuration CORS.

## Références
- `Protos/game.proto`
- Commit à créer : `feat(frontend): integrate opponent grpc client`
