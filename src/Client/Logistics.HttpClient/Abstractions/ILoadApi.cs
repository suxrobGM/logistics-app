using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface ILoadApi
{
    Task<LoadDto?> GetLoadAsync(Guid id);
    Task<PagedResponse<LoadDto>?> GetLoadsAsync(GetLoadsQuery query);
    Task<ICollection<LoadDto>?> GetDriverActiveLoadsAsync(Guid userId);
    Task<bool> CreateLoadAsync(CreateLoadCommand command);
    Task<bool> UpdateLoadAsync(UpdateLoadCommand command);
    Task<bool> DeleteLoadAsync(Guid id);
}
