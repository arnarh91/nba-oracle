create or alter procedure nba.sp_Predictions_LowerDefensiveRatingLastSeason_ForSeason (
  @SeasonStartYear int
, @Description     varchar (256))
as
begin
  set nocount on;

  declare @SeasonId int = ( select Id
                            from   nba.Season
                            where  StartYear = @SeasonStartYear );
  if ( @SeasonId is null )
  begin
    ; throw 50001, 'Season not found', 1;
  end;

  declare @LastSeasonId int = ( select Id
                                from   nba.Season
                                where  StartYear = @SeasonStartYear - 1 );
  if ( @SeasonId is null )
  begin
    ; throw 50001, 'Season not found', 1;
  end;

  declare @PredictionModelId int = ( select Id
                                     from   nba.PredictionModel
                                     where  Name = 'LowerDefensiveRatingLastSeason' );
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
  select @PredictionModelInstanceId
       , g.Id
       , case
           when homeLastSeason.DefensiveRating < awayLastSeason.DefensiveRating then g.HomeTeamIdentifier
           else g.AwayTeamIdentifier
         end
       , null
  from   nba.Game            g
         join nba.TeamSeason homeLastSeason on homeLastSeason.SeasonId = @LastSeasonId
                                           and homeLastSeason.TeamIdentifier = g.HomeTeamIdentifier

         join nba.TeamSeason awayLastSeason on awayLastSeason.SeasonId = @LastSeasonId
                                           and awayLastSeason.TeamIdentifier = g.AwayTeamIdentifier
  where  g.SeasonId = @SeasonId;

end;
go

grant execute on nba.sp_Predictions_LowerDefensiveRatingLastSeason_ForSeason to roleNba;
go