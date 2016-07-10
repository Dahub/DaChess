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
	<board name="classic">
		<rank wording="1">
			<col wording="A">Tb</col>
			<col wording="B">Cb</col>
			<col wording="C">Fb</col>
			<col wording="D">Db</col>
			<col wording="E">Rb</col>
			<col wording="F">Fb</col>
			<col wording="G">Cb</col>
			<col wording="H">Tb</col>
		</rank>
		<rank wording="2">
			<col wording="A">Pb</col>
			<col wording="B">Pb</col>
			<col wording="C">Pb</col>
			<col wording="D">Pb</col>
			<col wording="E">Pb</col>
			<col wording="F">Pb</col>
			<col wording="G">Pb</col>
			<col wording="H">Pb</col>
		</rank>
		<rank wording="3">
			<col wording="A"></col>
			<col wording="B"></col>
			<col wording="C"></col>
			<col wording="D"></col>
			<col wording="E"></col>
			<col wording="F"></col>
			<col wording="G"></col>
			<col wording="H"></col>
		</rank>
		<rank wording="4">
			<col wording="A"></col>
			<col wording="B"></col>
			<col wording="C"></col>
			<col wording="D"></col>
			<col wording="E"></col>
			<col wording="F"></col>
			<col wording="G"></col>
			<col wording="H"></col>
		</rank>
		<rank wording="5">
			<col wording="A"></col>
			<col wording="B"></col>
			<col wording="C"></col>
			<col wording="D"></col>
			<col wording="E"></col>
			<col wording="F"></col>
			<col wording="G"></col>
			<col wording="H"></col>
		</rank>
		<rank wording="6">
			<col wording="A"></col>
			<col wording="B"></col>
			<col wording="C"></col>
			<col wording="D"></col>
			<col wording="E"></col>
			<col wording="F"></col>
			<col wording="G"></col>
			<col wording="H"></col>
		</rank>
		<rank wording="7">
			<col wording="A">Pn</col>
			<col wording="B">Pn</col>
			<col wording="C">Pn</col>
			<col wording="D">Pn</col>
			<col wording="E">Pn</col>
			<col wording="F">Pn</col>
			<col wording="G">Pn</col>
			<col wording="H">Pn</col>
		</rank>
		<rank wording="8">
			<col wording="A">Tn</col>
			<col wording="B">Cn</col>
			<col wording="C">Fn</col>
			<col wording="D">Dn</col>
			<col wording="E">Rn</col>
			<col wording="F">Fn</col>
			<col wording="G">Cn</col>
			<col wording="H">Tn</col>
		</rank>
	</board>
')
go


select * from [chess].[Board]
select * from [chess].[Party]