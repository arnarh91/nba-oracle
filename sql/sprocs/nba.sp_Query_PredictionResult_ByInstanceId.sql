create or alter procedure nba.sp_Query_PredictionResult_ByInstanceId ( @PredictionModelInstanceId int )
as
begin
  set nocount on;

  select   pm.Name                                               as PredictionModelName
         , pms.Description                                       Description
         , s.StartYear
         , count( * )                                            as TotalPredictions
         , sum( case
                  when gp.PredictedWinTeamIdentifier = g.WinTeamIdentifier then 1
                  else 0
                end )                                            as CorrectPredictions
         , cast(sum( case
                       when gp.PredictedWinTeamIdentifier = g.WinTeamIdentifier then 1.0
                       else 0.0
                     end ) / count( * ) * 100 as decimal (7, 4)) as CorrectPredictionsPercentage
  from     nba.GamePrediction               gp
           join nba.PredictionModelInstance pms on gp.PredictionModelInstanceId = pms.Id

           join nba.PredictionModel         pm on pms.PredictionModelId = pm.Id

           join nba.Season                  s on pms.SeasonId = s.Id

           join nba.Game                    g on gp.GameId = g.Id
  where    pms.Id = @PredictionModelInstanceId
  group by pm.Name
         , pms.Description
         , s.StartYear;
end;
go

grant execute on nba.sp_Query_PredictionResult_ByInstanceId to roleNba;
go