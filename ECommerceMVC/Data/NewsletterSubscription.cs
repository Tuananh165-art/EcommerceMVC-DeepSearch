using System;

namespace ECommerceMVC.Data;

public partial class NewsletterSubscription
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }
}