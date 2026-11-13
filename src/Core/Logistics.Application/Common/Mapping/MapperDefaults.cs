using Riok.Mapperly.Abstractions;

// Only an unfilled DTO field is a bug; an entity column the DTO leaves out is normal.
[assembly: MapperDefaults(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
