﻿using System.ComponentModel.DataAnnotations;

namespace demo_02.Models;

public partial class Eventparticipation
{
    public int ParticipationId { get; set; }

    public int EventsId { get; set; }

    public int UserId { get; set; }

    public string ParticipationStatus { get; set; }

    public int? EarnedPoints { get; set; }

    public DateTime? ParticipationTime { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? IsDelete { get; set; }

    public virtual ICollection<Eventdonation> Eventdonations { get; set; } = new List<Eventdonation>();

    public virtual Event Events { get; set; }

    public virtual User User { get; set; }
}
