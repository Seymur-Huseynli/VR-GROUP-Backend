Use master
CREATE DATABASE [VR.GROUP.TEST]
GO
USE [VR.GROUP.TEST]
GO
CREATE TABLE Box
(
Id int Primary Key identity(1,1),
SupplierIdentifier nvarchar(50),
Identifier nvarchar(50) Unique
)

CREATE TABLE Content
(
Id int Primary Key identity(1,1),
Identifier nvarchar(50) FOREIGN KEY References Box(Identifier),
PoNumber nvarchar(50),
Isbn nvarchar(50),
Quantity int not null default(0)
)