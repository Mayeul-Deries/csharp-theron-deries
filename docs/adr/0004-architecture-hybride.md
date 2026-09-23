# ADR 0004 : Architecture de communication (REST vs gRPC-Web)

## Statut et date
Accepté le 2026-09-16.

## Contexte
Le projet nécessite une communication bidirectionnelle entre un front Blazor WebAssembly et un backend .NET 10. Les contraintes incluent la sécurité (masquage des informations adverses), la performance et l'exigence d'un échange gRPC-Web fonctionnel.

## Options envisagées
1. **REST intégral** : Simple, robuste, bien supporté par Blazor. Limité pour des échanges complexes en temps réel.
2. **gRPC-Web intégral** : Performant, typage fort, mais demande une configuration CORS et proxy plus complexe.
3. **Hybride (REST + gRPC-Web)** : REST pour les opérations de gestion de partie (légères, asynchrones) et gRPC-Web pour l'action de tir (bidirectionnelle/temps réel, besoin de latence faible).

## Décision
Option 3 (Hybride). Nous utilisons REST pour la gestion de l'état du jeu (`GET`) et gRPC-Web pour les actions de tir (`TakeShot`, `TakeOpponentShot`) afin de bénéficier du typage fort et de la performance du contrat binaire lors des échanges fréquents de tir.

## Conséquences
- **Avantages** : Meilleure performance sur les échanges critiques (tirs), sécurité renforcée par le typage.
- **Inconvénients** : Double stack réseau à configurer et maintenir (REST + gRPC).

## Vérification et réexamen
- Vérifié via tests d'intégration API et validation de la génération des clients gRPC. Réexamen si le front subit trop de latence ou des problèmes complexes de CORS.
