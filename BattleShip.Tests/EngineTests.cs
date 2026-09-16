using BattleShip.Models.Domain;
using BattleShip.Models.Enums;
using Xunit;

namespace BattleShip.Tests;

public class EngineTests
{
    [Fact]
    public void Ship_Creation_ShouldCalculateCorrectOccupiedCoordinates()
    {
        // Arrange
        var origin = new Coordinate(0, 0);
        
        // Act
        var ship = new Ship(ShipType.TorpedoBoat, origin, Direction.Horizontal); // Taille 2

        // Assert
        Assert.Equal(2, ship.OccupiedCoordinates.Count);
        Assert.Contains(new Coordinate(0, 0), ship.OccupiedCoordinates);
        Assert.Contains(new Coordinate(0, 1), ship.OccupiedCoordinates);
    }

    [Fact]
    public void Grid_AddShip_ShouldSucceed_WhenValid()
    {
        // Arrange
        var grid = new Grid();
        var ship = new Ship(ShipType.Submarine, new Coordinate(2, 2), Direction.Vertical);

        // Act
        var result = grid.TryAddShip(ship);

        // Assert
        Assert.True(result);
        Assert.Single(grid.Ships);
    }

    [Fact]
    public void Grid_AddShip_ShouldFail_WhenOverlapDetected()
    {
        // Arrange
        var grid = new Grid();
        var ship1 = new Ship(ShipType.Destroyer, new Coordinate(1, 1), Direction.Horizontal); // (1,1), (1,2), (1,3)
        var ship2 = new Ship(ShipType.TorpedoBoat, new Coordinate(0, 2), Direction.Vertical);   // (0,2), (1,2)

        grid.TryAddShip(ship1);

        // Act
        var result = grid.TryAddShip(ship2);

        // Assert
        Assert.False(result);
        Assert.Single(grid.Ships);
    }

    [Fact]
    public void Grid_AddShip_ShouldFail_WhenOutOfBounds()
    {
        // Arrange
        var grid = new Grid();
        // Le Porte-avions fait 5 cases, démarrer à la colonne 7 dépasse la grille 10x10 (indices 0 à 9)
        var ship = new Ship(ShipType.Carrier, new Coordinate(0, 7), Direction.Horizontal);

        // Act
        var result = grid.TryAddShip(ship);

        // Assert
        Assert.False(result);
        Assert.Empty(grid.Ships);
    }

    [Fact]
    public void GameEngine_TakeShot_ShouldReturnMiss_AndSwitchTurn_WhenWaterIsHit()
    {
        // Arrange
        var engine = new GameEngine();
        engine.SetupPlayerGridWithDefaultShips();
        engine.SetupOpponentGridWithDefaultShips();

        // (0,0) contient le Porte-avions. (1,0) est bien une case d'eau vide.
        var target = new Coordinate(1, 0);

        // Act
        var result = engine.TakeShot(target);

        // Assert
        Assert.Equal(ShotResult.Miss, result);
        Assert.Equal(GameState.OpponentTurn, engine.State); // Tir manqué -> Passe le tour
    }

    [Fact]
    public void GameEngine_TakeShot_ShouldReturnHit_AndKeepPlayerTurn_WhenShipIsHit()
    {
        // Arrange
        var engine = new GameEngine();
        var shipTarget = new Coordinate(3, 3);
        
        // On place manuellement un navire connu sur la grille adverse
        engine.OpponentGrid.TryAddShip(new Ship(ShipType.TorpedoBoat, shipTarget, Direction.Horizontal));
        engine.StartGame();

        // Act
        var result = engine.TakeShot(shipTarget);

        // Assert
        Assert.Equal(ShotResult.Hit, result);
        Assert.Equal(GameState.PlayerTurn, engine.State); // Tir touché -> Le joueur rejoue !
    }

    [Fact]
    public void GameEngine_TakeShot_ShouldDetectSunk_WhenAllShipPartsAreHit()
    {
        // Arrange
        var engine = new GameEngine();
        var origin = new Coordinate(5, 5);
        // Torpilleur occupe (5,5) et (5,6)
        engine.OpponentGrid.TryAddShip(new Ship(ShipType.TorpedoBoat, origin, Direction.Horizontal));
        engine.StartGame();

        // Act
        engine.TakeShot(new Coordinate(5, 5)); // Premier tir (Touché)
        var lastShotResult = engine.TakeShot(new Coordinate(5, 6)); // Second tir (Coulé)

        // Assert
        Assert.Equal(ShotResult.Sunk, lastShotResult);
    }

    [Fact]
    public void GameEngine_TakeShot_ShouldPreventDuplicateShotsOnSameCoordinate()
    {
        // Arrange
        var engine = new GameEngine();
        engine.StartGame();
        var target = new Coordinate(2, 2);

        engine.TakeShot(target);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => engine.TakeShot(target));
    }

    [Fact]
    public void GameEngine_TakeShot_ShouldTransitionToPlayerWon_WhenAllOpponentShipsAreSunk()
    {
        // Arrange
        var engine = new GameEngine();
        // Place un seul petit bateau pour l'adversaire pour simplifier
        var target = new Coordinate(0, 0);
        engine.OpponentGrid.TryAddShip(new Ship(ShipType.TorpedoBoat, target, Direction.Horizontal)); // (0,0) et (0,1)
        engine.StartGame();

        // Act
        engine.TakeShot(new Coordinate(0, 0));
        var result = engine.TakeShot(new Coordinate(0, 1));

        // Assert
        Assert.Equal(ShotResult.Sunk, result);
        Assert.Equal(GameState.PlayerWon, engine.State);
    }

    [Fact]
    public void GameEngine_TakeOpponentShot_ShouldTransitionToOpponentWon_WhenAllPlayerShipsAreSunk()
    {
        // Arrange
        var engine = new GameEngine();
        // Place un seul petit bateau pour le joueur
        var target = new Coordinate(0, 0);
        engine.PlayerGrid.TryAddShip(new Ship(ShipType.TorpedoBoat, target, Direction.Horizontal)); // (0,0) et (0,1)
        engine.StartGame();
        
        // On force le tour à OpponentTurn pour que l'IA puisse tirer
        // Nous allons simuler le tour de l'IA en passant un tir manqué du joueur
        engine.TakeShot(new Coordinate(9, 9)); // Tir dans l'eau -> OpponentTurn
        Assert.Equal(GameState.OpponentTurn, engine.State);

        // Act
        engine.TakeOpponentShot(new Coordinate(0, 0));
        var result = engine.TakeOpponentShot(new Coordinate(0, 1));

        // Assert
        Assert.Equal(ShotResult.Sunk, result);
        Assert.Equal(GameState.OpponentWon, engine.State);
    }

    [Fact]
    public void GameEngine_TakeOpponentShot_ShouldReturnMiss_AndSwitchToPlayerTurn_WhenWaterIsHit()
    {
        // Arrange
        var engine = new GameEngine();
        engine.SetupPlayerGridWithDefaultShips();
        engine.StartGame();
        
        // Joueur rate pour passer le tour à l'adversaire
        engine.TakeShot(new Coordinate(9, 9)); 

        // Act
        var result = engine.TakeOpponentShot(new Coordinate(9, 9)); // L'IA tire dans l'eau chez le joueur

        // Assert
        Assert.Equal(ShotResult.Miss, result);
        Assert.Equal(GameState.PlayerTurn, engine.State);
    }

    [Fact]
    public void GameEngine_TakeShot_ShouldThrowException_WhenGameIsAlreadyWon()
    {
        // Arrange
        var engine = new GameEngine();
        engine.OpponentGrid.TryAddShip(new Ship(ShipType.TorpedoBoat, new Coordinate(0, 0), Direction.Horizontal));
        engine.StartGame();

        // Coulons le bateau pour gagner la partie
        engine.TakeShot(new Coordinate(0, 0));
        engine.TakeShot(new Coordinate(0, 1));
        Assert.Equal(GameState.PlayerWon, engine.State);

        // Act & Assert : Impossible de tirer après la victoire
        Assert.Throws<InvalidOperationException>(() => engine.TakeShot(new Coordinate(5, 5)));
    }

    [Fact]
    public void GameEngine_TakeOpponentShot_ShouldReturnMiss_AndGiveTurnToPlayer()
    {
        var engine = new GameEngine();
        engine.PlayerGrid.TryAddShip(new Ship(ShipType.TorpedoBoat, new Coordinate(0, 0), Direction.Horizontal));
        engine.OpponentGrid.TryAddShip(new Ship(ShipType.TorpedoBoat, new Coordinate(0, 0), Direction.Horizontal));
        engine.StartGame();
        engine.TakeShot(new Coordinate(1, 0));

        var result = engine.TakeOpponentShot(new Coordinate(9, 9));

        Assert.Equal(ShotResult.Miss, result);
        Assert.Equal(GameState.PlayerTurn, engine.State);
    }

    [Fact]
    public void GameEngine_TakeOpponentShot_ShouldRejectCallOutsideOpponentTurn()
    {
        var engine = new GameEngine();

        Assert.Throws<InvalidOperationException>(() =>
            engine.TakeOpponentShot(new Coordinate(0, 0)));
    }

}