using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface ILoadApi
{
    Task<Result<LoadDto>> GetLoadAsync(Guid id);
    Task<PagedResult<LoadDto>> GetLoadsAsync(GetLoadsQuery query);
    Task<Result<ICollection<LoadDto>>> GetDriverActiveLoadsAsync(Guid userId);
    Task<Result> CreateLoadAsync(CreateLoad command);
    Task<Result> UpdateLoadAsync(UpdateLoad command);
    Task<Result> DeleteLoadAsync(Guid id);
}
