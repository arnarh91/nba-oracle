create or alter procedure nba.sp_MergeGameBoxScores
  @Games                   nba.tt_Merge_GameBoxScore               readonly
, @Quarters                nba.tt_Merge_GameQuarterScore           readonly
, @DidNotPlay              nba.tt_Merge_GameDidNotPlay             readonly
, @PlayerBasicBoxScores    nba.tt_Merge_GamePlayerBasicBoxScore    readonly
, @PlayerAdvancedBoxScores nba.tt_Merge_GamePlayerAdvancedBoxScore readonly
, @TeamBoxScores           nba.tt_Merge_GameTeamBoxScore           readonly
as
begin
  set nocount on;

  -- Merge GameBoxScore
  merge nba.GameBoxScore as target
  using ( select g.GameId GameId
          from   @Games g ) as source
  on target.GameId = source.GameId
  when not matched by target then insert ( GameId )
                                  values
                                       ( source.GameId );

  -- Merge GameQuarterScore
  merge nba.GameQuarterScore as target
  using ( select q.GameBoxScoreId
               , q.QuarterNumber
               , q.QuarterLabel
               , q.HomeScore
               , q.AwayScore
          from   @Quarters q ) as source
  on target.GameBoxScoreId = source.GameBoxScoreId
 and target.QuarterNumber = source.QuarterNumber
  when matched then update set QuarterLabel = source.QuarterLabel
                             , HomeScore = source.HomeScore
                             , AwayScore = source.AwayScore
  when not matched by target then insert ( GameBoxScoreId
                                         , QuarterNumber
                                         , QuarterLabel
                                         , HomeScore
                                         , AwayScore )
                                  values
                                       ( source.GameBoxScoreId, source.QuarterNumber, source.QuarterLabel, source.HomeScore, source.AwayScore );

  -- Merge GameTeamBoxScore (Four Factors)
  merge nba.GameTeamBoxScore as target
  using ( select tbs.GameBoxScoreId
               , tbs.TeamIdentifier
               , tbs.Pace
               , tbs.Efg
               , tbs.Tov
               , tbs.Orb
               , tbs.Ftfga
               , tbs.Ortg
          from   @TeamBoxScores tbs ) as source
  on target.GameBoxScoreId = source.GameBoxScoreId
 and target.TeamIdentifier = source.TeamIdentifier
  when matched then update set Pace = source.Pace
                             , Efg = source.Efg
                             , Tov = source.Tov
                             , Orb = source.Orb
                             , Ftfga = source.Ftfga
                             , Ortg = source.Ortg
  when not matched by target then insert ( GameBoxScoreId
                                         , TeamIdentifier
                                         , Pace
                                         , Efg
                                         , Tov
                                         , Orb
                                         , Ftfga
                                         , Ortg )
                                  values
                                       ( source.GameBoxScoreId, source.TeamIdentifier, source.Pace, source.Efg, source.Tov, source.Orb, source.Ftfga, source.Ortg );

  -- Merge DidNotPlay
  merge nba.GameDidNotPlay as target
  using ( select d.GameBoxScoreId
               , d.TeamIdentifier
               , p.Id PlayerId
               , d.Reason
          from   @DidNotPlay               d
                 join nba.Season           s on s.StartYear = d.SeasonStartYear

                 join nba.TeamSeason       ts on ts.SeasonId = s.Id
                                             and ts.TeamIdentifier = d.TeamIdentifier

                 join nba.TeamSeasonRoster tsr on tsr.TeamSeasonId = ts.Id

                 join nba.Player           p on p.Id = tsr.PlayerId
                                            and lower( p.Name ) = lower( d.PlayerName )) as source
  on target.GameBoxScoreId = source.GameBoxScoreId
 and target.PlayerId = source.PlayerId
  when matched then update set Reason = source.Reason
  when not matched by target then insert ( GameBoxScoreId
                                         , TeamIdentifier
                                         , PlayerId
                                         , Reason )
                                  values
                                       ( source.GameBoxScoreId, source.TeamIdentifier, source.PlayerId, source.Reason );

  -- Merge GamePlayerBasicBoxScore
  merge nba.GamePlayerBasicBoxScore as target
  using ( select pbs.GameBoxScoreId
               , p.Id PlayerId
               , pbs.TeamIdentifier
               , pbs.Starter
               , pbs.SecondsPlayed
               , pbs.FieldGoalsMade
               , pbs.FieldGoalsAttempted
               , pbs.ThreePointersMade
               , pbs.ThreePointersAttempted
               , pbs.FreeThrowsMade
               , pbs.FreeThrowsAttempted
               , pbs.OffensiveRebounds
               , pbs.DefensiveRebounds
               , pbs.TotalRebounds
               , pbs.Assists
               , pbs.Steals
               , pbs.Blocks
               , pbs.Turnovers
               , pbs.PersonalFouls
               , pbs.Points
               , pbs.GameScore
               , pbs.PlusMinusScore
          from   @PlayerBasicBoxScores     pbs
                 join nba.GameBoxScore     gbs on gbs.GameId = pbs.GameBoxScoreId

                 join nba.Game             g on g.Id = gbs.GameId

                 join nba.Season           s on s.Id = g.SeasonId

                 join nba.TeamSeason       ts on ts.SeasonId = s.Id
                                             and ts.TeamIdentifier = pbs.TeamIdentifier

                 join nba.TeamSeasonRoster tsr on tsr.TeamSeasonId = ts.Id

                 join nba.Player           p on p.Id = tsr.PlayerId
                                            and lower( p.Name ) = lower( pbs.PlayerName )) as source
  on target.GameBoxScoreId = source.GameBoxScoreId
 and target.PlayerId = source.PlayerId
  when matched then update set TeamIdentifier = source.TeamIdentifier
                             , Starter = source.Starter
                             , SecondsPlayed = source.SecondsPlayed
                             , FieldGoalsMade = source.FieldGoalsMade
                             , FieldGoalsAttempted = source.FieldGoalsAttempted
                             , ThreePointersMade = source.ThreePointersMade
                             , ThreePointersAttempted = source.ThreePointersAttempted
                             , FreeThrowsMade = source.FreeThrowsMade
                             , FreeThrowsAttempted = source.FreeThrowsAttempted
                             , OffensiveRebounds = source.OffensiveRebounds
                             , DefensiveRebounds = source.DefensiveRebounds
                             , TotalRebounds = source.TotalRebounds
                             , Assists = source.Assists
                             , Steals = source.Steals
                             , Blocks = source.Blocks
                             , Turnovers = source.Turnovers
                             , PersonalFouls = source.PersonalFouls
                             , Points = source.Points
                             , GameScore = source.GameScore
                             , PlusMinusScore = source.PlusMinusScore
  when not matched by target then insert ( GameBoxScoreId
                                         , PlayerId
                                         , TeamIdentifier
                                         , Starter
                                         , SecondsPlayed
                                         , FieldGoalsMade
                                         , FieldGoalsAttempted
                                         , ThreePointersMade
                                         , ThreePointersAttempted
                                         , FreeThrowsMade
                                         , FreeThrowsAttempted
                                         , OffensiveRebounds
                                         , DefensiveRebounds
                                         , TotalRebounds
                                         , Assists
                                         , Steals
                                         , Blocks
                                         , Turnovers
                                         , PersonalFouls
                                         , Points
                                         , GameScore
                                         , PlusMinusScore )
                                  values
                                       ( source.GameBoxScoreId, source.PlayerId, source.TeamIdentifier, source.Starter, source.SecondsPlayed, source.FieldGoalsMade, source.FieldGoalsAttempted, source.ThreePointersMade, source.ThreePointersAttempted, source.FreeThrowsMade, source.FreeThrowsAttempted, source.OffensiveRebounds, source.DefensiveRebounds, source.TotalRebounds, source.Assists, source.Steals, source.Blocks, source.Turnovers, source.PersonalFouls, source.Points, source.GameScore, source.PlusMinusScore );

  -- Merge GamePlayerAdvancedBoxScore
  merge nba.GamePlayerAdvancedBoxScore as target
  using ( select pabs.GameBoxScoreId
               , p.Id PlayerId
               , pabs.TeamIdentifier
               , pabs.TrueShootingPercentage
               , pabs.EffectiveFieldGoalPercentage
               , pabs.ThreePointAttemptRate
               , pabs.FreeThrowsAttemptRate
               , pabs.OffensiveReboundPercentage
               , pabs.DefensiveReboundPercentage
               , pabs.TotalReboundPercentage
               , pabs.AssistPercentage
               , pabs.StealPercentage
               , pabs.BlockPercentage
               , pabs.TurnoverPercentage
               , pabs.UsagePercentage
               , pabs.OffensiveRating
               , pabs.DefensiveRating
               , pabs.BoxPlusMinus
          from   @PlayerAdvancedBoxScores  pabs
                 join nba.GameBoxScore     gbs on gbs.GameId = pabs.GameBoxScoreId

                 join nba.Game             g on g.Id = gbs.GameId

                 join nba.Season           s on s.Id = g.SeasonId

                 join nba.TeamSeason       ts on ts.SeasonId = s.Id
                                             and ts.TeamIdentifier = pabs.TeamIdentifier

                 join nba.TeamSeasonRoster tsr on tsr.TeamSeasonId = ts.Id

                 join nba.Player           p on p.Id = tsr.PlayerId
                                            and lower( p.Name ) = lower( pabs.PlayerName )) as source
  on target.GameBoxScoreId = source.GameBoxScoreId
 and target.PlayerId = source.PlayerId
  when matched then update set TeamIdentifier = source.TeamIdentifier
                             , TrueShootingPercentage = source.TrueShootingPercentage
                             , EffectiveFieldGoalPercentage = source.EffectiveFieldGoalPercentage
                             , ThreePointAttemptRate = source.ThreePointAttemptRate
                             , FreeThrowsAttemptRate = source.FreeThrowsAttemptRate
                             , OffensiveReboundPercentage = source.OffensiveReboundPercentage
                             , DefensiveReboundPercentage = source.DefensiveReboundPercentage
                             , TotalReboundPercentage = source.TotalReboundPercentage
                             , AssistPercentage = source.AssistPercentage
                             , StealPercentage = source.StealPercentage
                             , BlockPercentage = source.BlockPercentage
                             , TurnoverPercentage = source.TurnoverPercentage
                             , UsagePercentage = source.UsagePercentage
                             , OffensiveRating = source.OffensiveRating
                             , DefensiveRating = source.DefensiveRating
                             , BoxPlusMinus = source.BoxPlusMinus
  when not matched by target then insert ( GameBoxScoreId
                                         , PlayerId
                                         , TeamIdentifier
                                         , TrueShootingPercentage
                                         , EffectiveFieldGoalPercentage
                                         , ThreePointAttemptRate
                                         , FreeThrowsAttemptRate
                                         , OffensiveReboundPercentage
                                         , DefensiveReboundPercentage
                                         , TotalReboundPercentage
                                         , AssistPercentage
                                         , StealPercentage
                                         , BlockPercentage
                                         , TurnoverPercentage
                                         , UsagePercentage
                                         , OffensiveRating
                                         , DefensiveRating
                                         , BoxPlusMinus )
                                  values
                                       ( source.GameBoxScoreId, source.PlayerId, source.TeamIdentifier, source.TrueShootingPercentage, source.EffectiveFieldGoalPercentage, source.ThreePointAttemptRate, source.FreeThrowsAttemptRate, source.OffensiveReboundPercentage, source.DefensiveReboundPercentage, source.TotalReboundPercentage, source.AssistPercentage, source.StealPercentage, source.BlockPercentage, source.TurnoverPercentage, source.UsagePercentage, source.OffensiveRating, source.DefensiveRating, source.BoxPlusMinus );

end;
go

grant execute on nba.sp_MergeGameBoxScores to roleNba;
go