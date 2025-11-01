merge nba.BettingProvider as target
using ( select 'OddsPortal' as ProviderName ) as source ( ProviderName )
on ( target.ProviderName = source.ProviderName )
when not matched then insert ( ProviderName )
                      values
                           ( source.ProviderName );
go