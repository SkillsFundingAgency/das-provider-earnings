﻿namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[Table("Domain.EarningsProfile")]
[System.ComponentModel.DataAnnotations.Schema.Table("Domain.EarningsProfile")]
public class EarningsProfileModel
{
    [Key]
    public Guid EarningsProfileId { get; set; }
    public Guid EpisodeKey { get; set; }
    public decimal OnProgramTotal { get; set; }
    public List<InstalmentModel> Instalments { get; set; } = null!;
    public decimal CompletionPayment { get; set; }
}
