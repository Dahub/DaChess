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

/* Cr�ation des tables */
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
	FK_Board_Type integer not null constraint fk_board_type foreign key references [chess].[Board] (Id),
	Board text null,
	History text null,
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

insert into [chess].[Board] (Wording, Content) values
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


select * from [chess].[Board]
select * from [chess].[Party]
select * from [chess].[PartyMove]
