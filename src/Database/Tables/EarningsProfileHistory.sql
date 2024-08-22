﻿CREATE TABLE [Domain].[EarningsProfileHistory]
(
    [EarningsProfileId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[EpisodeKey] UNIQUEIDENTIFIER NOT NULL, 
    [AdjustedPrice] MONEY NULL, 
    [CompletionPayment] MONEY NULL,
    [SupersededDate] DATETIME NOT NULL
)
GO
ALTER TABLE Domain.[EarningsProfileHistory]
ADD CONSTRAINT FK_EarningsProfileHistory_Episode FOREIGN KEY (EpisodeKey) REFERENCES Domain.Episode ([Key])
GO
CREATE INDEX IX_EpisodeKey ON Domain.[EarningsProfileHistory] (EpisodeKey);
GO