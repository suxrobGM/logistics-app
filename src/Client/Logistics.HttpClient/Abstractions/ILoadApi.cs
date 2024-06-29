using Logistics.HttpClient.Models;
using Logistics.Shared;
using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface ILoadApi
{
    Task<Result<LoadDto>> GetLoadAsync(string id);
    Task<PagedResult<LoadDto>> GetLoadsAsync(GetLoadsQuery query);
    Task<Result<ICollection<LoadDto>>> GetDriverActiveLoadsAsync(string userId);
    Task<Result> CreateLoadAsync(CreateLoad command);
    Task<Result> UpdateLoadAsync(UpdateLoad command);
    Task<Result> DeleteLoadAsync(string id);
}
