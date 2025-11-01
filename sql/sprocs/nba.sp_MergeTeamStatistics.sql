create or alter procedure nba.sp_MergeTeamStatistics (
  @Teams            nba.tt_Merge_TeamSeason             readonly
, @Players          nba.tt_Merge_Player                 readonly
, @Roster           nba.tt_Merge_TeamSeasonRoster       readonly
, @PlayerStatistics nba.tt_Merge_PlayerSeasonStatistics readonly )
as
begin
  set nocount on;

  --
  -- MERGE: TeamSeason
  --
  merge nba.TeamSeason as target
  using ( select s.Id SeasonId
               , TeamIdentifier
               , Wins
               , Losses
               , MarginOfVictory
               , OffensiveRating
               , DefensiveRating
               , WinsLeagueRank
               , LossesLeagueRank
               , MarginOfVictoryLeagueRank
               , OffensiveRatingLeagueRank
               , DefensiveRatingLeagueRank
          from   @Teams          t
                 join nba.Season s on s.StartYear = t.SeasonStartYear ) as source ( SeasonId, TeamIdentifier, Wins, Losses, MarginOfVictory, OffensiveRating, DefensiveRating, WinsLeagueRank, LossesLeagueRank, MarginOfVictoryLeagueRank, OffensiveRatingLeagueRank, DefensiveRatingLeagueRank )
  on ( target.SeasonId = source.SeasonId
   and target.TeamIdentifier = source.TeamIdentifier )
  when matched then update set Wins = source.Wins
                             , Losses = source.Losses
                             , MarginOfVictory = source.MarginOfVictory
                             , OffensiveRating = source.OffensiveRating
                             , DefensiveRating = source.DefensiveRating
                             , WinsLeagueRank = source.WinsLeagueRank
                             , LossesLeagueRank = source.LossesLeagueRank
                             , MarginOfVictoryLeagueRank = source.MarginOfVictoryLeagueRank
                             , OffensiveRatingLeagueRank = source.OffensiveRatingLeagueRank
                             , DefensiveRatingLeagueRank = source.DefensiveRatingLeagueRank
  when not matched then insert ( SeasonId
                               , TeamIdentifier
                               , Wins
                               , Losses
                               , MarginOfVictory
                               , OffensiveRating
                               , DefensiveRating
                               , WinsLeagueRank
                               , LossesLeagueRank
                               , MarginOfVictoryLeagueRank
                               , OffensiveRatingLeagueRank
                               , DefensiveRatingLeagueRank )
                        values
                             ( source.SeasonId, source.TeamIdentifier, source.Wins, source.Losses, source.MarginOfVictory, source.OffensiveRating, source.DefensiveRating, source.WinsLeagueRank, source.LossesLeagueRank, source.MarginOfVictoryLeagueRank, source.OffensiveRatingLeagueRank, source.DefensiveRatingLeagueRank );
  --
  -- MERGE: Players
  --
  merge nba.Player as target
  using ( select Identifier
               , Name
               , BirthDate
               , BirthCountry
               , College
          from   @Players ) as source ( Identifier, Name, BirthDate, BirthCountry, College )
  on ( target.Identifier = source.Identifier )
  when not matched then insert ( Identifier
                               , Name
                               , BirthDate
                               , BirthCountry
                               , College )
                        values
                             ( source.Identifier, source.Name, source.BirthDate, source.BirthCountry, source.College );

  --
  -- MERGE: Roster
  --
  merge nba.TeamSeasonRoster as target
  using ( select ts.Id                  TeamSeasonId
               , p.Id                   PlayerId
               , r.JerseyNumber         JerseyNumber
               , r.Position             Position
               , r.Height               Height
               , r.Weight               Weight
               , r.NumberOfYearInLeague NumberOfYearInLeague
          from   @Roster             r
                 join nba.Player     p on p.Identifier = r.PlayerIdentifier

                 join nba.Season     s on s.StartYear = r.SeasonStartYear

                 join nba.TeamSeason ts on ts.SeasonId = s.Id
                                       and ts.TeamIdentifier = r.TeamIdentifier ) as source ( TeamSeasonId, PlayerId, JerseyNumber, Position, Height, Weight, NumberOfYearInLeague )
  on ( target.TeamSeasonId = source.TeamSeasonId
   and target.PlayerId = source.PlayerId )
  when matched then update set JerseyNumber = source.JerseyNumber
                             , Position = source.Position
                             , Height = source.Height
                             , Weight = source.Weight
                             , NumberOfYearInLeague = source.NumberOfYearInLeague
  when not matched then insert ( TeamSeasonId
                               , PlayerId
                               , JerseyNumber
                               , Position
                               , Height
                               , Weight
                               , NumberOfYearInLeague )
                        values
                             ( source.TeamSeasonId, source.PlayerId, source.JerseyNumber, source.Position, source.Height, source.Weight, source.NumberOfYearInLeague );

  --
  -- MERGE: Player statistics
  --
  merge nba.PlayerSeasonStatistics as target
  using ( select ts.Id TeamSeasonId
               , p.Id  PlayerId
               , ps.ForRegularSeason
               , ps.GamesPlayed
               , ps.MinutesPlayed
               , ps.MinutedPlayedPerGame
               , ps.FieldGoalsMade
               , ps.FieldGoalsAttempted
               , ps.FieldGoalPercentage
               , ps.ThreePointersMade
               , ps.ThreePointersAttempted
               , ps.ThreePointersPercentage
               , ps.TwoPointersMade
               , ps.TwoPointersAttempted
               , ps.TwoPointersPercentage
               , ps.FreeThrowsMade
               , ps.FreeThrowsAttempted
               , ps.FreeThrowsPercentage
               , ps.EffectiveFieldGoalPercentage
               , ps.OffensiveRebounds
               , ps.DefensiveRebounds
               , ps.TotalRebounds
               , ps.ReboundsPerGame
               , ps.Assists
               , ps.AssistsPerGame
               , ps.Steals
               , ps.StealsPerGame
               , ps.Blocks
               , ps.BlocksPerGame
               , ps.Turnovers
               , ps.TurnoversPerGame
               , ps.PersonalFouls
               , ps.PersonalFoulsPerGame
               , ps.Points
               , ps.PointsPerGame
               , ps.PlusMinusOnCourt
               , ps.PlusMinusNetOnCourt
          from   @PlayerStatistics   ps
                 join nba.Season     s on s.StartYear = ps.SeasonStartYear

                 join nba.TeamSeason ts on ts.SeasonId = s.Id
                                       and ts.TeamIdentifier = ps.TeamIdentifier

                 join nba.Player     p on p.Identifier = ps.PlayerIdentifier ) as source
  on ( target.TeamSeasonId = source.TeamSeasonId
   and target.PlayerId = source.PlayerId )
  when matched then update set GamesPlayed = source.GamesPlayed
                             , MinutesPlayed = source.MinutesPlayed
                             , MinutedPlayedPerGame = source.MinutedPlayedPerGame
                             , FieldGoalsMade = source.FieldGoalsMade
                             , FieldGoalsAttempted = source.FieldGoalsAttempted
                             , FieldGoalPercentage = source.FieldGoalPercentage
                             , ThreePointersMade = source.ThreePointersMade
                             , ThreePointersAttempted = source.ThreePointersAttempted
                             , ThreePointersPercentage = source.ThreePointersPercentage
                             , TwoPointersMade = source.TwoPointersMade
                             , TwoPointersAttempted = source.TwoPointersAttempted
                             , TwoPointersPercentage = source.TwoPointersPercentage
                             , FreeThrowsMade = source.FreeThrowsMade
                             , FreeThrowsAttempted = source.FreeThrowsAttempted
                             , FreeThrowsPercentage = source.FreeThrowsPercentage
                             , EffectiveFieldGoalPercentage = source.EffectiveFieldGoalPercentage
                             , OffensiveRebounds = source.OffensiveRebounds
                             , DefensiveRebounds = source.DefensiveRebounds
                             , TotalRebounds = source.TotalRebounds
                             , ReboundsPerGame = source.ReboundsPerGame
                             , Assists = source.Assists
                             , AssistsPerGame = source.AssistsPerGame
                             , Steals = source.Steals
                             , StealsPerGame = source.StealsPerGame
                             , Blocks = source.Blocks
                             , BlocksPerGame = source.BlocksPerGame
                             , Turnovers = source.Turnovers
                             , TurnoversPerGame = source.TurnoversPerGame
                             , PersonalFouls = source.PersonalFouls
                             , PersonalFoulsPerGame = source.PersonalFoulsPerGame
                             , Points = source.Points
                             , PointsPerGame = source.PointsPerGame
                             , PlusMinusOnCourt = source.PlusMinusOnCourt
                             , PlusMinusNetOnCourt = source.PlusMinusNetOnCourt
  when not matched then insert ( TeamSeasonId
                               , PlayerId
                               , ForRegularSeason
                               , GamesPlayed
                               , MinutesPlayed
                               , MinutedPlayedPerGame
                               , FieldGoalsMade
                               , FieldGoalsAttempted
                               , FieldGoalPercentage
                               , ThreePointersMade
                               , ThreePointersAttempted
                               , ThreePointersPercentage
                               , TwoPointersMade
                               , TwoPointersAttempted
                               , TwoPointersPercentage
                               , FreeThrowsMade
                               , FreeThrowsAttempted
                               , FreeThrowsPercentage
                               , EffectiveFieldGoalPercentage
                               , OffensiveRebounds
                               , DefensiveRebounds
                               , TotalRebounds
                               , ReboundsPerGame
                               , Assists
                               , AssistsPerGame
                               , Steals
                               , StealsPerGame
                               , Blocks
                               , BlocksPerGame
                               , Turnovers
                               , TurnoversPerGame
                               , PersonalFouls
                               , PersonalFoulsPerGame
                               , Points
                               , PointsPerGame
                               , PlusMinusOnCourt
                               , PlusMinusNetOnCourt )
                        values
                             ( source.TeamSeasonId, source.PlayerId, source.ForRegularSeason, source.GamesPlayed, source.MinutesPlayed, source.MinutedPlayedPerGame, source.FieldGoalsMade, source.FieldGoalsAttempted, source.FieldGoalPercentage, source.ThreePointersMade, source.ThreePointersAttempted, source.ThreePointersPercentage, source.TwoPointersMade, source.TwoPointersAttempted, source.TwoPointersPercentage, source.FreeThrowsMade, source.FreeThrowsAttempted, source.FreeThrowsPercentage, source.EffectiveFieldGoalPercentage, source.OffensiveRebounds, source.DefensiveRebounds, source.TotalRebounds, source.ReboundsPerGame, source.Assists, source.AssistsPerGame, source.Steals, source.StealsPerGame, source.Blocks, source.BlocksPerGame, source.Turnovers, source.TurnoversPerGame, source.PersonalFouls, source.PersonalFoulsPerGame, source.Points, source.PointsPerGame, source.PlusMinusOnCourt, source.PlusMinusNetOnCourt );

end;
go

grant execute on nba.sp_MergeTeamStatistics to roleNba;
go