using Logistics.Models;

namespace Logistics.Client.Abstractions;

public interface ILoadApi
{
    Task<ResponseResult<LoadDto>> GetLoadAsync(string id);
    Task<PagedResponseResult<LoadDto>> GetLoadsAsync(GetLoadsQuery query);
    Task<ResponseResult> CreateLoadAsync(CreateLoad load);
    Task<ResponseResult> UpdateLoadAsync(UpdateLoad load);
    Task<ResponseResult> DeleteLoadAsync(string id);
}
