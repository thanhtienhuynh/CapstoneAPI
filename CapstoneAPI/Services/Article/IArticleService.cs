﻿using CapstoneAPI.DataSets.Article;
using CapstoneAPI.Filters;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Article
{
    public interface IArticleService
    {
        Task<PagedResponse<List<ArticleCollapseDataSet>>> GetListArticleForGuest(PaginationFilter validFilter);
        Task<PagedResponse<List<AdminArticleCollapseDataSet>>> GetListArticleForAdmin(PaginationFilter validFilter);
        Task<ArticleDetailDataSet> GetArticleById(int id);
        Task<Response<AdminArticleDetailDataSet>> AdminGetArticleById(int id);

    }
}
