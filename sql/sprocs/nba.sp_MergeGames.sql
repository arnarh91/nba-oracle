create or alter procedure nba.sp_MergeGames @Games nba.tt_Merge_Game readonly
as
begin
  set nocount on;

  merge nba.Game as target
  using ( select g.GameIdentifier
               , s.Id SeasonId
               , g.GameDate
               , g.HomeTeamIdentifier
               , g.AwayTeamIdentifier
               , g.WinTeamIdentifier
               , g.HomePoints
               , g.AwayPoints
               , g.BoxScoreLink
               , g.NumberOfOvertimes
               , g.Attendance
               , g.IsPlayoffGame
          from   @Games          g
                 join nba.Season s on s.StartYear = g.SeasonStartYear ) as source
  on target.GameIdentifier = source.GameIdentifier
  when matched then update set SeasonId = source.SeasonId
                             , GameDate = source.GameDate
                             , HomeTeamIdentifier = source.HomeTeamIdentifier
                             , AwayTeamIdentifier = source.AwayTeamIdentifier
                             , WinTeamIdentifier = source.WinTeamIdentifier
                             , HomePoints = source.HomePoints
                             , AwayPoints = source.AwayPoints
                             , BoxScoreLink = source.BoxScoreLink
                             , NumberOfOvertimes = source.NumberOfOvertimes
                             , Attendance = source.Attendance
                             , IsPlayoffGame = source.IsPlayoffGame
  when not matched by target then insert ( GameIdentifier
                                         , SeasonId
                                         , GameDate
                                         , HomeTeamIdentifier
                                         , AwayTeamIdentifier
                                         , WinTeamIdentifier
                                         , HomePoints
                                         , AwayPoints
                                         , BoxScoreLink
                                         , NumberOfOvertimes
                                         , Attendance
                                         , IsPlayoffGame )
                                  values
                                       ( source.GameIdentifier, source.SeasonId, source.GameDate, source.HomeTeamIdentifier, source.AwayTeamIdentifier, source.WinTeamIdentifier, source.HomePoints, source.AwayPoints, source.BoxScoreLink, source.NumberOfOvertimes, source.Attendance, source.IsPlayoffGame );
end;
go

grant execute on nba.sp_MergeGames to roleNba;
go