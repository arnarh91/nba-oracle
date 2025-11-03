create or alter procedure nba.sp_MergeInjuryReportToStaging @Injuries nba.tt_Merge_PlayerInjuryReport readonly
as
begin
  set nocount on;

  merge nba.InjuryReport_Staging as target
  using ( select i.PlayerName
               , i.TeamIdentifier
               , i.InjuryStatus
               , i.EstimatedReturnDate
               , i.InjuryDescription
               , i.EffectiveFrom
          from   @Injuries i ) as source
  on target.PlayerName = source.PlayerName
 and target.TeamIdentifier = source.TeamIdentifier
  when matched then update set InjuryStatus = source.InjuryStatus
                             , EstimatedReturnDate = source.EstimatedReturnDate
                             , InjuryDescription = source.InjuryDescription
                             , EffectiveFrom = source.EffectiveFrom
  when not matched by target then insert ( PlayerName
                                         , TeamIdentifier
                                         , InjuryStatus
                                         , EstimatedReturnDate
                                         , InjuryDescription
                                         , EffectiveFrom )
                                  values
                                       ( source.PlayerName, source.TeamIdentifier, source.InjuryStatus, source.EstimatedReturnDate, source.InjuryDescription, source.EffectiveFrom );
end;
go

grant execute on nba.sp_MergeInjuryReportToStaging to roleNba;
go