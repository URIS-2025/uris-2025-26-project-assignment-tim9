import ProjectCardSkeleton from './ProjectCardSkeleton';

// The full loading grid: `count` placeholder cards laid out like the real list.
export default function ProjectListSkeleton({ count = 6 }) {
  return (
    <div className="projects-grid" aria-hidden="true">
      {Array.from({ length: count }).map((_, index) => (
        <ProjectCardSkeleton key={index} />
      ))}
    </div>
  );
}
