using System.Timers;
using Blaze3SDK;
using Blaze3SDK.Blaze;
using Blaze3SDK.Blaze.GameManager;
using Blaze3SDK.Components;
using BlazeCommon;
using NLog;
using Timer = System.Timers.Timer;

namespace Zamboni14Legacy.Components.Blaze;

internal class GameManager : GameManagerBase.Server
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static readonly Timer Timer;

    static GameManager()
    {
        Timer = new Timer(5000);
        Timer.Elapsed += OnTimedEvent;
        Timer.AutoReset = true;
        Timer.Enabled = true;
    }

    private static void OnTimedEvent(object sender, ElapsedEventArgs e)
    {
        foreach (var serverGame in ServerManager.GetServerGames().ToList()) // How to not fix bugs
            if (serverGame.ServerPlayers.Count == 0)
            {
                ServerManager.RemoveServerGame(serverGame);
                foreach (var serverPlayer in ServerManager.GetServerPlayers())
                    NotifyGameRemovedAsync(serverPlayer.BlazeServerConnection, new NotifyGameRemoved
                    {
                        mDestructionReason = GameDestructionReason.HOST_LEAVING,
                        mGameId = serverGame.ReplicatedGameData.mGameId
                    });
            }

        var time = (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        foreach (var serverPlayer in ServerManager.GetServerPlayers()) // How to not fix bugs pt2
        {
            if (serverPlayer.LastPingedTime == 0) continue;
            if (serverPlayer.LastPingedTime + 3600 >= time) continue;
            if (serverPlayer.BlazeServerConnection != null)
                UserSessionsBase.Server.NotifyUserSessionDisconnectedAsync(serverPlayer.BlazeServerConnection, new UserSessionDisconnectReason
                {
                    mDisconnectReason = UserSessionDisconnectReason.DisconnectReason.DUPLICATE_LOGIN
                });

            ServerManager.RemoveServerPlayer(serverPlayer);
        }
    }

    public override Task<StartMatchmakingResponse> StartMatchmakingAsync(StartMatchmakingRequest request, BlazeRpcContext context)
    {
        var serverPlayer = ServerManager.GetServerPlayer(context.BlazeConnection);
        var gameModeRule = request.mCriteriaData.mGenericRulePrefsList
            .FirstOrDefault(p => p.mRuleName == "OSDK_gameMode");
        var gameMode = gameModeRule.mRuleName != null
            ? gameModeRule.mDesiredValues?.FirstOrDefault() ?? "1"
            : "1";

        if (gameMode != "3")
            throw new BlazeRpcException(Blaze3RpcError.ERR_COMMAND_NOT_FOUND);

        var queuedPlayer = new QueuedPlayer(serverPlayer, request);

        // Look for an existing OTP game in PRE_GAME with space
        var existingGame = ServerManager.GetServerGames()
            .FirstOrDefault(g =>
                g.ReplicatedGameData.mGameAttribs.TryGetValue("OSDK_gameMode", out var m) && m == "3" &&
                g.HasSpaceForPlayer() &&
                g.ReplicatedGameData.mGameState == GameState.PRE_GAME);

        if (existingGame != null)
        {
            Task.Run(async () =>
            {
                await Task.Delay(100);
                existingGame.AddGameParticipant(serverPlayer, queuedPlayer.MatchmakingSessionId);

                var lobbies = GetLobbies();
                foreach (var sp in ServerManager.GetServerPlayers().ToList())
                    NotifyGameListUpdateAsync(sp.BlazeServerConnection, new NotifyGameListUpdate
                    {
                        mIsFinalUpdate = 1,
                        mListId = 1,
                        mUpdatedGames = lobbies
                    });
            });
        }
        else
        {
            // Build game attribs from the matchmaking criteria
            var attribs = new SortedDictionary<string, string> { { "OSDK_gameMode", "3" } };
            foreach (var rule in request.mCriteriaData.mGenericRulePrefsList)
            {
                var val = rule.mDesiredValues?.FirstOrDefault();
                if (val != null && val != "abstain")
                    attribs[rule.mRuleName] = val;
            }

            var createRequest = new CreateGameRequest
            {
                mGameAttribs = attribs,
                mGameProtocolVersionString = request.mGameProtocolVersionString,
                mGameSettings = request.mGameSettings,
                mIgnoreEntryCriteriaWithInvite = request.mIgnoreEntryCriteriaWithInvite,
                mMaxPlayerCapacity = request.mMaxPlayerCapacity,
                mNetworkTopology = request.mNetworkTopology,
                mPresenceMode = PresenceMode.PRESENCE_MODE_STANDARD,
                mQueueCapacity = request.mQueueCapacity,
                mSlotCapacities = new List<ushort> { 12, 0 },
                mTeamCapacity = 0,
                mVoipNetwork = VoipTopology.VOIP_DISABLED,
                mPersistedGameIdSecret = new byte[] { },
                mHostNetworkAddressList = new List<NetworkAddress> { serverPlayer.ExtendedData.mAddress }
            };

            var serverGame = new ServerGame(serverPlayer, createRequest);

            Task.Run(async () =>
            {
                await Task.Delay(100);
                serverGame.AddGameParticipant(serverPlayer, queuedPlayer.MatchmakingSessionId);

                var lobbies = GetLobbies();
                foreach (var sp in ServerManager.GetServerPlayers().ToList())
                    NotifyGameListUpdateAsync(sp.BlazeServerConnection, new NotifyGameListUpdate
                    {
                        mIsFinalUpdate = 1,
                        mListId = 1,
                        mUpdatedGames = lobbies
                    });
            });
        }

        return Task.FromResult(new StartMatchmakingResponse
        {
            mSessionId = queuedPlayer.MatchmakingSessionId
        });
    }

    public override Task<NullStruct> CancelMatchmakingAsync(CancelMatchmakingRequest request, BlazeRpcContext context)
    {
        var serverPlayer = ServerManager.GetServerPlayer(context.BlazeConnection);
        var queuedPlayer = ServerManager.GetQueuedPlayer(serverPlayer);
        if (queuedPlayer != null) ServerManager.RemoveQueuedPlayer(queuedPlayer);
        NotifyMatchmakingFailedAsync(context.BlazeConnection, new NotifyMatchmakingFailed
        {
            mMatchmakingResult = MatchmakingResult.SESSION_TERMINATED,
            mMaxPossibleFitScore = 0
            // mSessionId = queuedPlayer.MatchmakingSessionId,
            // mUserSessionId = (uint)serverPlayer.SessionInfo.mBlazeUserId
        });
        return Task.FromResult(new NullStruct());
    }

    public override Task<JoinGameResponse> JoinGameAsync(JoinGameRequest request, BlazeRpcContext context)
    {
        var serverGame = ServerManager.GetServerGame(request.mGameId);
        var serverPlayer = ServerManager.GetServerPlayer(context.BlazeConnection);

        if (!serverGame.HasSpaceForPlayer()) throw new Exception();

        if (serverGame == null || serverPlayer == null) throw new Exception();

        Task.Run(async () =>
        {
            await Task.Delay(50);
            NotifyMatchmakingAsyncStatusAsync(context.BlazeConnection, new NotifyMatchmakingAsyncStatus
            {
                mMatchmakingAsyncStatusList = new List<MatchmakingAsyncStatus>
                {
                    new()
                    {
                        mHostBalanceRuleStatus = new HostBalanceRuleStatus
                        {
                            mMatchedHostBalanceValue = HostBalanceRuleStatus.HostBalanceValues.HOSTS_UNBALANCED
                        }
                    }
                },
                mMatchmakingSessionId = 0,
                mUserSessionId = (uint)serverPlayer.UserIdentification.mAccountId
            });
        });

        Task.Run(async () =>
        {
            await Task.Delay(100);
            serverGame.AddGameParticipant(serverPlayer);
        });

        return Task.FromResult(new JoinGameResponse
        {
            mGameId = request.mGameId,
            mJoinState = JoinState.JOINED_GAME
        });
    }

    private static List<GameBrowserMatchData> GetLobbies()
    {
        var lobbies = new List<GameBrowserMatchData>();
        foreach (var serverGame in ServerManager.GetServerGames())
        {
            if (serverGame.ReplicatedGameData.mGameState != GameState.PRE_GAME &&
                serverGame.ReplicatedGameData.mGameState != GameState.INITIALIZING) continue;

            if (!serverGame.HasSpaceForPlayer()) continue;

            var participants = new List<GameBrowserPlayerData>();
            foreach (var gamePlayer in serverGame.ReplicatedGamePlayers)
                participants.Add(new GameBrowserPlayerData
                {
                    mAccountLocale = gamePlayer.mAccountLocale,
                    mExternalId = gamePlayer.mExternalId,
                    mPlayerAttribs = gamePlayer.mPlayerAttribs,
                    mPlayerId = gamePlayer.mPlayerId,
                    mPlayerName = gamePlayer.mPlayerName,
                    mPlayerState = gamePlayer.mPlayerState,
                    mTeamIndex = gamePlayer.mTeamIndex
                });

            var gameMode = serverGame.ReplicatedGameData.mGameAttribs.TryGetValue("OSDK_gameMode", out var mode) ? mode : "1";
            var teamCapacity = gameMode == "3" ? (ushort)6 : (ushort)1;

            // Build team info vector with actual player counts
            var teamInfo = new List<GameBrowserTeamInfo>();
            if (gameMode == "3")
            {
                // OTP mode: teams 0 (home) and 1 (away)
                var team0Count = (ushort)serverGame.ReplicatedGamePlayers.Count(p => p.mTeamIndex == 0);
                var team1Count = (ushort)serverGame.ReplicatedGamePlayers.Count(p => p.mTeamIndex == 1);

                teamInfo.Add(new GameBrowserTeamInfo
                {
                    mTeamId = 0,
                    mTeamSize = team0Count
                });
                teamInfo.Add(new GameBrowserTeamInfo
                {
                    mTeamId = 1,
                    mTeamSize = team1Count
                });
            }

            lobbies.Add(new GameBrowserMatchData
            {
                mFitScore = 1,
                mGameData = new GameBrowserGameData
                {
                    mAdminPlayerList = serverGame.ReplicatedGameData.mAdminPlayerList,
                    mEntryCriteriaMap = serverGame.ReplicatedGameData.mEntryCriteriaMap,
                    mExternalSessionId = 1,
                    mGameAttribs = serverGame.ReplicatedGameData.mGameAttribs,
                    mGameBrowserTeamInfoVector = teamInfo,
                    mGameId = serverGame.ReplicatedGameData.mGameId,
                    mGameName = serverGame.ReplicatedGameData.mGameName,
                    mGameProtocolVersionString = serverGame.ReplicatedGameData.mGameProtocolVersionString,
                    mGameRoster = participants,
                    mGameSettings = serverGame.ReplicatedGameData.mGameSettings,
                    mGameState = serverGame.ReplicatedGameData.mGameState,
                    mHostId = serverGame.ReplicatedGameData.mTopologyHostInfo.mPlayerId,
                    mHostNetworkAddressList = serverGame.ReplicatedGameData.mHostNetworkAddressList,
                    mNetworkTopology = serverGame.ReplicatedGameData.mNetworkTopology,
                    mPersistedGameId = serverGame.ReplicatedGameData.mPersistedGameId,
                    mPingSiteAlias = "qos",
                    mPlayerCounts = new List<ushort>
                    {
                        (ushort)serverGame.ReplicatedGamePlayers.Count(p => p.mTeamIndex == 0),
                        (ushort)serverGame.ReplicatedGamePlayers.Count(p => p.mTeamIndex == 1)
                    },
                    mPresenceMode = serverGame.ReplicatedGameData.mPresenceMode,
                    mQueueCapacity = serverGame.ReplicatedGameData.mQueueCapacity,
                    mQueueCount = serverGame.ReplicatedGameData.mQueueCapacity,
                    mSlotCapacities = serverGame.ReplicatedGameData.mSlotCapacities,
                    mTeamCapacity = teamCapacity,
                    mVoipTopology = VoipTopology.VOIP_DISABLED
                }
            });
        }

        return lobbies;
    }

    public override Task<GetGameListResponse> GetGameListSubscriptionAsync(GetGameListRequest request, BlazeRpcContext context)
    {
        var lobbies = GetLobbies();

        Task.Run(async () =>
        {
            await Task.Delay(100);
            NotifyGameListUpdateAsync(context.BlazeConnection, new NotifyGameListUpdate
            {
                mIsFinalUpdate = 1,
                mListId = 1,
                // mRemovedGameList = null, Not sure should we use this
                mUpdatedGames = lobbies
            });
        });


        return Task.FromResult(new GetGameListResponse
        {
            mListId = 1,
            mMaxPossibleFitScore = 1
        });
    }

    public override Task<NullStruct> DestroyGameListAsync(DestroyGameListRequest request, BlazeRpcContext context)
    {
        return Task.FromResult(new NullStruct());
    }

    public override Task<NullStruct> FinalizeGameCreationAsync(UpdateGameSessionRequest request, BlazeRpcContext context)
    {
        var serverGame = ServerManager.GetServerGame(request.mGameId);
        if (serverGame == null) return Task.FromResult(new NullStruct());

        var replicatedGameData = serverGame.ReplicatedGameData;
        replicatedGameData.mXnetNonce = request.mXnetNonce;
        replicatedGameData.mXnetSession = request.mXnetSession;

        serverGame.ReplicatedGameData = replicatedGameData;

        foreach (var serverPlayer in serverGame.ServerPlayers)
            NotifyGameSessionUpdatedAsync(serverPlayer.BlazeServerConnection, new GameSessionUpdatedNotification
            {
                mGameId = request.mGameId,
                mXnetNonce = request.mXnetNonce,
                mXnetSession = request.mXnetSession
            });

        // Advance game state from INITIALIZING to PRE_GAME so clients leave the loading screen
        replicatedGameData.mGameState = GameState.PRE_GAME;
        serverGame.ReplicatedGameData = replicatedGameData;

        foreach (var serverPlayer in serverGame.ServerPlayers)
            NotifyGameStateChangeAsync(serverPlayer.BlazeServerConnection, new NotifyGameStateChange
            {
                mGameId = request.mGameId,
                mNewGameState = GameState.PRE_GAME
            });

        return Task.FromResult(new NullStruct());
    }

    public override Task<NullStruct> AdvanceGameStateAsync(AdvanceGameStateRequest request, BlazeRpcContext context)
    {
        var serverGame = ServerManager.GetServerGame(request.mGameId);
        if (serverGame == null) return Task.FromResult(new NullStruct());

        var replicatedGameData = serverGame.ReplicatedGameData;
        replicatedGameData.mGameState = request.mNewGameState;

        serverGame.ReplicatedGameData = replicatedGameData;

        foreach (var serverPlayer in serverGame.ServerPlayers)
            NotifyGameStateChangeAsync(serverPlayer.BlazeServerConnection, new NotifyGameStateChange
            {
                mGameId = request.mGameId,
                mNewGameState = request.mNewGameState
            });
        return Task.FromResult(new NullStruct());
    }


    public override Task<NullStruct> SetPlayerAttributesAsync(SetPlayerAttributesRequest request, BlazeRpcContext context)
    {
        var zamboniGame = ServerManager.GetServerGame(request.mGameId);
        var serverPlayer = ServerManager.GetServerPlayer((uint)request.mPlayerId);

        foreach (var participant in zamboniGame.ServerPlayers)
            NotifyPlayerAttribChangeAsync(participant.BlazeServerConnection, new NotifyPlayerAttribChange
            {
                mGameId = zamboniGame.ReplicatedGameData.mGameId,
                mPlayerAttribs = request.mPlayerAttributes,
                mPlayerId = request.mPlayerId
            });

        // REQ attribute signals the player has entered the pre-game lobby.
        // Transition them to ACTIVE_CONNECTED so the side select UI becomes interactive.
        if (request.mPlayerAttributes.ContainsKey("REQ") && serverPlayer != null)
        {
            zamboniGame.NotifyParticipants(new NotifyGamePlayerStateChange
            {
                mGameId = (uint)zamboniGame.ReplicatedGameData.mGameId,
                mPlayerId = (long)request.mPlayerId,
                mPlayerState = PlayerState.ACTIVE_CONNECTED
            });
            zamboniGame.NotifyParticipants(new NotifyPlayerJoinCompleted
            {
                mGameId = (uint)zamboniGame.ReplicatedGameData.mGameId,
                mPlayerId = (long)request.mPlayerId
            });
        }

        return Task.FromResult(new NullStruct());
    }

    public override Task<NullStruct> UpdateMeshConnectionAsync(UpdateMeshConnectionRequest request, BlazeRpcContext context)
    {
        var serverGame = ServerManager.GetServerGame(request.mGameId);
        if (serverGame == null) return Task.FromResult(new NullStruct());

        foreach (var playerConnectionStatus in request.mMeshConnectionStatusList)
            switch (playerConnectionStatus.mPlayerNetConnectionStatus)
            {
                case PlayerNetConnectionStatus.CONNECTED:
                {
                    var statePacket = new NotifyGamePlayerStateChange
                    {
                        mGameId = request.mGameId,
                        mPlayerId = playerConnectionStatus.mTargetPlayer,
                        mPlayerState = PlayerState.ACTIVE_CONNECTED
                    };
                    serverGame.NotifyParticipants(statePacket);

                    var joinCompletedPacket = new NotifyPlayerJoinCompleted
                    {
                        mGameId = request.mGameId,
                        mPlayerId = playerConnectionStatus.mTargetPlayer
                    };
                    serverGame.NotifyParticipants(joinCompletedPacket);
                    break;
                }
                case PlayerNetConnectionStatus.ESTABLISHING_CONNECTION:
                {
                    var statePacket = new NotifyGamePlayerStateChange
                    {
                        mGameId = request.mGameId,
                        mPlayerId = playerConnectionStatus.mTargetPlayer,
                        mPlayerState = PlayerState.ACTIVE_CONNECTING
                    };
                    serverGame.NotifyParticipants(statePacket);
                    break;
                }
                case PlayerNetConnectionStatus.DISCONNECTED:
                {
                    var serverPlayer = ServerManager.GetServerPlayer((uint)playerConnectionStatus.mTargetPlayer);
                    serverGame.RemoveGameParticipant(serverPlayer, PlayerRemovedReason.PLAYER_CONN_LOST);
                    break;
                }
                default:
                    Logger.Debug("Unknown player connection status: " + playerConnectionStatus.mPlayerNetConnectionStatus);
                    break;
            }

        return Task.FromResult(new NullStruct());
    }

    public override Task<NullStruct> RemovePlayerAsync(RemovePlayerRequest request, BlazeRpcContext context)
    {
        var serverGame = ServerManager.GetServerGame(request.mGameId);
        var serverPlayer = ServerManager.GetServerPlayer((uint)request.mPlayerId);

        if (serverGame == null || serverPlayer == null) return Task.FromResult(new NullStruct());

        serverGame.RemoveGameParticipant(serverPlayer, request.mPlayerRemovedReason);
        
        //Hack fix
        Task.Run(async () =>
        {
            await Task.Delay(100);
            UserSessionsBase.Server.NotifyUserSessionDisconnectedAsync(context.BlazeConnection, new UserSessionDisconnectReason
            {
                mDisconnectReason = UserSessionDisconnectReason.DisconnectReason.DUPLICATE_LOGIN
            });
        });


        // var lobbies = GetLobbies();
        // Task.Run(async () =>
        // {
        //     await Task.Delay(100);
        //
        //     NotifyGameListUpdateAsync(context.BlazeConnection, new NotifyGameListUpdate
        //     {
        //         mIsFinalUpdate = 1,
        //         mListId = 1,
        //         // mRemovedGameList = null, Not sure should we use this
        //         mUpdatedGames = lobbies
        //     });
        // });
        return Task.FromResult(new NullStruct());
    }

    public override Task<NullStruct> UpdateGameSessionAsync(UpdateGameSessionRequest request, BlazeRpcContext context)
    {
        var serverGame = ServerManager.GetServerGame(request.mGameId);
        if (serverGame == null) return Task.FromResult(new NullStruct());

        var replicatedGameData = serverGame.ReplicatedGameData;
        replicatedGameData.mXnetNonce = request.mXnetNonce;
        replicatedGameData.mXnetSession = request.mXnetSession;

        serverGame.ReplicatedGameData = replicatedGameData;

        foreach (var serverPlayer in serverGame.ServerPlayers)
            NotifyGameSessionUpdatedAsync(serverPlayer.BlazeServerConnection, new GameSessionUpdatedNotification
            {
                mGameId = request.mGameId,
                mXnetNonce = request.mXnetNonce,
                mXnetSession = request.mXnetSession
            });
        return Task.FromResult(new NullStruct());
    }


    public override Task<NullStruct> SetGameSettingsAsync(SetGameSettingsRequest request, BlazeRpcContext context)
    {
        var serverGame = ServerManager.GetServerGame(request.mGameId);
        if (serverGame == null) return Task.FromResult(new NullStruct());

        var replicatedGameData = serverGame.ReplicatedGameData;
        replicatedGameData.mGameSettings = request.mGameSettings;

        serverGame.ReplicatedGameData = replicatedGameData;

        foreach (var serverPlayer in serverGame.ServerPlayers)
            NotifyGameSettingsChangeAsync(serverPlayer.BlazeServerConnection, new NotifyGameSettingsChange
            {
                mGameSettings = request.mGameSettings,
                mGameId = request.mGameId
            });
        return Task.FromResult(new NullStruct());
    }

    public override Task<NullStruct> SetPlayerTeamAsync(SetPlayerTeamRequest request, BlazeRpcContext context)
    {
        var serverGame = ServerManager.GetServerGame(request.mGameId);
        if (serverGame == null) return Task.FromResult(new NullStruct());

        var playerIndex = serverGame.ReplicatedGamePlayers.FindIndex(p => p.mPlayerId == request.mPlayerId);
        if (playerIndex >= 0)
        {
            var player = serverGame.ReplicatedGamePlayers[playerIndex];
            player.mTeamIndex = request.mTeamIndex;
            serverGame.ReplicatedGamePlayers[playerIndex] = player;

            foreach (var serverPlayer in serverGame.ServerPlayers)
                NotifyGamePlayerTeamChangeAsync(serverPlayer.BlazeServerConnection, new NotifyGamePlayerTeamChange
                {
                    mGameId = request.mGameId,
                    mPlayerId = request.mPlayerId,
                    mTeamIndex = request.mTeamIndex
                });
        }

        return Task.FromResult(new NullStruct());
    }

    public override Task<NullStruct> SwapPlayersTeamAsync(SwapPlayersTeamRequest request, BlazeRpcContext context)
    {
        var serverGame = ServerManager.GetServerGame(request.mGameId);
        if (serverGame == null) return Task.FromResult(new NullStruct());

        foreach (var swap in request.mSwapPlayersTeam)
        {
            var playerIndex = serverGame.ReplicatedGamePlayers.FindIndex(p => p.mPlayerId == swap.mPlayerId);
            if (playerIndex < 0) continue;

            var player = serverGame.ReplicatedGamePlayers[playerIndex];
            player.mTeamIndex = swap.mTeamIndex;
            serverGame.ReplicatedGamePlayers[playerIndex] = player;

            foreach (var serverPlayer in serverGame.ServerPlayers)
                NotifyGamePlayerTeamChangeAsync(serverPlayer.BlazeServerConnection, new NotifyGamePlayerTeamChange
                {
                    mGameId = request.mGameId,
                    mPlayerId = swap.mPlayerId,
                    mTeamIndex = swap.mTeamIndex
                });
        }

        return Task.FromResult(new NullStruct());
    }

    public override Task<JoinGameResponse> ResetDedicatedServerAsync(CreateGameRequest request, BlazeRpcContext context)
    {
        var host = ServerManager.GetServerPlayer(context.BlazeConnection);
        var serverGame = new ServerGame(host, request);
        
        Task.Run(async () =>
        {
            await Task.Delay(100);
            serverGame.AddGameParticipant(host);
            var lobbies = GetLobbies();

            foreach (var serverPlayer in ServerManager.GetServerPlayers().ToList())
                NotifyGameListUpdateAsync(serverPlayer.BlazeServerConnection, new NotifyGameListUpdate
                {
                    mIsFinalUpdate = 1,
                    mListId = 1,
                    mUpdatedGames = lobbies
                });
        });

        return Task.FromResult(new JoinGameResponse
        {
            mGameId = (uint)serverGame.ReplicatedGameData.mGameId,
            mJoinState = JoinState.JOINED_GAME
        });
    }

    public override Task<CreateGameResponse> CreateGameAsync(CreateGameRequest request, BlazeRpcContext context)
    {
        var host = ServerManager.GetServerPlayer(context.BlazeConnection);
        var serverGame = new ServerGame(host, request);
        Task.Run(async () =>
        {
            await Task.Delay(100);
            serverGame.AddGameParticipant(host);
            var lobbies = GetLobbies();

            foreach (var serverPlayer in ServerManager.GetServerPlayers().ToList())
                NotifyGameListUpdateAsync(serverPlayer.BlazeServerConnection, new NotifyGameListUpdate
                {
                    mIsFinalUpdate = 1,
                    mListId = 1,
                    // mRemovedGameList = null, Not sure should we use this
                    mUpdatedGames = lobbies
                });
        });
        return Task.FromResult(new CreateGameResponse
        {
            mGameId = serverGame.ReplicatedGameData.mGameId
        });
    }
}