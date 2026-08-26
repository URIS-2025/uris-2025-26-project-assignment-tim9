// TEMPORARY: WorkPackageService's Task has no SprintId (and no
// GET /api/task/sprint/{id}) yet - see the handoff spec for the
// WorkPackageService owner. Until that lands, "which tasks belong to which
// sprint" is tracked here, per-browser, in localStorage - not on the backend.
//
// Swap-out plan once the real endpoint exists: delete this file, replace
// getMockTasksForSprint(sprintId) with `getTasksBySprint(sprintId, token)`
// from ../api/taskApi, and drop the addMockTaskToSprint call in
// TaskFormModal (pass sprintId straight through to createTask instead).

const STORAGE_KEY = 'mock.sprintTaskLinks';

function loadStore() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : {};
  } catch {
    return {};
  }
}

function saveStore(store) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(store));
  } catch {
    // Best-effort only - a private window or full storage just means no seed/persistence.
  }
}

function exampleTasksFor(sprintId) {
  return [
    {
      taskId: `mock-${sprintId}-1`,
      title: 'Example task (mock)',
      description: 'Placeholder task - WorkPackageService does not link tasks to sprints yet.',
      status: 0,
      priority: 1,
      dueDate: null,
    },
    {
      taskId: `mock-${sprintId}-2`,
      title: 'Another example task (mock)',
      description: null,
      status: 1,
      priority: 2,
      dueDate: null,
    },
  ];
}

// Reads the mock tasks for a sprint, seeding two example tasks the first
// time a given sprint is opened so the page isn't empty out of the box.
export function getMockTasksForSprint(sprintId) {
  const store = loadStore();
  if (!store[sprintId]) {
    store[sprintId] = exampleTasksFor(sprintId);
    saveStore(store);
  }
  return store[sprintId];
}

// Records a real task (created via the real POST /api/task call) as
// belonging to this sprint, locally only.
export function addMockTaskToSprint(sprintId, task) {
  const store = loadStore();
  const existing = store[sprintId] || [];
  store[sprintId] = [...existing, task];
  saveStore(store);
  return store[sprintId];
}

// Drops one task from a sprint's local list - either because the underlying
// real task was deleted, or because it was one of the seeded examples.
export function removeMockTaskFromSprint(sprintId, taskId) {
  const store = loadStore();
  const existing = store[sprintId] || [];
  store[sprintId] = existing.filter((t) => t.taskId !== taskId);
  saveStore(store);
  return store[sprintId];
}

// Drops a sprint's entry entirely - call this when the sprint itself is deleted.
export function removeMockLinksForSprint(sprintId) {
  const store = loadStore();
  delete store[sprintId];
  saveStore(store);
}

// The two seeded example tasks aren't real WorkPackageService rows - only
// call DELETE /api/task/{id} for tasks that don't have this prefix.
export function isMockOnlyTask(taskId) {
  return typeof taskId === 'string' && taskId.startsWith('mock-');
}
