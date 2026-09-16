using BattleShip.API.Protos;
using BattleShip.Models.Enums;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;

namespace BattleShip.App.Services;

/// <summary>
/// Calls the backend operation that executes the opponent turn.
/// </summary>
public sealed class OpponentGrpcClient : IOpponentGrpcClient, IDisposable
{
    private readonly GrpcChannel channel;
    private readonly GameService.GameServiceClient client;

    public OpponentGrpcClient()
    {
        var handler = new GrpcWebHandler(
            GrpcWebMode.GrpcWebText,
            new HttpClientHandler());

        channel = GrpcChannel.ForAddress(
            "https://localhost:7091",
            new GrpcChannelOptions { HttpHandler = handler });
        client = new GameService.GameServiceClient(channel);
    }

    public async Task<OpponentShotResult> TakeOpponentShotAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        var response = await client.TakeOpponentShotAsync(
            new OpponentShotRequest { GameId = gameId.ToString() },
            cancellationToken: cancellationToken);

        if (!Enum.TryParse<ShotResult>(response.Result, true, out var result))
        {
            throw new InvalidOperationException(
                $"Unknown opponent shot result '{response.Result}'.");
        }

        return new OpponentShotResult(result, response.Row, response.Col);
    }

    public void Dispose() => channel.Dispose();
}

public sealed record OpponentShotResult(ShotResult Result, int Row, int Col);
