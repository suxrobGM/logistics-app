namespace Logistics.Application.Mappers;

internal class EmployeeProfile : Profile
{
    public EmployeeProfile()
    {
        CreateMap<Employee, CreateEmployeeCommand>().ReverseMap();
        CreateMap<Employee, UpdateEmployeeCommand>().ReverseMap();
        CreateMap<EmployeeDto, CreateEmployeeCommand>().ReverseMap();
        CreateMap<EmployeeDto, UpdateEmployeeCommand>().ReverseMap();
    }
}
