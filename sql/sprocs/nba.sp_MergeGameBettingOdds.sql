create or alter procedure nba.sp_MergeGameBettingOdds @Odds nba.tt_Merge_GameBettingOdds readonly
as
begin
  set nocount on;

  merge nba.GameBettingOdds as target
  using ( select bp.Id BettingProviderId
               , g.Id  GameId
               , o.HomeOdds
               , o.AwayOdds
               , o.HomeConfidence
               , o.AwayConfidence
               , o.ScrapedAt
          from   @Odds                    o
                 join nba.BettingProvider bp on bp.ProviderName = o.BettingProviderName

                 join nba.Game            g on g.GameIdentifier = o.GameIdentifier ) as source
  on target.GameId = source.GameId
 and target.BettingProviderId = source.BettingProviderId
  when not matched by target then insert ( BettingProviderId
                                         , GameId
                                         , HomeOdds
                                         , AwayOdds
                                         , HomeConfidence
                                         , AwayConfidence
                                         , ScrapedAt )
                                  values
                                       ( source.BettingProviderId, source.GameId, source.HomeOdds, source.AwayOdds, source.HomeConfidence, source.AwayConfidence, source.ScrapedAt );
end;
go

grant execute on nba.sp_MergeGameBettingOdds to roleNba;
go