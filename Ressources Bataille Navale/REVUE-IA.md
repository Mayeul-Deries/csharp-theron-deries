**REVUE-IA.md — Revues de propositions IA**

**Revue 1 : client REST avant que l'API existe**

**Proposition examinée** : `GameApiClient.cs`, branché sur `POST /games`, `GET /games/{gameId}`, `POST /games/{gameId}/shots`, alors que le contrat n'est pas encore implémenté côté serveur.

**Hypothèse à vérifier** : le front peut être développé contre les DTO partagés sans casser quoi que ce soit une fois l'API branchée derrière.

**Expérience** : `dotnet test BattleShip.Tests\BattleShip.Tests.csproj --no-restore`. Si le projet compile et que les tests passent, ça veut dire que les DTO et les signatures sont cohérents entre les projets, ce qui est le point de rupture le plus probable à ce stade.

**Observation** : le projet compile, 22 tests réussis.

**Décision** : acceptée. Ça isole bien le transport de l'UI et ça ne verrouille rien côté serveur, Boris garde toute latitude sur l'implémentation de l'API.

**Preuves et limites** : commit `38207c5`, ADR 0002. Reste non vérifié à ce stade : l'API réelle, la configuration CORS, le comportement dans un vrai navigateur.

**Revue 2 : RPC TakeOpponentShot**

**Proposition examinée** : nouvelle RPC dans `game.proto` dédiée au tir de l'ordinateur, appelée automatiquement après un `Miss` côté joueur.

**Hypothèse à vérifier** : séparer `TakeShot` du joueur et `TakeOpponentShot` de l'IA évite toute ambiguïté sur qui tire à un instant donné, et n'introduit pas de régression sur l'existant.

**Expérience** : `dotnet test`. Si la génération protobuf et la compilation passent sans casser les tests déjà écrits sur `TakeShot`, le contrat proto est valide et n'a pas d'effet de bord.

**Observation** : génération protobuf réussie, 28 tests verts, y compris ceux déjà présents sur le tir joueur.

**Décision** : acceptée côté frontend. L'implémentation reste à charge de Boris côté API, donc pas encore testable de bout en bout.

**Preuves et limites** : `Protos/game.proto`, `OpponentGrpcClient.cs`, ADR 0003. Aucun appel gRPC-Web réel vérifié pour l'instant, faute d'API disponible.

**Revue 3 : refonte de l'UI et confidentialité des données adverses**

**Proposition examinée** : nouveau design tactique et `CellDto` enrichi d'un `ShipType?` optionnel pour l'affichage des navires côtés joueur et adversaire.

**Hypothèse à vérifier** : ajouter ce champ à `CellDto` ne doit pas permettre de déduire la position des navires adverses non touchés, c'est une règle explicite du sujet et donc le point le plus sensible de cette proposition.

**Expérience** : relecture manuelle du mapping dans `GamePrivacyMapper.cs`, complétée par `dotnet test` en portant une attention particulière à `PrivacyTests.cs`. Un test qui passerait à tort sur ce point masquerait une vraie fuite d'information.

**Observation** : 49 tests verts, les tests de `PrivacyTests.cs` confirment que les navires adverses vivants restent invisibles côté client. Vérification complémentaire à l'œil dans le navigateur, aucune case adverse ne révèle un type de navire avant d'être touchée.

**Décision** : acceptée. C'était le point qui inquiétait le plus dans cette proposition, donc celui vérifié en premier avant d'accepter le reste du changement.

**Preuves et limites** : commits `c3a621e`, `12e4daa`, `BattleShip.Tests/PrivacyTests.cs`.

**Revue 4 : correction du bug d'alias sur ShipType**

**Proposition examinée** : renumérotation de l'énumération `ShipType`, dans laquelle `Destroyer` et `Submarine` partageaient auparavant la même valeur numérique.

**Hypothèse à vérifier** : c'est bien cette collision de valeurs qui cause le bug observé, où placer un sous-marin marque aussi le croiseur comme placé, et non un problème de logique dans `Home.razor`.

**Expérience** : reproduction isolée du problème par comparaison directe `ShipType.Destroyer == ShipType.Submarine` avant correction, puis ajout d'un test de régression dans `EngineTests.cs` reproduisant le scénario de placement.

**Observation** : avant correction, la comparaison renvoie `true`, ce qui confirme la cause racine. Après renumérotation, 56/56 tests verts, y compris le nouveau test de régression. Vérification manuelle complémentaire en plaçant un sous-marin seul dans le navigateur, le croiseur reste bien marqué comme non placé.

**Décision** : acceptée. Le diagnostic est confirmé par un test reproduisant le défaut avant correction et validant la correction après, pas seulement par l'explication fournie par l'IA.

**Preuves et limites** : `ShipType.cs`, `Ship.cs`, `EngineTests.cs`. Correction ponctuelle, les autres énumérations du projet n'ont pas fait l'objet d'une revue systématique similaire.