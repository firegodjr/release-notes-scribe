export interface ConfigVariable {
  name: string
  isSet: boolean
  isSecret: boolean
  displayValue: string | null
}

export interface ConfigStatusResponse {
  isConfigured: boolean
  variables: ConfigVariable[]
}

export interface WorkItemDto {
  id: number
  title: string
  workItemType: string
  state: string
  description: string | null
  acceptanceCriteria: string | null
  iterationPath: string
}

export interface WorkItemQueryRequest {
  iterationPath: string
  onlyClosed: boolean
}

export interface WorkItemQueryResponse {
  items: WorkItemDto[]
  normalizedPaths: string[]
}

export interface GenerateRequest {
  versionLabel: string
  items: WorkItemDto[]
}

export interface GenerateResponse {
  markdown: string
}

export interface SelectableWorkItem extends WorkItemDto {
  selected: boolean
}
