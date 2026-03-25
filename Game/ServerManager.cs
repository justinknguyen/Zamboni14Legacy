using System.Collections.Concurrent;
using BlazeCommon;

namespace Zamboni14Legacy.Game;

public static class ServerManager
{
    private static readonly ConcurrentDictionary<ulong, ServerPlayer> ServerPlayers = new();
    private static readonly ConcurrentDictionary<ulong, QueuedPlayer> QueuedPlayers = new();
    private static readonly ConcurrentDictionary<uint, ServerGame> ServerGames = new();

    public static void AddServerPlayer(ServerPlayer serverPlayer)
    {
        var existing = GetServerPlayer(serverPlayer.UserIdentification.mName);
        if (existing != null) RemoveServerPlayer(existing);
        ServerPlayers[(ulong)serverPlayer.UserIdentification.mBlazeId] = serverPlayer;
    }

    public static void AddQueuedPlayer(QueuedPlayer queuedPlayer)
    {
        QueuedPlayers[(ulong)queuedPlayer.ServerPlayer.UserIdentification.mBlazeId] = queuedPlayer;
    }

    public static void AddServerGame(ServerGame serverGame)
    {
        ServerGames[(uint)serverGame.ReplicatedGameData.mGameId] = serverGame;
    }

    public static bool RemoveServerPlayer(ServerPlayer serverPlayer)
    {
        return ServerPlayers.TryRemove((ulong)serverPlayer.UserIdentification.mBlazeId, out _);
    }

    public static bool RemoveQueuedPlayer(QueuedPlayer queuedPlayer)
    {
        return QueuedPlayers.TryRemove((ulong)queuedPlayer.ServerPlayer.UserIdentification.mBlazeId, out _);
    }

    public static bool RemoveServerGame(ServerGame serverGame)
    {
        return ServerGames.TryRemove((uint)serverGame.ReplicatedGameData.mGameId, out _);
    }

    public static ICollection<ServerPlayer> GetServerPlayers()
    {
        return ServerPlayers.Values;
    }

    public static ICollection<QueuedPlayer> GetQueuedPlayers()
    {
        return QueuedPlayers.Values;
    }

    public static ICollection<ServerGame> GetServerGames()
    {
        return ServerGames.Values;
    }

    public static ServerPlayer? GetServerPlayer(BlazeServerConnection blazeServerConnection)
    {
        return ServerPlayers.Values.FirstOrDefault(serverPlayer => serverPlayer.BlazeServerConnection.Equals(blazeServerConnection));
    }

    public static ServerPlayer? GetServerPlayer(ProtoFireConnection protoFireConnection)
    {
        return ServerPlayers.Values.FirstOrDefault(serverPlayer => serverPlayer.BlazeServerConnection.ProtoFireConnection.Equals(protoFireConnection));
    }

    public static ServerPlayer? GetServerPlayer(uint userId)
    {
        return ServerPlayers.Values.FirstOrDefault(serverPlayer => serverPlayer.UserIdentification.mBlazeId.Equals(userId));
    }

    public static ServerPlayer? GetServerPlayer(string name)
    {
        return ServerPlayers.Values.FirstOrDefault(serverPlayer => serverPlayer.UserIdentification.mName.Equals(name));
    }

    public static ServerGame? GetServerGame(uint id)
    {
        return ServerGames.TryGetValue(id, out var game) ? game : null;
    }

    public static ServerGame? GetServerGame(ServerPlayer serverPlayer)
    {
        return ServerGames.Values.FirstOrDefault(serverGame => serverGame.ServerPlayers.Contains(serverPlayer));
    }

    public static QueuedPlayer? GetQueuedPlayer(ServerPlayer serverPlayer)
    {
        return QueuedPlayers.Values.FirstOrDefault(queuedPlayer => queuedPlayer.ServerPlayer.Equals(serverPlayer));
    }
}
