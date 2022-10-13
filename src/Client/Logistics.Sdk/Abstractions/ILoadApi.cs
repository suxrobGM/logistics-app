namespace Logistics.Sdk;

public interface ILoadApi
{
    Task<ResponseResult<LoadDto>> GetLoadAsync(string id);
    Task<PagedResponseResult<LoadDto>> GetLoadsAsync(string searchInput = "", int page = 1, int pageSize = 10);
    Task<ResponseResult> CreateLoadAsync(LoadDto load);
    Task<ResponseResult> UpdateLoadAsync(LoadDto load);
    Task<ResponseResult> DeleteLoadAsync(string id);
}
