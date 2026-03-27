using Blaze3SDK;
using BlazeCommon;
using NLog;
using Zamboni14Legacy.Components.NHL14Legacy.Bases;
using Zamboni14Legacy.Components.NHL14Legacy.Requests;
using Zamboni14Legacy.Components.NHL14Legacy.Responses;
using Zamboni14Legacy.Components.NHL14Legacy.Structs.Hut;
using Zamboni14Legacy.Server;

namespace Zamboni14Legacy.Components.NHL14Legacy;

internal class CardHouseComponent : CardHouseComponentBase.Server
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public override async Task<LoginResponse> LoginRequestAsync(LoginRequest request, BlazeRpcContext context)
    {
        var userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;
        var gamerInfo = await HutManager.GetGamerInfo(userId);
        if (gamerInfo == null) return new LoginResponse();
        return new LoginResponse
        {
            mTeamAbbreviation = gamerInfo.Value.mTeamAbbreviation,
            mBonusAwarded = 0,
            mTeamName = gamerInfo.Value.mTeamName,
            mRewardType = 0,
            mRewardValue = 0,
            mUserId = 0
        };
    }

    public override Task<NumericResponse> LogoutRequestAsync(LogoutRequest request, BlazeRpcContext context)
    {
        return Task.FromResult(new NumericResponse
        {
            mNumber = 0,
        });
    }

    public override async Task<MoveCardResponse> MoveCardAsync(MoveCardRequest request, BlazeRpcContext context)
    {
        long userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;
        CardData cardData = (await HutManager.GetCard(request.mCardId, userId)).Card;
        var versionInfo = await HutManager.GetVersionInfo(userId);
        switch (request.mDeckType)
        {
            case DeckType.CARDHOUSE_DECK_ESCROW:
                await HutCardFactory.CreateOrUpdateCard(cardData, userId, DeckType.CARDHOUSE_DECK_ESCROW);
                versionInfo = await HutManager.IncrementVersionInfo(userId, HutManager.VersionType.Escrow);
                break;
            default:
                throw new NotImplementedException();
        }

        return new MoveCardResponse
        {
            mDisplacedCardId = request.mCardId,
            mDisplacedDeckType = request.mDeckType,
            mDisplacedCardPosition = 0,
            mVersionInfo = versionInfo.Value
        };
    }

    public override async Task<GamerGetInfoResponse> GetGamerInfoRequestAsync(GamerGetInfoRequest request, BlazeRpcContext context)
    {
        var userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;
        var gamerInfo = await HutManager.GetGamerInfo(userId);
        if (gamerInfo == null) throw new BlazeRpcException(Blaze3RpcError.CARDHOUSE_ERR_NO_PLAYER_INFO);
        return new GamerGetInfoResponse
        {
            mGamerInfo = gamerInfo.Value,
            mUserId = 0
        };
    }

    public override async Task<NumericResponse> SetGamerInfoRequestAsync(GamerSetInfoRequest request, BlazeRpcContext context)
    {
        var userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;
        await HutManager.SetGamerInfo(request.mGamerInfo, userId);
        return new NumericResponse
        {
            mNumber = 0
        };
    }

    public override async Task<DeckInfoResponse> GetDeckInfoAsync(DeckInfoRequest request, BlazeRpcContext context)
    {
        var userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;

        var generalInfo = await HutManager.GetGeneralInfo(userId);
        if (generalInfo == null)
            generalInfo = await HutManager.SetGeneralInfo(new GeneralInfo
            {
                mCredits = 1000,
                mStats = new List<byte>()
            }, userId);

        var squadInfo = await HutManager.GetSquadInfo(userId);
        uint teamRating = 0;
        if (squadInfo != null) teamRating = squadInfo.Value.mStarRating;

        var versionInfo = await HutManager.GetVersionInfo(userId);
        if (versionInfo == null)
        {
            versionInfo = await HutManager.CreateVersionInfo(new VersionInfo
            {
                mVersionEscrow = 1,
                mVersionGeneral = 1,
                mVersionUnassigned = 1
            }, userId);
        }

        var escrowList = await HutManager.GetCardList(userId, DeckType.CARDHOUSE_DECK_ESCROW, CardState.CARDHOUSE_CARDSTATE_FREE);
        var unassignedList = await HutManager.GetCardList(userId, DeckType.CARDHOUSE_DECK_UNASSIGNED, CardState.CARDHOUSE_CARDSTATE_FREE);

        return new DeckInfoResponse
        {
            mDuplicateEscrowCardIdPairList = new List<CardIdPair>(),
            mDuplicateUnassignedCardIdPairList = new List<CardIdPair>(),
            mEscrowCardDataList = escrowList,
            mEscrowCount = (byte)escrowList.Count,
            mGeneralInfo = generalInfo.Value,
            mTeamRating = teamRating,
            mUnassignedCardDataList = unassignedList,
            mUserId = 0,
            mVersionInfo = versionInfo.Value
        };
    }

    public override Task<GetConfigResponse> GetConfigRequestAsync(ProvidedUID request, BlazeRpcContext context)
    {
        return Task.FromResult(new GetConfigResponse
        {
            mConfigList = new List<uint>
            {
                10, 20, 30, 40, 50, 60, 70, 80, 90, 100
            }
        });
    }

    public override Task<StoreGetPackTypesResponse> StoreGetPackTypesAsync(StoreGetPackTypesRequest request, BlazeRpcContext context)
    {
        return Task.FromResult(new StoreGetPackTypesResponse
        {
            mFreePack = 0,
            mPremiumPacksHidden = 0,
            mPackTypeList = new List<StorePackTypeData>()
            {
                new StorePackTypeData
                {
                    mAttributes = StorePackAttribute.CARDHOUSE_STOREPACKATTRIBUTES_SAVINGS_COINS,
                    mAvailability = StorePackAvailability.CARDHOUSE_STOREPACKAVAILABILITY_COINS,
                    mCoinCost = 1,
                    mEndDate = 0,
                    mId = StorePackId.CARDHOUSE_CARD_PACK_TYPE_PEEWEE,
                    mQuantity = 0,
                    mSaleType = StoreSaleType.CARDHOUSE_STORESALETYPE_NONE,
                    mStartDate = 0,
                    mState = StorePackState.CARDHOUSE_STOREPACKSTATE_ACTIVE
                }
            },
            mServerTime = Util.TimeNow()
        });
    }

    public override Task<GetSeasonConfigurationResponse> GetSeasonConfigurationAsync(SeasonDetailsRequest request, BlazeRpcContext context)
    {
        return Task.FromResult(new GetSeasonConfigurationResponse
        {
            mInstanceConfigList = new List<SeasonConfiguration>
            {
                new SeasonConfiguration
                {
                    mLeagueID = 1,
                    mLeagueName = "HUT Seasons",
                    mMemberType = MemberType.SEASONALPLAY_MEMBERTYPE_USER,
                    mSeasonID = 1,
                    mStatPeriodEnum = StatPeriod.STAT_PERIOD_ALLTIME,
                    mTeamID = 0,
                    mDivisionList = new List<Division>
                    {
                        new Division { mNumber = 1, mSize = 10, mTournamentRule = TournamentRule.SEASONALPLAY_TOURNAMENTRULE_UNLIMITED }
                    }
                }
            }
        });
    }

    public override Task<StorePackQuantitiesResponse> StorePackQuantitiesAsync(StorePackQuantitiesRequest request, BlazeRpcContext context)
    {
        return Task.FromResult(new StorePackQuantitiesResponse
        {
            mPackQuantitiesList = new List<int>
            {
                10, 20
            }
        });
    }

    public override async Task<DiscardCardResponse> DiscardCardAsync(DiscardCardRequest request, BlazeRpcContext context)
    {
        long userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;
        var cardData = await HutManager.GetCard(request.mCardId, userId);
        await HutManager.HardDelete(userId, cardData.Card.mCardId);
        var generalInfo = await HutManager.GetGeneralInfo(userId);
        await HutManager.SetGeneralInfo(new GeneralInfo
        {
            mCredits = request.mCredits + generalInfo.Value.mCredits,
            mStats = generalInfo.Value.mStats
        }, userId);

        switch (cardData.DeckType)
        {
            case DeckType.CARDHOUSE_DECK_ESCROW:
            {
                await HutManager.IncrementVersionInfo(userId, HutManager.VersionType.Escrow);
                break;
            }
            case DeckType.CARDHOUSE_DECK_UNASSIGNED:
            {
                await HutManager.IncrementVersionInfo(userId, HutManager.VersionType.Unassigned);
                break;
            }
        }

        var versionInfo = await HutManager.GetVersionInfo(userId);

        return new DiscardCardResponse
        {
            mCredits = request.mCredits,
            mVersionInfo = versionInfo.Value
        };
    }

    public override Task<StaffBonusResponse> GetStaffBonusAsync(ProvidedUID request, BlazeRpcContext context)
    {
        return Task.FromResult(new StaffBonusResponse
        {
            mStaffBonusInfo = new StaffBonusInfo
            {
                mPhysioArmBonus = 0,
                mPhysioBackBonus = 0,
                mContractBonus = 0,
                mFitnessBonus = 0,
                mPhysioFootBonus = 0,
                mGKDivingBonus = 0,
                mGKHandlingBonus = 0,
                mGKKickingBonus = 0,
                mGKOneOnOneBonus = 0,
                mGKPositioningBonus = 0,
                mGKReflexesBonus = 0,
                mPhysioHeadBonus = 0,
                mPhysioHipBonus = 0,
                mPhysioLegBonus = 0,
                mDefendingBonus = 0,
                mDribblingBonus = 0,
                mHeadingBonus = 0,
                mPaceBonus = 0,
                mPassingBonus = 0,
                mShootingBonus = 0,
                mPhysioShoulderBonus = 0,
                mManagerTalkBonus = 0
            }
        });
    }

    public override async Task<AssignCardsResponse> AssignCardsAsync(AssignCardsRequest request, BlazeRpcContext context)
    {
        var userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;
        foreach (var assignCardCard in request.mList)
        {
            CardData cardData = (await HutManager.GetCard(assignCardCard.mCardId)).Card;
            cardData.mCardStateId = assignCardCard.mCardStateId;
            await HutCardFactory.CreateOrUpdateCard(cardData, userId, assignCardCard.mDeckType);
        }

        await HutManager.IncrementVersionInfo(userId, HutManager.VersionType.Unassigned);
        var versionInfo = HutManager.GetVersionInfo(userId);
        return new AssignCardsResponse
        {
            mVersionInfo = versionInfo.Result.Value
        };
    }

    public override Task<UserReliabilityInfoResponse> GetUserReliabilityInfoAsync(ProvidedUID request, BlazeRpcContext context)
    {
        return Task.FromResult(new UserReliabilityInfoResponse
        {
            mPreviousMatchUnfinished = 0,
            mMatchesFinished = 10,
            mMatchesStarted = 10,
            mReliability = 0,
            mUserId = 0
        });
    }

    public override async Task<NumericResponse> ResetUserRequestAsync(ProvidedUID request, BlazeRpcContext context)
    {
        var userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;

        await HutManager.HardDelete(userId);
        return new NumericResponse
        {
            mNumber = 0,
        };
    }

    public override async Task<SquadListResponse> GetSquadListAsync(ProvidedUID request, BlazeRpcContext context)
    {
        var userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;
        var squadInfo = await HutManager.GetSquadInfo(userId);
        if (squadInfo == null) return new SquadListResponse();

        return new SquadListResponse
        {
            mActiveSquad = 1,
            mSquads = new List<SquadSmall>
            {
                new SquadSmall
                {
                    mChemistry = squadInfo.Value.mChemistry,
                    mFormation = squadInfo.Value.mFormationId,
                    mRating = squadInfo.Value.mStarRating,
                    mSquadId = 0,
                    mSquadName = squadInfo.Value.mSquadName
                }
            }
        };
    }

    public override async Task<ViewCardsResponse> ViewCardsAsync(ViewCardsRequest request, BlazeRpcContext context)
    {
        var cardDataList = new List<CardData>();
        foreach (var cardId in request.mCardIdList)
        {
            cardDataList.Add((await HutManager.GetCard(cardId)).Card);
        }

        return new ViewCardsResponse
        {
            mCardDataList = cardDataList
        };
    }

    public override async Task<SquadSaveResponse> SquadSaveAsync(SquadSaveRequest request, BlazeRpcContext context)
    {
        var userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;
        await HutManager.SetSquadInfo(request, userId);
        return new SquadSaveResponse
        {
            mSquadId = request.mSquadId
        };
    }

    public override async Task<StickerBookStats2Response> StickerBookStats2Async(StickerBookStats2Request request, BlazeRpcContext context)
    {
        long userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;

        List<StickerBookStatResult> stats = new();

        Logger.Debug($"stickerBookStats2: mContextId={(int)request.mContextId} ({request.mContextId}), mYearId={request.mYearId}, mValue={request.mValue}");

        var playerTypes = new[]
        {
            CardSubType.CARDHOUSE_CARD_TYPE_PLAYER_C,
            CardSubType.CARDHOUSE_CARD_TYPE_PLAYER_LW,
            CardSubType.CARDHOUSE_CARD_TYPE_PLAYER_RW,
            CardSubType.CARDHOUSE_CARD_TYPE_PLAYER_D,
            CardSubType.CARDHOUSE_CARD_TYPE_PLAYER_GK
        };

        if (request.mContextId == RequestContext.CARDHOUSE_STICKERBOOK_STATS_REQUEST_CONTEXT_TOP)
        {
            stats.Add(new StickerBookStatResult
            {
                mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                mContextValue = 2,
                mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_PLAYERS,
                mValue = await HutManager.GetCardCountAsync(userId, DeckType.CARDHOUSE_DECK_STICKERBOOK, playerTypes)
            });
            stats.Add(new StickerBookStatResult
            {
                mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                mContextValue = 2,
                mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_STAFF_HEADCOACH,
                mValue = await HutManager.GetCardCountAsync(userId, DeckType.CARDHOUSE_DECK_STICKERBOOK, CardSubType.CARDHOUSE_CARD_TYPE_STAFF_HEADCOACH)
            });
            stats.Add(new StickerBookStatResult
            {
                mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                mContextValue = 2,
                mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_STADIA,
                mValue = await HutManager.GetCardCountAsync(userId, DeckType.CARDHOUSE_DECK_STICKERBOOK, CardSubType.CARDHOUSE_CARD_TYPE_CUSTOM_STADIUM)
            });
            stats.Add(new StickerBookStatResult
            {
                mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                mContextValue = 2,
                mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_KITS,
                mValue = await HutManager.GetCardCountAsync(userId, DeckType.CARDHOUSE_DECK_STICKERBOOK, CardSubType.CARDHOUSE_CARD_TYPE_CUSTOM_KIT)
            });
            stats.Add(new StickerBookStatResult
            {
                mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                mContextValue = 2,
                mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_BADGES,
                mValue = await HutManager.GetCardCountAsync(userId, DeckType.CARDHOUSE_DECK_STICKERBOOK, CardSubType.CARDHOUSE_CARD_TYPE_CUSTOM_BADGE)
            });
        }

        if ((int)request.mContextId == 5)
        {
            Logger.Debug("stickerBookStats2: entering NEW_CARDS_SCREEN block");
            stats.Add(new StickerBookStatResult
            {
                mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                mContextValue = 2,
                mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_PLAYERS,
                mValue = await HutManager.GetCardCountAsync(userId, DeckType.CARDHOUSE_DECK_UNASSIGNED, playerTypes)
            });
            stats.Add(new StickerBookStatResult
            {
                mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                mContextValue = 2,
                mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_STAFF_HEADCOACH,
                mValue = await HutManager.GetCardCountAsync(userId, DeckType.CARDHOUSE_DECK_UNASSIGNED, CardSubType.CARDHOUSE_CARD_TYPE_STAFF_HEADCOACH)
            });
            stats.Add(new StickerBookStatResult
            {
                mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                mContextValue = 2,
                mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_STADIA,
                mValue = await HutManager.GetCardCountAsync(userId, DeckType.CARDHOUSE_DECK_UNASSIGNED, CardSubType.CARDHOUSE_CARD_TYPE_CUSTOM_STADIUM)
            });
            stats.Add(new StickerBookStatResult
            {
                mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                mContextValue = 2,
                mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_KITS,
                mValue = await HutManager.GetCardCountAsync(userId, DeckType.CARDHOUSE_DECK_UNASSIGNED, CardSubType.CARDHOUSE_CARD_TYPE_CUSTOM_KIT)
            });
            stats.Add(new StickerBookStatResult
            {
                mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                mContextValue = 2,
                mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_BADGES,
                mValue = await HutManager.GetCardCountAsync(userId, DeckType.CARDHOUSE_DECK_UNASSIGNED, CardSubType.CARDHOUSE_CARD_TYPE_CUSTOM_BADGE)
            });
            stats.Add(new StickerBookStatResult
            {
                mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                mContextValue = 2,
                mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_CONSUMABLES_CONTRACT,
                mValue = await HutManager.GetCardCountAsync(userId, DeckType.CARDHOUSE_DECK_UNASSIGNED, CardSubType.CARDHOUSE_CARD_TYPE_CONTRACT_PLAYER)
            });
            stats.Add(new StickerBookStatResult
            {
                mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                mContextValue = 2,
                mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_CONSUMABLES_TRAINING,
                mValue = await HutManager.GetCardCountAsync(userId, DeckType.CARDHOUSE_DECK_UNASSIGNED,
                    CardSubType.CARDHOUSE_CARD_TYPE_TRAINING_PLAYER_ATTRIBUTE_SKATING,
                    CardSubType.CARDHOUSE_CARD_TYPE_TRAINING_PLAYER_ATTRIBUTE_SHOOTING,
                    CardSubType.CARDHOUSE_CARD_TYPE_TRAINING_PLAYER_ATTRIBUTE_HANDS,
                    CardSubType.CARDHOUSE_CARD_TYPE_TRAINING_PLAYER_ATTRIBUTE_CHECKING,
                    CardSubType.CARDHOUSE_CARD_TYPE_TRAINING_PLAYER_ATTRIBUTE_DEFENSE,
                    CardSubType.CARDHOUSE_CARD_TYPE_TRAINING_PLAYER_ALL,
                    CardSubType.CARDHOUSE_CARD_TYPE_TRAINING_GK_ATTRIBUTE_HIGH,
                    CardSubType.CARDHOUSE_CARD_TYPE_TRAINING_GK_ATTRIBUTE_LOW,
                    CardSubType.CARDHOUSE_CARD_TYPE_TRAINING_GK_ATTRIBUTE_QUICKNESS,
                    CardSubType.CARDHOUSE_CARD_TYPE_TRAINING_GK_ATTRIBUTE_POSITIONING,
                    CardSubType.CARDHOUSE_CARD_TYPE_TRAINING_GK_ATTRIBUTE_REBOUNDCONTROL,
                    CardSubType.CARDHOUSE_CARD_TYPE_TRAINING_GK_ALL)
            });
        }

        if (request.mContextId == RequestContext.CARDHOUSE_STICKERBOOK_STATS_REQUEST_CONTEXT_YEAR)
        {
            foreach (var leagueId in HutCardFactory.LeagueTeamsMapping.Keys)
            {
                var correction = 0;
                if (leagueId is 0 or 1 or 2) correction = 2;
                if (leagueId == 3) correction = 1;
                var playerCounts = await HutManager.GetTeamCardCountsAsync(userId, leagueId, DeckType.CARDHOUSE_DECK_STICKERBOOK, playerTypes);

                stats.Add(new StickerBookStatResult
                {
                    mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                    mContextValue = leagueId,
                    mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_PLAYERS_BRONZE,
                    mValue = playerCounts.Values.Sum() + correction
                });

                var jerseyCounts = await HutManager.GetTeamCardCountsAsync(userId, leagueId, DeckType.CARDHOUSE_DECK_STICKERBOOK, CardSubType.CARDHOUSE_CARD_TYPE_CUSTOM_KIT);

                stats.Add(new StickerBookStatResult
                {
                    mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                    mContextValue = leagueId,
                    mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_KITS,
                    mValue = jerseyCounts.Values.Sum()
                });

                var badgeCounts = await HutManager.GetTeamCardCountsAsync(userId, leagueId, DeckType.CARDHOUSE_DECK_STICKERBOOK, CardSubType.CARDHOUSE_CARD_TYPE_CUSTOM_BADGE);

                stats.Add(new StickerBookStatResult
                {
                    mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                    mContextValue = leagueId,
                    mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_BADGES,
                    mValue = badgeCounts.Values.Sum()
                });
            }

            stats.Add(new StickerBookStatResult
            {
                mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                mContextValue = 12,
                mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_STADIA,
                mValue = await HutManager.GetCardCountAsync(userId, DeckType.CARDHOUSE_DECK_STICKERBOOK, CardSubType.CARDHOUSE_CARD_TYPE_CUSTOM_STADIUM)
            });

            stats.Add(new StickerBookStatResult
            {
                mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_YEAR,
                mContextValue = 13,
                mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_BALLS,
                mValue = await HutManager.GetCardCountAsync(userId, DeckType.CARDHOUSE_DECK_STICKERBOOK, CardSubType.CARDHOUSE_CARD_TYPE_STAFF_HEADCOACH)
            });
        }

        if (request.mContextId == RequestContext.CARDHOUSE_STICKERBOOK_STATS_REQUEST_CONTEXT_LEAGUE)
        {
            int leagueId = request.mValue;
            var teamPlayerCounts = await HutManager.GetTeamCardCountsAsync(userId, leagueId, DeckType.CARDHOUSE_DECK_STICKERBOOK, playerTypes);
            foreach (var teamId in teamPlayerCounts.Keys)
            {
                stats.Add(new StickerBookStatResult
                {
                    mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_TEAM,
                    mContextValue = teamId,
                    mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_PLAYERS,
                    mValue = teamPlayerCounts[teamId]
                });
            }

            var teamJerseyCounts = await HutManager.GetTeamCardCountsAsync(userId, leagueId, DeckType.CARDHOUSE_DECK_STICKERBOOK, CardSubType.CARDHOUSE_CARD_TYPE_CUSTOM_KIT);
            foreach (var teamId in teamJerseyCounts.Keys)
            {
                stats.Add(new StickerBookStatResult
                {
                    mContextId = ResultContext.CARDHOUSE_STICKERBOOK_STAT_RESULT_CONTEXT_TEAM,
                    mContextValue = teamId,
                    mTypeId = ResultType.CARDHOUSE_STICKERBOOK_STAT_RESULT_TYPE_KITS,
                    mValue = teamJerseyCounts[teamId]
                });
            }
        }

        Logger.Debug($"stickerBookStats2: returning {stats.Count} stats");
        return new StickerBookStats2Response { mStats = stats };
    }

    public override async Task<StickerBookSearchResponse> StickerBookSearchAsync(StickerBookSearchRequest request, BlazeRpcContext context)
    {
        long userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;

        List<CardData> cardDatas = await HutManager.GetCardList(userId, request);

        return new StickerBookSearchResponse
        {
            mSearchResults = cardDatas
        };
    }

    public override async Task<StickerBookCardResponse> StickerBookCardAsync(StickerBookCardRequest request, BlazeRpcContext context)
    {
        var userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mExternalId;
        var card = (await HutManager.GetCard(request.mCardId, (long)userId));
        await HutCardFactory.CreateOrUpdateCard(card.Card, (long)userId, DeckType.CARDHOUSE_DECK_STICKERBOOK);
        switch (card.DeckType)
        {
            case DeckType.CARDHOUSE_DECK_ESCROW: await HutManager.IncrementVersionInfo((long)userId, HutManager.VersionType.Escrow); break;
            case DeckType.CARDHOUSE_DECK_UNASSIGNED: await HutManager.IncrementVersionInfo((long)userId, HutManager.VersionType.Unassigned); break;
            default: throw new Exception();
        }
        var versionInfo = await HutManager.GetVersionInfo((long)userId);

        return new StickerBookCardResponse
        {
            mTotalCredits = 0,
            mVersionInfo = versionInfo.Value
        };
    }

    public override async Task<ISSearchResponse> ISSearchAsync(ISSearchRequest request, BlazeRpcContext context)
    {
        var userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mExternalId;
        return await HutTradeManager.SearchTradesAsync(request, (long)userId);
    }

    public override Task<ISWatchListResponse> ISWatchListAsync(ISWatchListRequest request, BlazeRpcContext context)
    {
        return Task.FromResult(new ISWatchListResponse
        {
            mTradeResults = new List<ISTradeInfo>(),
            mTotalCount = 0
        });
    }

    public override Task<ISWatchTradeResponse> ISWatchTradeAsync(ISWatchTradeRequest request, BlazeRpcContext context)
    {
        throw new NotImplementedException();
    }

    public override async Task<ISStartResponse> ISStartAsync(ISStartRequest request, BlazeRpcContext context)
    {
        ServerPlayer serverPlayer = ServerManager.GetServerPlayer(context.BlazeConnection);
        var tradeId = await HutTradeManager.InsertTrade(request, serverPlayer.UserIdentification.mAccountId, serverPlayer.UserIdentification.mName);

        return new ISStartResponse
        {
            mTradeId = tradeId
        };
    }

    public override async Task<ISOfferTradeResponse> ISOfferTradeAsync(ISOfferTradeRequest request, BlazeRpcContext context)
    {
        ServerPlayer serverPlayer = ServerManager.GetServerPlayer(context.BlazeConnection);
        var offerId = await HutTradeManager.InsertOffer(request, serverPlayer.UserIdentification.mAccountId);

        return new ISOfferTradeResponse
        {
            mOfferId = offerId
        };
    }

    public override async Task<ISViewTradeResponse> ISViewTradeAsync(ISViewTradeRequest request, BlazeRpcContext context)
    {
        var userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mExternalId;
        return await HutTradeManager.ViewTradeAsync(request, (long)userId);
    }

    public override Task<ISRemoveWatchResponse> ISRemoveWatchAsync(ISRemoveWatchRequest request, BlazeRpcContext context)
    {
        throw new BlazeRpcException(Blaze3RpcError.CARDHOUSE_ERR_UNKNOWN);
    }

    public override Task<ISAdminOfferResponse> ISAdminOfferAsync(ISAdminOfferRequest request, BlazeRpcContext context)
    {
        throw new NotImplementedException();
    }

    public override Task<ISGetOffersResponse> ISGetOffersAsync(ISGetOffersRequest request, BlazeRpcContext context)
    {
        throw new NotImplementedException();
    }

    public override Task<ActivateCardResponse> ActivateCardAsync(ActivateCardRequest request, BlazeRpcContext context)
    {
        return Task.FromResult(new ActivateCardResponse
        {
            mCardId = request.mCardId
        });
    }

    public override Task<ApplyCardResponse> ApplyCardAsync(ApplyCardRequest request, BlazeRpcContext context)
    {
        throw new NotImplementedException();
    }

    public override Task<ApplySalaryCapResponse> ApplySalaryCapAsync(ApplySalaryCapRequest request, BlazeRpcContext context)
    {
        return Task.FromResult(new ApplySalaryCapResponse
        {
            mPlayerCardId = request.mPlayerCardId,
            mSalaryCap = request.mSalaryCap,
            mUserId = request.mUserId
        });
    }

    public override Task<MatchRegisterStartResponse> MatchRegisterStartAsync(MatchRegisterStartRequest request, BlazeRpcContext context)
    {
        return Task.FromResult(new MatchRegisterStartResponse
        {
            mId = 0
        });
    }

    public override Task<NumericResponse> MatchRegisterFinishAsync(MatchRegisterFinishRequest request, BlazeRpcContext context)
    {
        return Task.FromResult(new NumericResponse
        {
        });
    }

    public override async Task<ChangePlayersResponse> ChangePlayersAsync(ChangePlayersRequest request, BlazeRpcContext context)
    {
        var userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;

        foreach (var loopVar in request.mCardDataList)
        {
            CardData cardData = (await HutManager.GetCard(loopVar.mCardId)).Card;
            cardData.mUsesRemaining--;
            cardData.mInjuryGames = loopVar.mInjuryGames;
            cardData.mInjuryType = loopVar.mInjuryType;
            cardData.mListStats = loopVar.mListStats;
            await HutCardFactory.CreateOrUpdateCard(cardData, userId);
        }

        return new ChangePlayersResponse();
    }

    public override async Task<PlayGameResponse> PlayGameAsync(PlayGameRequest request, BlazeRpcContext context)
    {
        var userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;
        var versionInfo = await HutManager.GetVersionInfo(userId);

        return new PlayGameResponse
        {
            mBonusAwarded = 10,
            mCredits = 10,
            mGoldenTickets = 10,
            mPrestige = 10,
            mTrophyCardCreated = 10,
            mVersionInfo = versionInfo.Value
        };
    }

    public override async Task<SquadLoadActiveResponse> SquadLoadActiveAsync(SquadLoadActiveRequest request, BlazeRpcContext context)
    {
        var userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;
        var squadInfo = await HutManager.GetSquadInfo(userId);
        if (squadInfo == null) throw new Exception();

        List<CardData> activeCards = new();
        activeCards.AddRange(await HutManager.GetCardList(userId, DeckType.CARDHOUSE_DECK_STICKERBOOK, CardState.CARDHOUSE_CARDSTATE_ACTIVE_BADGE));
        activeCards.AddRange(await HutManager.GetCardList(userId, DeckType.CARDHOUSE_DECK_STICKERBOOK, CardState.CARDHOUSE_CARDSTATE_ACTIVE_AWAY_KIT));
        activeCards.AddRange(await HutManager.GetCardList(userId, DeckType.CARDHOUSE_DECK_STICKERBOOK, CardState.CARDHOUSE_CARDSTATE_ACTIVE_HOME_KIT));
        activeCards.AddRange(await HutManager.GetCardList(userId, DeckType.CARDHOUSE_DECK_STICKERBOOK, CardState.CARDHOUSE_CARDSTATE_ACTIVE_STADIUM));

        return new SquadLoadActiveResponse
        {
            mActiveCards = activeCards,
            mSquadInfo = squadInfo.Value,
            mTargetUserId = (long)request.mTargetUserId
        };
    }

    public override async Task<CreatePackResponse> CreatePackAsync(CreatePackRequest request, BlazeRpcContext context)
    {
        long userId = ServerManager.GetServerPlayer(context.BlazeConnection).UserIdentification.mAccountId;
        var versionInfo = await HutManager.GetVersionInfo(userId);

        List<CardData> cardDataList = await HutPackFactory.CreatePack(userId, request.mPackType);

        return new CreatePackResponse
        {
            mCardDataList = cardDataList,
            mDuplicateCardIdPairList = new List<CardIdPair>(),
            mNumCards = (uint)cardDataList.Count,
            mNumPackPurchased = 0,
            mRandPackType = 0,
            mVersionInfo = versionInfo.Value
        };
    }
}
