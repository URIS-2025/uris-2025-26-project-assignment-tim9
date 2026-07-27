using WorkPackageService.Models.DTO.CommentDTOs;

namespace WorkPackageService.Data
{
    public interface ICommentRepository
    {
        IEnumerable<CommentDisplayDTO> GetAll();
        IEnumerable<CommentDisplayDTO> GetByTaskId(Guid taskId);
        CommentDisplayDTO? GetById(Guid id);
        CommentDisplayDTO Add(CommentCreateDTO dto);
        CommentDisplayDTO? Update(Guid commentId, Guid callerId, CommentUpdateDTO dto);
        bool Delete(Guid commentId, Guid callerId);
        bool SaveChanges();
    }
}
