using Logistics.Client.Models;
using Logistics.Shared;
using Logistics.Shared.Models;

namespace Logistics.Client.Abstractions;

public interface ILoadApi
{
    Task<ResponseResult<LoadDto>> GetLoadAsync(string id);
    Task<PagedResponseResult<LoadDto>> GetLoadsAsync(GetLoadsQuery query);
    Task<ResponseResult<ICollection<LoadDto>>> GetDriverActiveLoadsAsync(string userId);
    Task<ResponseResult> CreateLoadAsync(CreateLoad command);
    Task<ResponseResult> UpdateLoadAsync(UpdateLoad command);
    Task<ResponseResult> DeleteLoadAsync(string id);
}
