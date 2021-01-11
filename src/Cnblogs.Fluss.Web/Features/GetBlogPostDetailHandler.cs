using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cnblogs.Fluss.Domain;
using Cnblogs.Fluss.Web.Models;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cnblogs.Fluss.Web.Features
{
    public class GetBlogPostDetailHandler : IRequestHandler<GetBlogPostDetail, BlogPostDetailViewModel>
    {
        private readonly IBlogSiteRepository _blogSiteRepository;

        public GetBlogPostDetailHandler(IBlogSiteRepository blogSiteRepository)
        {
            _blogSiteRepository = blogSiteRepository;
        }

        /// <inheritdoc />
        public async Task<BlogPostDetailViewModel> Handle(
            GetBlogPostDetail request,
            CancellationToken cancellationToken)
        {
            var post = await _blogSiteRepository.NoTrackingQueryable
                .Where(b => b.Id == request.BlogId)
                .SelectMany(b => b.BlogPosts)
                .Include(p => p.ContentRecords)
                .ThenInclude(c => c.ContentBlock)
                .ThenInclude(c => c.ReferringBlock)
                .Where(p => p.Id == request.PostId)
                .FirstOrDefaultAsync(cancellationToken);
            var model = post.Adapt<BlogPostDetailViewModel>();
            model.Content = post.ContentRecords
                .OrderBy(c => c.Order)
                .Aggregate(
                    new StringBuilder(),
                    (sb, c) => c.ContentBlock.ReferringBlock == null
                        ? sb.AppendLine(c.ContentBlock.Content)
                        : sb.AppendLine(c.ContentBlock.ReferringBlock.Content))
                .ToString();
            return model;
        }
    }
}