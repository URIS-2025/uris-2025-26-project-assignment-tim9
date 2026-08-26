export async function getWorkPackages(projectId) {
  const response = await fetch(`http://localhost:5037/api/projects/${projectId}/work-packages`);

  if (!response.ok) {
    throw new Error(`Greška pri učitavanju work package-a: ${response.status}`);
  }

  return response.json();
}