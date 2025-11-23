create table nba.GameTeamBoxScore ( Id             int            not null identity (1, 1)
                                  , GameBoxScoreId int            not null
                                  , TeamIdentifier char (3)       not null
                                  , Pace           decimal (6, 2) not null
                                  , Efg            decimal (6, 2) not null
                                  , Tov            decimal (6, 2) not null
                                  , Orb            decimal (6, 2) not null
                                  , Ftfga          decimal (6, 2) not null
                                  , Ortg           decimal (6, 2) not null );

grant select, insert, update, delete on nba.GameTeamBoxScore to roleNba;
alter table nba.GameTeamBoxScore add constraint PK_GameTeamBoxScore primary key clustered ( Id );
alter table nba.GameTeamBoxScore
add constraint FK_GameTeamBoxScore_GameId foreign key ( GameBoxScoreId ) references nba.GameBoxScore ( GameId );
create nonclustered index IDX_GameTeamBoxScore_GameBoxScoreId on nba.GameTeamBoxScore ( GameBoxScoreId );
go

create type nba.tt_Merge_GameTeamBoxScore as table ( GameBoxScoreId int            not null
                                                   , TeamIdentifier char (3)       not null
                                                   , Pace           decimal (6, 2) not null
                                                   , Efg            decimal (6, 2) not null
                                                   , Tov            decimal (6, 2) not null
                                                   , Orb            decimal (6, 2) not null
                                                   , Ftfga          decimal (6, 2) not null
                                                   , Ortg           decimal (6, 2) not null );

go

grant exec on type::nba.tt_Merge_GameTeamBoxScore to roleNba;
go