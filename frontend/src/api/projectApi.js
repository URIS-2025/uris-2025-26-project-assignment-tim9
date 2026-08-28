import { apiRequest } from './httpClient';

export function getProjects(token) {
  return apiRequest('/api/project', { token }).then((r) => r || []);
}

export function getProjectsByStatus(status, token) {
  return apiRequest(`/api/project/status/${status}`, { token }).then((r) => r || []);
}

export function getProjectsByUserId(userId, token) {
  return apiRequest(`/api/project/user/${userId}`, { token }).then((r) => r || []);
}

export function getProjectById(projectId, token) {
  return apiRequest(`/api/project/${projectId}`, { token });
}

export function createProject(project, token) {
  return apiRequest('/api/project', { method: 'POST', token, body: project });
}

export function updateProject(project, token) {
  return apiRequest('/api/project', { method: 'PUT', token, body: project });
}

export function deleteProject(projectId, token) {
  return apiRequest(`/api/project/${projectId}`, { method: 'DELETE', token });
}

export function getMilestones(token) {
  return apiRequest('/api/milestone', { token }).then((r) => r || []);
}

export function getMilestonesByProjectId(projectId, token) {
  return apiRequest(`/api/milestone/project/${projectId}`, { token }).then((r) => r || []);
}

export function getMilestoneById(milestoneId, token) {
  return apiRequest(`/api/milestone/${milestoneId}`, { token });
}

export function createMilestone(milestone, token) {
  return apiRequest('/api/milestone', { method: 'POST', token, body: milestone });
}

export function updateMilestone(milestone, token) {
  return apiRequest('/api/milestone', { method: 'PUT', token, body: milestone });
}

export function deleteMilestone(milestoneId, token) {
  return apiRequest(`/api/milestone/${milestoneId}`, { method: 'DELETE', token });
}

export function getRequirements(token) {
  return apiRequest('/api/requirements', { token }).then((r) => r || []);
}

export function getRequirementsByProjectId(projectId, token) {
  return apiRequest(`/api/requirements/project/${projectId}`, { token }).then((r) => r || []);
}

export function getRequirementById(requirementId, token) {
  return apiRequest(`/api/requirements/${requirementId}`, { token });
}

export function createRequirement(requirement, token) {
  return apiRequest('/api/requirements', { method: 'POST', token, body: requirement });
}

export function updateRequirement(requirement, token) {
  return apiRequest('/api/requirements', { method: 'PUT', token, body: requirement });
}

export function deleteRequirement(requirementId, token) {
  return apiRequest(`/api/requirements/${requirementId}`, { method: 'DELETE', token });
}

export function getProjectMembers(token) {
  return apiRequest('/api/projectmember', { token }).then((r) => r || []);
}

export function getProjectMembersByProjectId(projectId, token) {
  return apiRequest(`/api/projectmember/project/${projectId}`, { token }).then((r) => r || []);
}

export function getProjectMemberById(projectMemberId, token) {
  return apiRequest(`/api/projectmember/${projectMemberId}`, { token });
}

export function createProjectMember(projectMember, token) {
  return apiRequest('/api/projectmember', { method: 'POST', token, body: projectMember });
}

export function updateProjectMember(projectMember, token) {
  return apiRequest('/api/projectmember', { method: 'PUT', token, body: projectMember });
}

export function deleteProjectMember(projectMemberId, token) {
  return apiRequest(`/api/projectmember/${projectMemberId}`, { method: 'DELETE', token });
}
