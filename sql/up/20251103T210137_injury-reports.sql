create table nba.InjuryReport ( Id                  int           identity (1, 1)
                              , PlayerId            int           not null
                              , TeamIdentifier      char (3)      not null
                              , InjuryStatus        varchar (26)  not null
                              , EstimatedReturnDate date          not null
                              , InjuryDescription   varchar (512) not null
                              , EffectiveFrom       date          not null
                              , EffectiveTo         date          null );

grant select, insert, update, delete on nba.InjuryReport to roleNba;
alter table nba.InjuryReport add constraint PK_InjuryReport primary key clustered ( Id );
alter table nba.InjuryReport
add constraint FK_InjuryReport_PlayerId foreign key ( PlayerId ) references nba.Player ( Id );
go

create table nba.InjuryReport_Staging ( Id                  int            identity (1, 1)
                                      , PlayerName          nvarchar (128) not null
                                      , TeamIdentifier      char (3)       not null
                                      , InjuryStatus        varchar (26)   not null
                                      , EstimatedReturnDate date           not null
                                      , InjuryDescription   varchar (512)  not null
                                      , EffectiveFrom       date           not null );

grant select, insert, update, delete on nba.InjuryReport_Staging to roleNba;
alter table nba.InjuryReport_Staging add constraint PK_InjuryReport_Staging primary key clustered ( Id );
go

create type nba.tt_Merge_PlayerInjuryReport as table ( PlayerName          nvarchar (128) not null
                                                     , TeamIdentifier      char (3)       not null
                                                     , InjuryStatus        varchar (26)   not null
                                                     , EstimatedReturnDate date           not null
                                                     , InjuryDescription   varchar (512)  not null
                                                     , EffectiveFrom       date           not null );
go

grant exec on type::nba.tt_Merge_PlayerInjuryReport to roleNba;
go