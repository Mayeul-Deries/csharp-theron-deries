# Projet C# ASP.NET Core & Blazor WebAssembly — Bataille Navale

## Binôme
- **Boris Theron** (`boris.theron@gmail.com`)
- **Mayeul Deries** (`mayeul.deries@gmail.com`)

---

## 1. Prérequis & Environnement

- **SDK .NET 10** (le fichier `global.json` à la racine cible la version `10.0.401`).
- Vérification de votre environnement :
  ```bash
  dotnet --version
  # Doit afficher une version 10.0.xxx
  ```

---

## 2. Lancement Rapide du Projet

L'application repose sur deux services distincts communiquant via **HTTP/JSON** et **gRPC-Web** :
1. **L'API Backend** (`BattleShip.API`) sur le port `5282` (HTTP) ou `7091` (HTTPS).
2. **Le Frontend Blazor WebAssembly** (`BattleShip.App`) sur le port `5270` (HTTP).

### Étape 1 : Démarrer le serveur API
Dans un premier terminal :
```bash
dotnet run --project BattleShip.API --launch-profile http
```
*Le serveur écoute sur `http://localhost:5282` et expose le document OpenAPI sur `http://localhost:5282/openapi/v1.json`.*

### Étape 2 : Démarrer le client Blazor WebAssembly
Dans un second terminal :
```bash
dotnet run --project BattleShip.App --launch-profile http
```
*L'application s'ouvre sur `http://localhost:5270`.*

### Étape 3 : Exécuter la suite de tests automatisés
```bash
dotnet test
```
*Exécute les 56 tests unitaires, de domaine, de confidentialité, de validation d'API et de composants bUnit.*

---

## 3. Architecture & Choix Techniques

Le projet respecte les principes de la **Clean Architecture** et du **SOLID** :

```
├── BattleShip.Models/       # Domaine métier pur (indépendant de toute techno web)
│   ├── Domain/              # Entités : Coordinate, Ship, Grid, GameEngine
│   ├── Enums/               # ShipType, Direction, GameState, CellState, ShotResult
│   ├── DTOs/                # Contrats d'échange REST (GameStatusDto, CellDto, etc.)
│   ├── AI/                  # Algorithmes de génération de tirs adverses
│   └── Privacy/             # GamePrivacyMapper (règles anti-triche strictes)
├── BattleShip.API/          # Backend Minimal API & gRPC (.NET 10)
│   ├── Grpc/                # GameGrpcService (service gRPC-Web)
│   ├── Validation/          # Validateurs FluentValidation
│   └── Services/            # GameStore singleton en mémoire
├── BattleShip.App/          # Client Blazor WebAssembly
│   ├── Pages/               # Home.razor (orchestrateur des écrans déploiement / combat)
│   ├── Components/          # BattleGrid, FleetStatusPanel, ShotLogPanel
│   └── Services/            # GameApiClient (REST) et OpponentGrpcClient (gRPC-Web)
├── BattleShip.Tests/        # Tests automatisés (xUnit, FluentValidation, bUnit)
└── Protos/                  # Contrat Protobuf partagé (game.proto)
```

---

## 4. Fonctionnalités Implémentées

### A. Moteur de Jeu & Intégrité du Domaine
- **Flotte réglementaire** : 5 navires aux tailles canoniques (Porte-avions : 5 cases, Cuirassé : 4 cases, Croiseur : 3 cases, Sous-marin : 3 cases, Torpilleur : 2 cases).
- **Règles d'invariants** : Détection géométrique stricte interdisant tout chevauchement ou débordement de grille (10x10).
- **Résolution des tirs** : Calcul déterministe (`Miss`, `Hit`, `Sunk`), identification immédiate de la destruction complète d'un navire.
- **Règles anti-triche** : Les positions des navires ennemis non découverts ne sont **jamais exposées** dans les DTOs renvoyés par l'API (`GamePrivacyMapper`).

### B. Validation Serveur & API REST
- **Endpoints REST documentés** :
  - `POST /games` : Création de partie (automatique aléatoire ou avec placement manuel).
  - `GET /games/{id}` : Récupération de l'état sécurisé de la partie.
  - `POST /games/{id}/shots` : Exécution d'un tir joueur.
- **Validation serveur (FluentValidation)** :
  - `CreateGameRequestValidator` : Valide la présence des 5 types distincts et leur intégrité.
  - `ShotRequestValidator` : Valide que les coordonnées transmises sont strictement bornées entre 0 et 9.
  - Retours normalisés en cas d'erreur (`400 ValidationProblem` ou `404 NotFound`).

### C. Contrat gRPC-Web
- Contrat défini dans `Protos/game.proto` avec la méthode `TakeOpponentShot`.
- Déclenché via gRPC-Web depuis le navigateur après un tir joueur pour piloter la riposte de l'IA adverse.
- Gestion explicite des erreurs gRPC (`StatusCode.NotFound`, `StatusCode.FailedPrecondition`).

### D. Expérience Utilisateur & Interface Tactique Blazor
- **Écran de Déploiement Tactique** :
  - Placement manuel des navires au clic avec retour visuel immédiat (cyan si valide, rouge si chevauchement/débordement).
  - Bascule d'orientation (Horizontal / Vertical) en temps réel.
  - Dock de navires interactif avec état unitaire (`✓ Placé` / `À placer`).
  - Boutons de pré-remplissage aléatoire rapide et de réinitialisation complète.
- **Centre de Commandement Naval (Combat)** :
  - Radar adverse avec tir direct réactif sans layout shift ni confirmation superflue.
  - Radar de flotte alliée affichant en temps réel les navires positionnés et les impacts subis.
  - Panneau comparateur d'intégrité des flottes en direct (navires opérationnels vs coulés).
  - Journal chronologique des tirs (horodatage, coordonnées, statut `TOUCHÉ / MANQUÉ / COULÉ`).
  - Détection et affichage explicite de la fin de partie (Victoire / Défaite) avec verrouillage des tirs et possibilité de rejouer instantanément.

---

## 5. Arbitrages du Backlog & Limites

### Fonctionnalités Priorisées
- **Finition et ergonomie** : Refonte visuelle complète (thème sombre tactique militaire) et élimination des frictions de jeu (tir direct réactif).
- **Placement de flotte flexible** : Support conjoint du déploiement manuel personnalisé et du placement aléatoire en un clic.
- **Fiabilité et couverture de tests** : 56 tests automatisés (couvrant la logique métier, la sécurité privacy, l'API et les composants UI via bUnit).
- **Rigueur architecturale** : Respect strict des contrats gRPC-Web et REST exigés par le référentiel.

### Fonctionnalités Écartées & Justifications
- **Multijoueur en ligne (SignalR / WebSockets)** : Écarté pour concentrer les efforts sur l'intégration gRPC-Web demandée au barème et garantir un mode solo contre l'IA d'une finition irréprochable.
- **IA prédictive par cartes de probabilités (heatmaps)** : L'IA adverse joue de façon aléatoire sur les cases non encore ciblées. Un algorithme de traque heuristique a été écarté pour privilégier la robustesse de l'échange gRPC et la clarté du flux de tour.
- **Animations 3D et effets sonores** : Écartés pour préserver des temps de chargement optimaux en WebAssembly et une compatibilité maximale sans dépendances externes lourdes.

---

## 6. Livrables d'Ingénierie & Preuves

- **Historique des échanges d'IA** : [PROMPTS.md](./PROMPTS.md)
- **Revues argumentées de propositions IA** : [REVUE-IA.md](./REVUE-IA.md) (5 revues détaillées avec hypothèses et vérifications)
- **Architecture Decision Records (ADR)** :
  - [ADR 0001 : Modélisation du domaine et intégrité de la grille](./docs/adr/0001-modele.md)
  - [ADR 0002 : Isolation des appels REST du frontend](./docs/adr/0002-client-http-frontend.md)
  - [ADR 0003 : Dédier une RPC au tir de l'adversaire](./docs/adr/0003-rpc-tir-adversaire.md)
- **Fichier de requêtes HTTP manuelles** : [api.http](./api.http)
