use [Chess]
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

/* Création des tables */
if exists (select * from dbo.sysobjects where id = object_id(N'[chess].[PartyMove]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [chess].[PartyMove]
go
if exists (select * from dbo.sysobjects where id = object_id(N'[chess].[Party]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [chess].[Party]
go
if exists (select * from dbo.sysobjects where id = object_id(N'[chess].[Player]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [chess].[Player]
go
if exists (select * from dbo.sysobjects where id = object_id(N'[chess].[Board]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [chess].[Board]
go

create table [chess].[Board]
(
	Id integer not null primary key identity(1,1),
	Wording nvarchar(256) not null,
	Content text not null
)
go

--create table [chess].[Player]
--(
--	Id integer not null primary key identity(1,1),
--	FullName nvarchar(512) not null,
--	Login nvarchar(64) not null,
--	Password nvarchar(256) not null
--)
--go

create table [chess].[Party]
(
	Id integer not null primary key identity(1,1),
	CreationDate datetime null default getdate(),
	WhiteLink nvarchar(256) null,
	BlackLink nvarchar(256) null,
	PartLink nvarchar(256) null,
	--FK_Player_White integer null constraint fk_player_white foreign key references [chess].[Player] (Id) default 1,
	--FK_Player_Black integer null constraint fk_player_black foreign key references [chess].[Player] (Id) default 2,
	FK_Board_Type integer not null constraint fk_board_type foreign key references [chess].[Board] (Id),
	Board text null,
	WhiteTurn bit not null default 1,
	Seed nvarchar(64) not null
)
go

create table [chess].[PartyMove]
(
	Id integer not null primary key identity(1,1),
	FK_Party integer not null constraint fk_party foreign key references [chess].[Party] (Id),
	MoveDate datetime not null default getdate(),
	Move nvarchar(16) not null,
	MoveNumber integer not null
)
go

/* init */
--insert into [chess].[Player] (FullName, Login, Password) values
--('Sammy le Crabe', 'ano1', HASHBYTES('SHA2_256','ano1'))
--go
--insert into [chess].[Player] (FullName, Login, Password) values
--('Jimmy la Jungle', 'ano2', HASHBYTES('SHA2_256','ano2'))
--go
insert into [chess].[Board] (Wording, Content) values
('Classic',
'
	{	"board":[
			{ 	"col": "a",
				"line": "1",
				"piece": "w_rook"
			},
			{	"col": "b",
				"line": "1",
				"piece": "w_knight"
			},
			{	"col": "c",
				"line": "1",
				"piece": "w_bishop"
			},
			{	"col": "d",
				"line": "1",
				"piece": "w_queen"
			},
			{	"col": "e",
				"line": "1",
				"piece": "w_king"
			},
			{	"col": "f",
				"line": "1",
				"piece": "w_bishop"
			},
			{	"col": "g",
				"line": "1",
				"piece": "w_knight"
			},
			{	"col": "h",
				"line": "1",
				"piece": "w_rook"
			},
			{ 	"col": "a",
				"line": "2",
				"piece": "w_pawn"
			},
			{ 	"col": "b",
				"line": "2",
				"piece": "w_pawn"
			},
			{ 	"col": "c",
				"line": "2",
				"piece": "w_pawn"
			},
			{ 	"col": "d",
				"line": "2",
				"piece": "w_pawn"
			},
			{ 	"col": "e",
				"line": "2",
				"piece": "w_pawn"
			},
			{ 	"col": "f",
				"line": "2",
				"piece": "w_pawn"
			},
			{ 	"col": "g",
				"line": "2",
				"piece": "w_pawn"
			},
			{ 	"col": "h",
				"line": "2",
				"piece": "w_pawn"
			},
			{ 	"col": "a",
				"line": "8",
				"piece": "b_rook"
			},
			{	"col": "b",
				"line": "8",
				"piece": "b_knight"
			},
			{	"col": "c",
				"line": "8",
				"piece": "b_bishop"
			},
			{	"col": "d",
				"line": "8",
				"piece": "b_queen"
			},
			{	"col": "e",
				"line": "8",
				"piece": "b_king"
			},
			{	"col": "f",
				"line": "8",
				"piece": "b_bishop"
			},
			{	"col": "g",
				"line": "8",
				"piece": "b_knight"
			},
			{	"col": "h",
				"line": "8",
				"piece": "b_rook"
			},
			{ 	"col": "a",
				"line": "7",
				"piece": "b_pawn"
			},
			{ 	"col": "b",
				"line": "7",
				"piece": "b_pawn"
			},
			{ 	"col": "c",
				"line": "7",
				"piece": "b_pawn"
			},
			{ 	"col": "d",
				"line": "7",
				"piece": "b_pawn"
			},
			{ 	"col": "e",
				"line": "7",
				"piece": "b_pawn"
			},
			{ 	"col": "f",
				"line": "7",
				"piece": "b_pawn"
			},
			{ 	"col": "g",
				"line": "7",
				"piece": "b_pawn"
			},
			{ 	"col": "h",
				"line": "7",
				"piece": "b_pawn"
			}
		]
	}
')
go


select * from [chess].[Board]
select * from [chess].[Party]