create table nba.BettingProvider ( Id           int          not null identity (1, 1)
                                 , ProviderName varchar (26) not null );
go

grant select, insert, update, delete on nba.BettingProvider to roleNba;
alter table nba.BettingProvider add constraint PK_BettingProvider primary key clustered ( Id );
create unique nonclustered index IDX_BettingProvider_ProviderName on nba.BettingProvider ( ProviderName );
go

create table nba.GameBettingOdds ( Id                int            not null identity (1, 1)
                                 , BettingProviderId int            not null
                                 , GameId            int            not null
                                 , HomeOdds          decimal (6, 2) not null
                                 , AwayOdds          decimal (6, 2) not null
                                 , HomeConfidence    decimal (6, 5) not null
                                 , AwayConfidence    decimal (6, 5) not null
                                 , ScrapedAt         datetime2      not null );
go
grant select, insert, update, delete on nba.GameBettingOdds to roleNba;
alter table nba.GameBettingOdds add constraint PK_GameBettingOdds primary key clustered ( Id );
alter table nba.GameBettingOdds
add constraint FK_GameBettingOdds_BettingProviderId foreign key ( BettingProviderId ) references nba.BettingProvider ( Id );
alter table nba.GameBettingOdds
add constraint FK_GameBettingOdds_GameId foreign key ( GameId ) references nba.Game ( Id );
go

create type nba.tt_Merge_GameBettingOdds as table ( BettingProviderName varchar (26)   not null
                                                  , GameIdentifier      varchar (26)   not null
                                                  , HomeOdds            decimal (6, 2) not null
                                                  , AwayOdds            decimal (6, 2) not null
                                                  , HomeConfidence      decimal (6, 5) not null
                                                  , AwayConfidence      decimal (6, 5) not null
                                                  , ScrapedAt           datetime2      not null );
go

grant exec on type::nba.tt_Merge_GameBettingOdds to roleNba;
go