﻿use [Chess]
go

if not exists 
(
	select schema_name
	from information_schema.schemata
	where schema_name = 'chess'
) 
begin
	exec sp_executesql N'CREATE SCHEMA chess'
end
go

if exists (select * from dbo.sysobjects where id = object_id(N'[chess].[PartyHistory]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [chess].[PartyHistory]
go
if exists (select * from dbo.sysobjects where id = object_id(N'[chess].[Party]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [chess].[Party]
go
if exists (select * from dbo.sysobjects where id = object_id(N'[chess].[BoardType]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [chess].[BoardType]
go
if exists (select * from dbo.sysobjects where id = object_id(N'[chess].[PlayerState]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [chess].[PlayerState]
go
if exists (select * from dbo.sysobjects where id = object_id(N'[chess].[PartyState]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [chess].[PartyState]
go

create table [chess].[PartyState]
(
	Id integer not null primary key identity(1,1),
	Wording nvarchar(256) not null
)
go

create table [chess].[PlayerState]
(
	Id integer not null primary key identity(1,1),
	Wording nvarchar(256) not null	
)
go

create table [chess].[BoardType]
(
	Id integer not null primary key identity(1,1),
	Wording nvarchar(256) not null,
	Content text not null
)
go

create table [chess].[Party]
(
	Id integer not null primary key identity(1,1),
	FK_BoardType integer not null constraint fk_board_type foreign key references [chess].[BoardType] (Id),
	FK_PartyState integer not null constraint fk_partyState foreign key references [chess].[PartyState] (Id),
	FK_White_PlayerState integer not null constraint fk_white_playerState foreign key references [chess].[PlayerState] (Id) default 1,
	FK_Black_PlayerState integer not null constraint fk_black_playerState foreign key references [chess].[PlayerState] (Id) default 1,	
	CreationDate datetime null default getdate(),
	WhiteToken nvarchar(256) null,
	BlackToken nvarchar(256) null,
	PartyName nvarchar(256) null,	
	Board nvarchar(max) null,
	JsonHistory nvarchar(max) null,
	Seed nvarchar(64) not null,
	EnPassantCase nvarchar(2) null,
	LastMoveCase nvarchar(2) null
)
go

create table [chess].[PartyHistory]
(
	Id integer not null primary key identity(1,1),
	FK_Party integer not null constraint fk_party foreign key references [chess].[Party] (Id),
	Board nvarchar(max) not null,
	DateCreation datetime not null default getdate()
)
go

insert into [chess].[PartyState] (wording) values ('Attente de joueurs')
insert into [chess].[PartyState] (wording) values ('En cours')
insert into [chess].[PartyState] (wording) values ('Terminée nulle')
insert into [chess].[PartyState] (wording) values ('Terminée victoire blanc')
insert into [chess].[PartyState] (wording) values ('Terminée victoire noir')
go

insert into [chess].[PlayerState] (wording) values ('non défini')
insert into [chess].[PlayerState] (wording) values ('peut jouer')
insert into [chess].[PlayerState] (wording) values ('attend son tour')
insert into [chess].[PlayerState] (wording) values ('peut promouvoir')
insert into [chess].[PlayerState] (wording) values ('demande nulle')
insert into [chess].[PlayerState] (wording) values ('pat')
insert into [chess].[PlayerState] (wording) values ('abandonne')
insert into [chess].[PlayerState] (wording) values ('échec')
insert into [chess].[PlayerState] (wording) values ('échec et mat')
go

insert into [chess].[BoardType] (Wording, Content) values
('Classic',
'
	{	"board":[
			{ 	"col": "a",
				"line": "1",
				"piece": "w_rook",
				"hasMove": "false"
			},
			{	"col": "b",
				"line": "1",
				"piece": "w_knight",
				"hasMove": "false"
			},
			{	"col": "c",
				"line": "1",
				"piece": "w_bishop",
				"hasMove": "false"
			},
			{	"col": "d",
				"line": "1",
				"piece": "w_queen",
				"hasMove": "false"
			},
			{	"col": "e",
				"line": "1",
				"piece": "w_king",
				"hasMove": "false"
			},
			{	"col": "f",
				"line": "1",
				"piece": "w_bishop",
				"hasMove": "false"
			},
			{	"col": "g",
				"line": "1",
				"piece": "w_knight",
				"hasMove": "false"
			},
			{	"col": "h",
				"line": "1",
				"piece": "w_rook",
				"hasMove": "false"
			},
			{ 	"col": "a",
				"line": "2",
				"piece": "w_pawn",
				"hasMove": "false"
			},
			{ 	"col": "b",
				"line": "2",
				"piece": "w_pawn",
				"hasMove": "false"
			},
			{ 	"col": "c",
				"line": "2",
				"piece": "w_pawn",
				"hasMove": "false"
			},
			{ 	"col": "d",
				"line": "2",
				"piece": "w_pawn",
				"hasMove": "false"
			},
			{ 	"col": "e",
				"line": "2",
				"piece": "w_pawn",
				"hasMove": "false"
			},
			{ 	"col": "f",
				"line": "2",
				"piece": "w_pawn",
				"hasMove": "false"
			},
			{ 	"col": "g",
				"line": "2",
				"piece": "w_pawn",
				"hasMove": "false"
			},
			{ 	"col": "h",
				"line": "2",
				"piece": "w_pawn",
				"hasMove": "false"
			},
			{ 	"col": "a",
				"line": "8",
				"piece": "b_rook",
				"hasMove": "false"
			},
			{	"col": "b",
				"line": "8",
				"piece": "b_knight",
				"hasMove": "false"
			},
			{	"col": "c",
				"line": "8",
				"piece": "b_bishop",
				"hasMove": "false"
			},
			{	"col": "d",
				"line": "8",
				"piece": "b_queen",
				"hasMove": "false"
			},
			{	"col": "e",
				"line": "8",
				"piece": "b_king",
				"hasMove": "false"
			},
			{	"col": "f",
				"line": "8",
				"piece": "b_bishop",
				"hasMove": "false"
			},
			{	"col": "g",
				"line": "8",
				"piece": "b_knight",
				"hasMove": "false"
			},
			{	"col": "h",
				"line": "8",
				"piece": "b_rook",
				"hasMove": "false"
			},
			{ 	"col": "a",
				"line": "7",
				"piece": "b_pawn",
				"hasMove": "false"
			},
			{ 	"col": "b",
				"line": "7",
				"piece": "b_pawn",
				"hasMove": "false"
			},
			{ 	"col": "c",
				"line": "7",
				"piece": "b_pawn",
				"hasMove": "false"
			},
			{ 	"col": "d",
				"line": "7",
				"piece": "b_pawn",
				"hasMove": "false"
			},
			{ 	"col": "e",
				"line": "7",
				"piece": "b_pawn",
				"hasMove": "false"
			},
			{ 	"col": "f",
				"line": "7",
				"piece": "b_pawn",
				"hasMove": "false"
			},
			{ 	"col": "g",
				"line": "7",
				"piece": "b_pawn",
				"hasMove": "false"
			},
			{ 	"col": "h",
				"line": "7",
				"piece": "b_pawn",
				"hasMove": "false"
			}
		]
	}
')
go






select count(*), Board  from [chess].[PartyHistory]
group by Board
select*  from [chess].[PartyHistory]
order by 1 desc

select partS.Wording, whiteS.Wording as 'blancs', blackS.Wording as 'noirs', *  from [chess].[Party] p
inner join [chess].[PartyState] partS on p.FK_PartyState = partS.Id
inner join [chess].[PlayerState] whiteS on p.FK_White_PlayerState = whiteS.Id
inner join [chess].[PlayerState] blackS on p.FK_Black_PlayerState = blackS.Id
order by p.Id desc