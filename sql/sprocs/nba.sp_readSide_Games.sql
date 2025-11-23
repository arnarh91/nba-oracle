create or alter procedure nba.sp_readSide_Games ( @startYear int )
as
begin
  set nocount on;

  select   g.Id                 GameId
         , g.GameIdentifier     GameIdentifier
         , g.GameDate           GameDate
         , g.HomeTeamIdentifier HomeTeam
         , g.AwayTeamIdentifier AwayTeam
         , g.WinTeamIdentifier  WinTeam
         , g.HomePoints         HomePoints
         , g.AwayPoints         AwayPoints
         , g.MatchupIdentifier  MatchupIdentifier
         , g.BoxScoreLink       BoxScoreLink
  from     nba.Game        g
           join nba.Season s on s.Id = g.SeasonId
  where    s.StartYear = @startYear
       and g.IsPlayoffGame = 0
  order by g.GameDate;

  select teambox.GameBoxScoreId
       , teambox.TeamIdentifier
       , teambox.Pace
       , teambox.Efg
       , teambox.Tov
       , teambox.Orb
       , teambox.Ftfga
       , teambox.Ortg
  from   nba.GameTeamBoxScore teambox
         join nba.Game        g on g.Id = teambox.GameBoxScoreId

         join nba.Season      s on s.Id = g.SeasonId
  where  s.StartYear = @startYear
     and g.IsPlayoffGame = 0;

end;
go

grant execute on nba.sp_readSide_Games to roleNba;
go