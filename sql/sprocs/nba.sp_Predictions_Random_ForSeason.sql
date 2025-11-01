create or alter procedure nba.sp_Predictions_Random_ForSeason (
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

  declare @PredictionModelId int = ( select Id
                                     from   nba.PredictionModel
                                     where  Name = 'Random' );
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
       , Id
       , case
           when ( abs( checksum( newid())) % 2 ) = 1 then HomeTeamIdentifier
           else AwayTeamIdentifier
         end
       , 0.5
  from   nba.Game
  where  SeasonId = @SeasonId;

end;
go

grant execute on nba.sp_Predictions_Random_ForSeason to roleNba;
go