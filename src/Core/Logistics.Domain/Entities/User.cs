using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class User : IdentityUser<string>, IAggregateRoot
{
    public override string Id { get; set; } = Generator.NewGuid();
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.Now;
    public Truck? Truck { get; set; }
    public IList<Cargo> Cargoes { get; set; } = new List<Cargo>();
}