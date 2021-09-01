CREATE DATABASE [Test];
GO

USE [Test];
GO

CREATE TABLE [dbo].[People] (
    [Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [Name] VARCHAR(255) NOT NULL
);

CREATE TABLE [dbo].[Pets] (
    [Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [PersonId] INT NOT NULL,
    [Name] VARCHAR(255) NOT NULL UNIQUE,
    CONSTRAINT FK_People_Pets FOREIGN KEY ([PersonId]) REFERENCES [People] ([Id])
);