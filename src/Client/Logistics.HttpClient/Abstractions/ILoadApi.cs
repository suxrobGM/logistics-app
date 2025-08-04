using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface ILoadApi
{
    Task<Result<LoadDto>> GetLoadAsync(Guid id);
    Task<PagedResult<LoadDto>> GetLoadsAsync(GetLoadsQuery query);
    Task<Result<ICollection<LoadDto>>> GetDriverActiveLoadsAsync(Guid userId);
    Task<Result> CreateLoadAsync(CreateLoadCommand command);
    Task<Result> UpdateLoadAsync(UpdateLoadCommand command);
    Task<Result> DeleteLoadAsync(Guid id);
}
