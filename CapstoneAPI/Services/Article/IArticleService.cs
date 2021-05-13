using CapstoneAPI.DataSets.Article;
using CapstoneAPI.Filters;
using CapstoneAPI.Filters.Article;
using CapstoneAPI.Wrappers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Article
{
    public interface IArticleService
    {
        Task<PagedResponse<List<ArticleCollapseDataSet>>> GetListArticleForGuest(PaginationFilter validFilter);
        Task<PagedResponse<List<AdminArticleCollapseDataSet>>> GetListArticleForAdmin(PaginationFilter validFilter);
        Task<PagedResponse<List<AdminArticleCollapseDataSet>>> GetListArticleForAdmin(PaginationFilter validFilter, 
            AdminArticleFilter articleFilter);
        Task<Response<ArticleDetailDataSet>> GetArticleById(int id);
        Task<Response<AdminArticleDetailDataSet>> AdminGetArticleById(int id);
        Task<Response<ApprovingArticleDataSet>> ApprovingArticle(ApprovingArticleDataSet approvingArticleDataSet, string token);
        Task<Response<List<int>>> GetUnApprovedArticleIds();
    }
}