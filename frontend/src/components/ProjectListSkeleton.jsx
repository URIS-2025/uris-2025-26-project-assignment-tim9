import ProjectCardSkeleton from './ProjectCardSkeleton';

export default function ProjectListSkeleton({ count = 6 }) {
  return (
    <div className="projects-grid" aria-hidden="true">
      {Array.from({ length: count }).map((_, index) => (
        <ProjectCardSkeleton key={index} />
      ))}
    </div>
  );
}
