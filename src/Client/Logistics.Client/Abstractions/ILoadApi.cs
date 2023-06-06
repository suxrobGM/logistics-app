using Logistics.Client.Models;

namespace Logistics.Client.Abstractions;

public interface ILoadApi
{
    Task<ResponseResult<Load>> GetLoadAsync(string id);
    Task<PagedResponseResult<Load>> GetLoadsAsync(SearchableQuery query);
    Task<ResponseResult> CreateLoadAsync(CreateLoad load);
    Task<ResponseResult> UpdateLoadAsync(UpdateLoad load);
    Task<ResponseResult> DeleteLoadAsync(string id);
}
