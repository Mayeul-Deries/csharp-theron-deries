# Revues de propositions IA

Trois revues argumentées minimum. Aucune erreur n’est exigée ; chaque conclusion doit être étayée.

## Revue : sujet du projet

- Proposition et référence dans le dépôt :
- Hypothèse à vérifier :
- Scénario, données ou commande :
- Résultat attendu avant exécution :
- Erreur que ce contrôle pourrait détecter :
- Résultat réellement observé :
- Décision et justification :
- Preuves reproductibles et liens vers les commits :
- Après correction éventuelle : résultat avant / après :
- Limites et points non vérifiés :

## Revue : client HTTP frontend basé sur le contrat prévu

- Proposition et référence dans le dépôt : ajouter `BattleShip.App/Services/GameApiClient.cs` et brancher la page Blazor sur les trois routes REST annoncées par le binôme.
- Hypothèse à vérifier : le frontend peut compiler et manipuler les DTO partagés avant que l'API réelle et le service gRPC soient intégrés.
- Scénario, données ou commande : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj --no-restore`.
- Résultat attendu avant exécution : compilation de l'application Blazor et réussite des tests existants.
- Erreur que ce contrôle pourrait détecter : mauvais nom de route, type de DTO incompatible, composant Razor non résolu ou régression de la grille.
- Résultat réellement observé : compilation réussie et 22 tests réussis.
- Décision et justification : proposition acceptée pour l'étape frontend HTTP. La réponse de tir est désérialisée dans un type privé minimal afin de ne pas modifier `ShotResultDto` partagé avant confirmation du contrat final.
- Preuves reproductibles et liens vers les commits : `feat(frontend): integrate game REST contract`.
- Après correction éventuelle : une directive `@using BattleShip.App.Components` a été ajoutée après le premier build pour supprimer l'avertissement Razor `RZ10012`.
- Limites et points non vérifiés : aucun test d'intégration réseau, aucune vérification CORS, aucune preuve d'échange gRPC-Web et aucun test navigateur ; l'API backend n'expose pas encore les routes annoncées.

## Revue : tests isolés du client REST

- Proposition et référence dans le dépôt : ajouter `BattleShip.Tests/Frontend/GameApiClientTests.cs` avec un faux `HttpMessageHandler`.
- Hypothèse à vérifier : le client respecte le contrat HTTP indépendamment de l'implémentation du serveur.
- Scénario, données ou commande : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj --no-restore`.
- Résultat attendu avant exécution : les routes, verbes, corps JSON, réponses et erreurs HTTP doivent être vérifiés sans accès réseau.
- Erreur que ce contrôle pourrait détecter : régression de route, payload incorrect, enum non désérialisable ou exception HTTP sans statut.
- Résultat réellement observé : 28 tests réussis ; les quatre nouveaux scénarios passent.
- Décision et justification : proposition acceptée. Le test isolé est adapté à cette étape car l'API n'est pas encore implémentée.
- Preuves reproductibles et liens vers les commits : fichier `BattleShip.Tests/Frontend/GameApiClientTests.cs`; commit à créer après revue.
- Après correction éventuelle : aucune correction nécessaire après exécution.
- Limites et points non vérifiés : pas de serveur réel, pas de CORS, pas de test gRPC-Web et pas de vérification du comportement métier du moteur.
