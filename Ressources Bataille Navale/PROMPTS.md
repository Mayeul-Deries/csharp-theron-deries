# Échanges décisifs avec l’IA

## 16/09/2026 - Architecture Hybrid REST + gRPC

- **Outil / modèle** : Gemini
- **Contexte** : Besoin d'une communication performante pour les tirs et simple pour le statut.
- **Prompt** : "Nous voulons une architecture hybride : REST pour le statut, gRPC-Web pour le tir. Comment configurer le CORS et gRPC-Web pour que Blazor appelle les deux ?"
- **Réponse** : Configuration de `UseGrpcWeb()` dans `Program.cs`, ajout des headers exposés dans le CORS, ajout des packages `Grpc.AspNetCore.Web`.
- **Décision** : Acceptée.
- **Vérification** : `dotnet build` au vert, tests d'intégration fonctionnels.

## 16/09/2026 - Amélioration de l'IA

- **Outil / modèle** : Gemini
- **Contexte** : L'IA tirait aléatoirement, ce qui n'est pas assez ambitieux.
- **Prompt** : "Comment rendre l'IA plus intelligente ? Implémente une stratégie 'damiers' pour la recherche et une logique de chasse pour finir les navires."
- **Réponse** : Implémentation via `GenerateShot` avec `FindTargetFromPreviousHits` et checkerboard pattern.
- **Décision** : Acceptée.
- **Vérification** : Tests unitaires dans `AITests.cs` vérifiant que l'IA cible les cases adjacentes après un tir réussi.
