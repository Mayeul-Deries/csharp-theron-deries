# ADR 0001 : modélisation du domaine et intégrité de la grille

## Statut et date
Accepté le 2026-09-15.

## Contexte
Le moteur de jeu de la bataille navale doit encapsuler l'ensemble des règles métier (placement des flottes, validation des coordonnées, résolution des tirs, détection des navires coulés et alternance des tours) de manière totalement indépendante des couches de transport (Minimal API, gRPC) et d'interface utilisateur (Blazor WebAssembly).
Il doit être rigoureusement testable de manière unitaire et garantir l'intégrité de l'état de jeu à tout instant.

## Options envisagées
- **Matrice 2D mutable (`Cell[,]`)** : Modéliser la grille sous forme d'un tableau à deux dimensions de cellules mutables. Bien que rapide à prototyper, cette approche disperse la responsabilité du navire, rend difficile la distinction entre « touché » et « coulé » sans parcours récursifs lourds, et fragilise les invariants de chevauchement.
- **Modèle riche à base d'entités et value objects (`Coordinate`, `Ship`, `Grid`, `GameEngine`)** : Isoler la coordonnée comme un type immuable (row, col bornés entre 0 et 9), le navire comme une entité calculant ses coordonnées occupées à partir d'une origine et d'une direction, la grille comme détentrice de la collection de navires et de l'historique des tirs, et le moteur de jeu comme orchestrateur de la partie (deux grilles, état de jeu, alternance des tours).

## Décision
Retenir l'architecture de modèle riche dans le projet `BattleShip.Models/Domain` :
1. `Coordinate` est un record immuable validant la borne de grille `[0, 9]`.
2. `Ship` calcule géométriquement ses cases occupées via son origine, sa direction (`Horizontal` ou `Vertical`) et sa longueur propre via `Ship.GetLength(ShipType)`.
3. `Grid` applique les règles d'intégrité : aucun débordement hors limites, aucun chevauchement entre navires, et traçabilité des tirs subis.
4. `GameEngine` pilote le cycle de vie de la partie (`NotStarted`, `PlayerTurn`, `OpponentTurn`, `PlayerWon`, `OpponentWon`), l'évaluation des tirs (`Miss`, `Hit`, `Sunk`) et empêche tout nouveau tir après la fin de partie.
5. Le masquage des navires ennemis non découverts est délégué à un composant de projection (`GamePrivacyMapper`) respectant le principe de moindre privilège.

## Conséquences
- Le domaine est strictement agnostique de la technologie d'affichage et des protocoles réseau.
- La logique métier est vérifiée exhaustivement par des tests unitaires xUnit purs, rapides et fiables.
- Les validateurs API (FluentValidation) et l'IA adverse s'appuient sur des contrats et des types fortement typés sans duplication de code.

## Vérification et réexamen
`dotnet test BattleShip.Tests` valide l'ensemble des règles (débordement, chevauchement, calcul des tirs et victoire). Le modèle sera enrichi par la méthode `TrySetupPlayerShips` lors de l'intégration du placement manuel.

## Références
- Commit initial `4a975f3 1st Commit : Models and Tests`
- `BattleShip.Models/Domain/GameEngine.cs`
- `BattleShip.Models/Domain/Grid.cs`
- `BattleShip.Models/Domain/Ship.cs`
- `BattleShip.Tests/EngineTests.cs`
