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

create table [chess].[Player]
(
	Id integer not null primary key identity(1,1),
	FullName nvarchar(512) not null,
	Login nvarchar(64) not null,
	Password nvarchar(256) not null
)
go

create table [chess].[Party]
(
	Id integer not null primary key identity(1,1),
	CreationDate datetime not null default getdate(),
	WhiteLink nvarchar(256) not null,
	BlackLink nvarchar(256) not null,
	PartLink nvarchar(256) not null,
	FK_Player_White integer not null constraint fk_player_white foreign key references [chess].[Player] (Id),
	FK_Player_Black integer not null constraint fk_player_black foreign key references [chess].[Player] (Id),
	Board nvarchar(2048) not null,
	WhiteTurn bit not null default 1
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
insert into [chess].[Player] (FullName, Login, Password) values
('Sammy le Crabe', 'ano1', HASHBYTES('SHA2_256','ano1'))
go
insert into [chess].[Player] (FullName, Login, Password) values
('Jimmy la Jungle', 'ano2', HASHBYTES('SHA2_256','ano2'))
go