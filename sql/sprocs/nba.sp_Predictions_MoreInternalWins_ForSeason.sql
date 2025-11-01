create or alter procedure nba.sp_Predictions_MoreInternalWins_ForSeason (
  @SeasonStartYear int
, @SeasonsBack     int
, @Description     varchar (256))
as
begin
  set nocount on;

  declare @SeasonId             int
        , @SeasonsBackStartDate date;

  select @SeasonId = Id
  from   nba.Season
  where  StartYear = @SeasonStartYear;

  if ( @SeasonId is null )
  begin
    ; throw 50001, 'Season not found', 1;
  end;

  select @SeasonsBackStartDate = RegularSeasonStartDate
  from   nba.Season
  where  StartYear = @SeasonStartYear - @SeasonsBack;

  if ( @SeasonsBackStartDate is null )
  begin
    ; throw 50001, 'SeasonsBack not found', 1;
  end;

  declare @PredictionModelId int = ( select Id
                                     from   nba.PredictionModel
                                     where  Name = 'MoreInternalWins' );
  if ( @PredictionModelId is null )
  begin
    ; throw 50001, 'PredictionModel not found', 1;
  end;

  declare @PredictionModelInstanceId int;

  insert into nba.PredictionModelInstance
       ( PredictionModelId, Description, SeasonId )
  values
       ( @PredictionModelId, @Description, @SeasonId );

  set @PredictionModelInstanceId = scope_identity();

  insert into nba.GamePrediction
       ( PredictionModelInstanceId, GameId, PredictedWinTeamIdentifier, PredictionConfidence )
  select   @PredictionModelInstanceId
         , g.Id
         , case
             when mw.HomeWins > mw.AwayWins then g.HomeTeamIdentifier
             when mw.HomeWins = mw.AwayWins then g.HomeTeamIdentifier
             else g.AwayTeamIdentifier
           end
         , case
             when mw.HomeWins > mw.AwayWins then round( cast(mw.HomeWins as decimal) / ( mw.HomeWins + mw.AwayWins ), 4 )
             when mw.AwayWins > mw.HomeWins then round( cast(mw.AwayWins as decimal) / ( mw.HomeWins + mw.AwayWins ), 4 )
             else 0.5
           end
  from     nba.Game                     g
           left join ( select   g1.Id
                              , sum( case
                                       when g2.WinTeamIdentifier = g1.HomeTeamIdentifier then 1
                                       else 0
                                     end ) as HomeWins
                              , sum( case
                                       when g2.WinTeamIdentifier = g1.AwayTeamIdentifier then 1
                                       else 0
                                     end ) as AwayWins
                       from     nba.Game           g1
                                left join nba.Game g2 on g2.MatchupIdentifier = g1.MatchupIdentifier
                                                     and g2.GameDate >= @SeasonsBackStartDate
                                                     and g2.GameDate < g1.GameDate
                                                     and g2.IsPlayoffGame = 0
                       where    g1.SeasonId = @SeasonId
                            and g1.IsPlayoffGame = 0
                       group by g1.Id ) mw on g.Id = mw.Id
  where    g.SeasonId = @SeasonId
       and g.IsPlayoffGame = 0
  order by g.GameDate;

end;
go

grant execute on nba.sp_Predictions_MoreInternalWins_ForSeason to roleNba;
go