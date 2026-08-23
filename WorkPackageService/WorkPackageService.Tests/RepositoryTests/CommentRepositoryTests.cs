using WorkPackageService.Context;
using WorkPackageService.Data;
using WorkPackageService.Exceptions;
using WorkPackageService.Models.DTO.CommentDTOs;
using WorkPackageService.Tests.TestHelpers;
using CommentEntity = WorkPackageService.Models.Comment;

namespace WorkPackageService.Tests.RepositoryTests
{
    public class CommentRepositoryTests
    {
        private static CommentRepository CreateRepository(WorkPackageServiceContext context)
        {
            var mapper = DbContextFactory.CreateMapper();
            return new CommentRepository(context, mapper);
        }

        [Fact]
        public void Update_WhenCallerIsAuthor_UpdatesComment()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var authorId = Guid.NewGuid();
            var commentId = Guid.NewGuid();

            context.Comments.Add(new CommentEntity
            {
                CommentId = commentId,
                TaskId = Guid.NewGuid(),
                AuthorId = authorId,
                Text = "Original",
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var repository = CreateRepository(context);
            var dto = new CommentUpdateDTO { Id = commentId, Text = "Updated" };

            // Act
            var result = repository.Update(commentId, authorId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated", result!.Text);
        }

        [Fact]
        public void Update_WhenCallerIsNotAuthor_ThrowsUnauthorizedOperationException()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var authorId = Guid.NewGuid();
            var wrongCallerId = Guid.NewGuid();
            var commentId = Guid.NewGuid();

            context.Comments.Add(new CommentEntity
            {
                CommentId = commentId,
                TaskId = Guid.NewGuid(),
                AuthorId = authorId,
                Text = "Original",
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var repository = CreateRepository(context);
            var dto = new CommentUpdateDTO { Id = commentId, Text = "Updated" };

            // Act + Assert
            Assert.Throws<UnauthorizedOperationException>(() => repository.Update(commentId, wrongCallerId, dto));
        }

        [Fact]
        public void Delete_WhenCallerIsAuthor_DeletesComment()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var authorId = Guid.NewGuid();
            var commentId = Guid.NewGuid();

            context.Comments.Add(new CommentEntity
            {
                CommentId = commentId,
                TaskId = Guid.NewGuid(),
                AuthorId = authorId,
                Text = "Original",
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var repository = CreateRepository(context);

            // Act
            var deleted = repository.Delete(commentId, authorId);

            // Assert
            Assert.True(deleted);
            Assert.Null(context.Comments.Find(commentId));
        }

        [Fact]
        public void Delete_WhenCallerIsNotAuthor_ThrowsUnauthorizedOperationException()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var authorId = Guid.NewGuid();
            var wrongCallerId = Guid.NewGuid();
            var commentId = Guid.NewGuid();

            context.Comments.Add(new CommentEntity
            {
                CommentId = commentId,
                TaskId = Guid.NewGuid(),
                AuthorId = authorId,
                Text = "Original",
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var repository = CreateRepository(context);

            // Act + Assert
            Assert.Throws<UnauthorizedOperationException>(() => repository.Delete(commentId, wrongCallerId));
        }
    }
}
