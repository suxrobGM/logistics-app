namespace Logistics.Sdk;

public interface ILoadApi
{
    Task<DataResult<LoadDto>> GetLoadAsync(string id);
    Task<PagedDataResult<LoadDto>> GetLoadsAsync(string searchInput = "", int page = 1, int pageSize = 10);
    Task<DataResult> CreateLoadAsync(LoadDto load);
    Task<DataResult> UpdateLoadAsync(LoadDto load);
    Task<DataResult> DeleteLoadAsync(string id);
}
