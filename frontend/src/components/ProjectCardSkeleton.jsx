// Loading placeholder for one card
export default function ProjectCardSkeleton() {
  return (
    <article className="project-card project-card--skeleton">
      <div className="project-card__head">
        <span className="pl-skeleton pl-skeleton--title" />
        <span className="pl-skeleton pl-skeleton--pill" />
      </div>
      <dl className="project-card__meta">
        <div className="project-meta">
          <span className="pl-skeleton pl-skeleton--line is-short" />
          <span className="pl-skeleton pl-skeleton--line" />
        </div>
        <div className="project-meta">
          <span className="pl-skeleton pl-skeleton--line is-short" />
          <span className="pl-skeleton pl-skeleton--line" />
        </div>
      </dl>
    </article>
  );
}
