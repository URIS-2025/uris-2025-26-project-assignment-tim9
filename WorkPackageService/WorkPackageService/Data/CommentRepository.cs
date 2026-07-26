using AutoMapper;
using WorkPackageService.Context;
using WorkPackageService.Exceptions;
using WorkPackageService.Models;
using WorkPackageService.Models.DTO.CommentDTOs;

namespace WorkPackageService.Data
{
    public class CommentRepository : ICommentRepository
    {
        private readonly WorkPackageServiceContext _context;
        private readonly IMapper _mapper;

        public CommentRepository(WorkPackageServiceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        public IEnumerable<CommentDisplayDTO> GetAll()
        {
            var entities = _context.Comments.ToList();
            return _mapper.Map<IEnumerable<CommentDisplayDTO>>(entities);
        }

        public CommentDisplayDTO? GetById(Guid id)
        {
            var entity = _context.Comments.FirstOrDefault(c => c.CommentId == id);
            if (entity == null) return null;
            return _mapper.Map<CommentDisplayDTO>(entity);
        }

        public CommentDisplayDTO Add(CommentCreateDTO dto)
        {
            var entity = _mapper.Map<Comment>(dto);
            entity.CommentId = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;

            _context.Comments.Add(entity);
            return _mapper.Map<CommentDisplayDTO>(entity);
        }

        // Autor komentara je jedini koji moze da ga izmeni.
        public CommentDisplayDTO? Update(Guid commentId, Guid callerId, CommentUpdateDTO dto)
        {
            var entity = _context.Comments.FirstOrDefault(c => c.CommentId == commentId);
            if (entity == null) throw new EntityNotFoundException($"Comment sa Id-jem {commentId} ne postoji.");
            if (entity.AuthorId != callerId) throw new UnauthorizedOperationException("Samo autor komentara moze da ga izmeni.");

            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            return _mapper.Map<CommentDisplayDTO>(entity);
        }

        // Autor komentara je jedini koji moze da ga obrise.
        public bool Delete(Guid commentId, Guid callerId)
        {
            var entity = _context.Comments.FirstOrDefault(c => c.CommentId == commentId);
            if (entity == null) throw new EntityNotFoundException($"Comment sa Id-jem {commentId} ne postoji.");
            if (entity.AuthorId != callerId) throw new UnauthorizedOperationException("Samo autor komentara moze da ga obrise.");

            _context.Comments.Remove(entity);
            return true;
        }
    }
}
