using System;
using System.Collections.Generic;

namespace Bokhandel.Models;

public partial class Publisher
{
    public int PublisherId { get; set; }

    public string Name { get; set; } = null!;

    public string? Adress { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
