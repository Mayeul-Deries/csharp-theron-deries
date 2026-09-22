# Revues de propositions IA

## Revue : Stratégie de l'IA (Amélioration)
**Proposition examinée**
Implémentation d'une stratégie de recherche "Checkerboard" (damiers) combinée à une stratégie de chasse adjacente dans `AiOpponentService.cs`.

**Hypothèse à vérifier**
Que l'IA soit plus efficace qu'un tir purement aléatoire.

**Expérience**
Simulation de parties dans `AITests.cs` où l'IA doit trouver un navire. Résultat attendu : le navire est coulé plus rapidement qu'avec un tir aléatoire complet.

**Observation**
Le code implémenté dans `AiOpponentService.cs` réduit l'espace de recherche lors de la phase de chasse et privilégie les cases (row+col)%2==0 lors de la phase de recherche.

**Décision et justification**
Acceptée. Améliore la jouabilité et l'ambition du projet.

**Preuves et limites**
Preuve par commit. Limite : l'IA ne mémorise pas les navires coulés pour éviter de tirer autour, ce qui pourrait être une extension.

## Revue : Architecture Hybride (REST + gRPC-Web)
**Proposition examinée**
Utiliser REST pour la récupération d'état (`GET`) et gRPC-Web pour l'action de tir.

**Hypothèse à vérifier**
Que la combinaison respecte les contraintes et simplifie le développement.

**Expérience**
Validation par `dotnet build` et tests d'intégration `ApiRobustnessTests.cs`.

**Observation**
Configuration fonctionnelle, CORS OK.

**Décision et justification**
Acceptée. Défendue dans l'ADR 0001.

**Preuves et limites**
Commit de configuration gRPC et tests d'intégration au vert.

## Revue : Sécurité du masquage des informations
**Proposition examinée**
Utilisation de `GamePrivacyMapper.ToStatusDto` pour filtrer les données avant envoi au client.

**Hypothèse à vérifier**
Que le client ne puisse jamais connaître les positions des bateaux adverses.

**Expérience**
Test `PrivacyTests.ToStatusDto_ShouldHideUnshotOpponentShips` qui vérifie que les cellules `CellState.Ship` sont absentes.

**Observation**
Le test passe, les cases non touchées sont marquées `CellState.Empty`.

**Décision et justification**
Acceptée. Indispensable pour la sécurité du jeu.
