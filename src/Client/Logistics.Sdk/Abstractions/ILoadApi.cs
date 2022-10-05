namespace Logistics.Sdk;

public interface ILoadApi
{
    Task<LoadDto> GetLoadAsync(string id);
    Task<PagedDataResult<LoadDto>> GetLoadsAsync(string searchInput = "", int page = 1, int pageSize = 10);
    Task CreateLoadAsync(LoadDto load);
    Task UpdateLoadAsync(LoadDto load);
    Task DeleteLoadAsync(string id);
}
