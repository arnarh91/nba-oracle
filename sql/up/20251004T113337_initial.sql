create schema [nba] authorization [dbo];
go

create table nba.Season ( Id                         int      not null identity (1, 1)
                        , StartYear                  int      not null
                        , EndYear                    int      not null
                        , RegularSeasonStartDate     date     not null
                        , RegularSeasonEndDate       date     null
                        , PlayoffStartDate           date     null
                        , PlayoffEndDate             date     null
                        , FinalsWinnerTeamIdentifier char (3) null
                        , FinalsLoserTeamIdentifier  char (3) null );
go

grant select, insert, update, delete on nba.Season to roleNba;
alter table nba.Season add constraint PK_Season primary key clustered ( Id );
create unique nonclustered index IDX_Season_StartYear on nba.Season ( StartYear );
go

create table nba.Team ( Identifier char (3)     not null
                      , Name       varchar (64) not null );
go

grant select, insert, update, delete on nba.Team to roleNba;
alter table nba.Team add constraint PK_Team primary key clustered ( Identifier );
go

create table nba.Player ( Id           int            not null identity (1, 1)
                        , Identifier   nvarchar (128) not null
                        , Name         nvarchar (128) not null
                        , BirthDate    date           not null
                        , BirthCountry char (2)       not null
                        , College      nvarchar (128) not null );
go

grant select, insert, update, delete on nba.Player to roleNba;
alter table nba.Player add constraint PK_Player primary key clustered ( Id );
create unique nonclustered index Idx_Player_Identifier on nba.Player ( Identifier );
go

create table nba.TeamSeason ( Id                        int            identity (1, 1) not null
                            , SeasonId                  int            not null
                            , TeamIdentifier            char (3)       not null
                            , Wins                      int            not null
                            , Losses                    int            not null
                            , MarginOfVictory           decimal (6, 2) not null
                            , OffensiveRating           decimal (6, 2) not null
                            , DefensiveRating           decimal (6, 2) not null
                            , WinsLeagueRank            int            not null
                            , LossesLeagueRank          int            not null
                            , MarginOfVictoryLeagueRank int            not null
                            , OffensiveRatingLeagueRank int            not null
                            , DefensiveRatingLeagueRank int            not null );
go

grant select, insert, update, delete on nba.TeamSeason to roleNba;
alter table nba.TeamSeason add constraint PK_TeamSeason primary key clustered ( Id );
alter table nba.TeamSeason add constraint FK_TeamSeason_SeasonId foreign key ( SeasonId ) references nba.Season ( Id );
alter table nba.TeamSeason
add constraint FK_TeamSeason_TeamIdentifier foreign key ( TeamIdentifier ) references nba.Team ( Identifier );
create nonclustered index Idx_TeamSeason_SeasonId on nba.TeamSeason ( Id );
go

create table nba.TeamSeasonRoster ( Id                   int            identity (1, 1) not null
                                  , TeamSeasonId         int            not null
                                  , PlayerId             int            not null
                                  , JerseyNumber         varchar (6)    not null
                                  , Position             varchar (2)    not null
                                  , Height               decimal (6, 2) not null
                                  , Weight               decimal (6, 2) not null
                                  , NumberOfYearInLeague int            not null );
go

grant select, insert, update, delete on nba.TeamSeasonRoster to roleNba;
alter table nba.TeamSeasonRoster add constraint PK_TeamSeasonRoster primary key clustered ( Id );
alter table nba.TeamSeasonRoster
add constraint FK_TeamSeasonRoster_TeamSeasonId foreign key ( TeamSeasonId ) references nba.TeamSeason ( Id );
alter table nba.TeamSeasonRoster
add constraint FK_TeamSeasonRoster_PlayerId foreign key ( PlayerId ) references nba.Player ( Id );
create nonclustered index Idx_TeamSeasonRoster_TeamSeasonId on nba.TeamSeasonRoster ( TeamSeasonId );
go

create table nba.PlayerSeasonStatistics ( Id                           int            not null identity (1, 1)
                                        , TeamSeasonId                 int            not null
                                        , PlayerId                     int            not null
                                        , ForRegularSeason             bit            not null
                                        , GamesPlayed                  int            not null
                                        , MinutesPlayed                int            not null
                                        , MinutedPlayedPerGame         decimal (6, 2) not null
                                        , FieldGoalsMade               int            not null
                                        , FieldGoalsAttempted          int            not null
                                        , FieldGoalPercentage          decimal (6, 5) not null
                                        , ThreePointersMade            int            not null
                                        , ThreePointersAttempted       int            not null
                                        , ThreePointersPercentage      decimal (6, 5) not null
                                        , TwoPointersMade              int            not null
                                        , TwoPointersAttempted         int            not null
                                        , TwoPointersPercentage        decimal (6, 5) not null
                                        , FreeThrowsMade               int            not null
                                        , FreeThrowsAttempted          int            not null
                                        , FreeThrowsPercentage         decimal (6, 5) not null
                                        , EffectiveFieldGoalPercentage decimal (6, 5) not null
                                        , OffensiveRebounds            int            not null
                                        , DefensiveRebounds            int            not null
                                        , TotalRebounds                int            not null
                                        , ReboundsPerGame              decimal (6, 2) not null
                                        , Assists                      int            not null
                                        , AssistsPerGame               decimal (6, 2) not null
                                        , Steals                       int            not null
                                        , StealsPerGame                decimal (6, 2) not null
                                        , Blocks                       int            not null
                                        , BlocksPerGame                decimal (6, 2) not null
                                        , Turnovers                    int            not null
                                        , TurnoversPerGame             decimal (6, 2) not null
                                        , PersonalFouls                int            not null
                                        , PersonalFoulsPerGame         decimal (6, 2) not null
                                        , Points                       int            not null
                                        , PointsPerGame                decimal (6, 2) not null
                                        , PlusMinusOnCourt             decimal (6, 2) not null
                                        , PlusMinusNetOnCourt          decimal (6, 2) not null );
go
grant select, insert, update, delete on nba.PlayerSeasonStatistics to roleNba;
alter table nba.PlayerSeasonStatistics add constraint PK_PlayerSeasonStatistics primary key clustered ( Id );
alter table nba.PlayerSeasonStatistics
add constraint FK_PlayerSeasonStatistics_PlayerId foreign key ( PlayerId ) references nba.Player ( Id );
alter table nba.PlayerSeasonStatistics
add constraint FK_PlayerSeasonStatistics_TeamSeasonId foreign key ( TeamSeasonId ) references nba.TeamSeason ( Id );
go

create table nba.Game ( Id                 int          not null identity (1, 1)
                      , SeasonId           int          not null
                      , GameIdentifier     varchar (26) not null
                      , GameDate           date         not null
                      , HomeTeamIdentifier char (3)     not null
                      , AwayTeamIdentifier char (3)     not null
                      , WinTeamIdentifier  char (3)     not null
                      , HomePoints         int          not null
                      , AwayPoints         int          not null
                      , BoxScoreLink       varchar (56) not null
                      , NumberOfOvertimes  int          null
                      , Attendance         int          not null
                      , IsPlayoffGame      bit          not null
                      , MatchupIdentifier  as ( convert( varchar (64)
                                                       , hashbytes( 'SHA2_256'
                                                                  , case
                                                                      when HomeTeamIdentifier < AwayTeamIdentifier then HomeTeamIdentifier + AwayTeamIdentifier
                                                                      else AwayTeamIdentifier + HomeTeamIdentifier
                                                                    end )
                                                       , 2 )) persisted );
go

grant select, insert, update, delete on nba.Game to roleNba;
alter table nba.Game add constraint PK_Game primary key clustered ( Id );
create unique nonclustered index Idx_Game_GameIdentifier on nba.Game ( GameIdentifier );
create nonclustered index Idx_Game_SeasonId on nba.Game ( SeasonId );
go

create table nba.PredictionModel ( Id          int           not null identity (1, 1)
                                 , Name        varchar (56)  not null
                                 , Description varchar (256) not null );
go
grant select, insert, update, delete on nba.PredictionModel to roleNba;
alter table nba.PredictionModel add constraint PK_PredictionModel primary key clustered ( Id );
go

create table nba.PredictionModelInstance ( Id                int           not null identity (1, 1)
                                         , PredictionModelId int           not null
                                         , Description       varchar (256) not null
                                         , SeasonId          int           not null
                                         , CreatedAt         datetime2     not null default getutcdate());
go
grant select, insert, update, delete on nba.PredictionModelInstance to roleNba;
alter table nba.PredictionModelInstance add constraint PK_PredictionModelInstance primary key clustered ( Id );
alter table nba.PredictionModelInstance
add constraint FK_PredictionModelInstance_PredictionModelId foreign key ( PredictionModelId ) references nba.PredictionModel ( Id );
alter table nba.PredictionModelInstance
add constraint FK_PredictionModelInstance_SeasonId foreign key ( SeasonId ) references nba.Season ( Id );
go

create table nba.GamePrediction ( Id                         int            not null identity (1, 1)
                                , PredictionModelInstanceId  int            not null
                                , GameId                     int            not null
                                , PredictedWinTeamIdentifier char (3)       not null
                                , PredictionConfidence       decimal (6, 5) null );
go
grant select, insert, update, delete on nba.GamePrediction to roleNba;
alter table nba.GamePrediction add constraint PK_GamePrediction primary key clustered ( Id );
alter table nba.GamePrediction
add constraint FK_GamePrediction_PredictionModelInstanceId foreign key ( PredictionModelInstanceId ) references nba.PredictionModelInstance ( Id );
alter table nba.GamePrediction add constraint FK_GamePrediction_GameId foreign key ( GameId ) references nba.Game ( Id );
go

create type nba.tt_Merge_Player as table ( Identifier   nvarchar (128) not null
                                         , Name         nvarchar (125) not null
                                         , BirthDate    date           not null
                                         , BirthCountry char (2)       not null
                                         , College      nvarchar (128) not null );

create type nba.tt_Merge_TeamSeason as table ( SeasonStartYear           int            not null
                                             , TeamIdentifier            char (3)       not null
                                             , Wins                      int            not null
                                             , Losses                    int            not null
                                             , MarginOfVictory           decimal (6, 2) not null
                                             , OffensiveRating           decimal (6, 2) not null
                                             , DefensiveRating           decimal (6, 2) not null
                                             , WinsLeagueRank            int            not null
                                             , LossesLeagueRank          int            not null
                                             , MarginOfVictoryLeagueRank int            not null
                                             , OffensiveRatingLeagueRank int            not null
                                             , DefensiveRatingLeagueRank int            not null );

create type nba.tt_Merge_TeamSeasonRoster as table ( SeasonStartYear      int            not null
                                                   , TeamIdentifier       char (3)       not null
                                                   , PlayerIdentifier     nvarchar (128) not null
                                                   , JerseyNumber         varchar (6)    not null
                                                   , Position             varchar (2)    not null
                                                   , Height               decimal (6, 2) not null
                                                   , Weight               decimal (6, 2) not null
                                                   , NumberOfYearInLeague int            not null );

create type nba.tt_Merge_PlayerSeasonStatistics as table ( SeasonStartYear              int            not null
                                                         , TeamIdentifier               char (3)       not null
                                                         , PlayerIdentifier             nvarchar (128) not null
                                                         , ForRegularSeason             bit            not null
                                                         , GamesPlayed                  int            not null
                                                         , MinutesPlayed                int            not null
                                                         , MinutedPlayedPerGame         decimal (6, 2) not null
                                                         , FieldGoalsMade               int            not null
                                                         , FieldGoalsAttempted          int            not null
                                                         , FieldGoalPercentage          decimal (6, 5) not null
                                                         , ThreePointersMade            int            not null
                                                         , ThreePointersAttempted       int            not null
                                                         , ThreePointersPercentage      decimal (6, 5) not null
                                                         , TwoPointersMade              int            not null
                                                         , TwoPointersAttempted         int            not null
                                                         , TwoPointersPercentage        decimal (6, 5) not null
                                                         , FreeThrowsMade               int            not null
                                                         , FreeThrowsAttempted          int            not null
                                                         , FreeThrowsPercentage         decimal (6, 5) not null
                                                         , EffectiveFieldGoalPercentage decimal (6, 5) not null
                                                         , OffensiveRebounds            int            not null
                                                         , DefensiveRebounds            int            not null
                                                         , TotalRebounds                int            not null
                                                         , ReboundsPerGame              decimal (6, 2) not null
                                                         , Assists                      int            not null
                                                         , AssistsPerGame               decimal (6, 2) not null
                                                         , Steals                       int            not null
                                                         , StealsPerGame                decimal (6, 2) not null
                                                         , Blocks                       int            not null
                                                         , BlocksPerGame                decimal (6, 2) not null
                                                         , Turnovers                    int            not null
                                                         , TurnoversPerGame             decimal (6, 2) not null
                                                         , PersonalFouls                int            not null
                                                         , PersonalFoulsPerGame         decimal (6, 2) not null
                                                         , Points                       int            not null
                                                         , PointsPerGame                decimal (6, 2) not null
                                                         , PlusMinusOnCourt             decimal (6, 2) not null
                                                         , PlusMinusNetOnCourt          decimal (6, 2) not null );

create type nba.tt_Merge_Game as table ( GameIdentifier     varchar (26) not null
                                       , SeasonStartYear    int          not null
                                       , GameDate           date         not null
                                       , HomeTeamIdentifier char (3)     not null
                                       , AwayTeamIdentifier char (3)     not null
                                       , WinTeamIdentifier  char (3)     not null
                                       , HomePoints         int          not null
                                       , AwayPoints         int          not null
                                       , BoxScoreLink       varchar (56) not null
                                       , NumberOfOvertimes  int          null
                                       , Attendance         int          not null
                                       , IsPlayoffGame      bit          not null );
go

grant exec on type::nba.tt_Merge_Player to roleNba;
grant exec on type::nba.tt_Merge_TeamSeason to roleNba;
grant exec on type::nba.tt_Merge_TeamSeasonRoster to roleNba;
grant exec on type::nba.tt_Merge_PlayerSeasonStatistics to roleNba;
grant exec on type::nba.tt_Merge_Game to roleNba;
go