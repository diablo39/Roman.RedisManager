# Redis Details Page Implementation Plan

## Objectives

- Add a dynamic Redis details route at `/redis/:id` driven by file-based routing (`src/pages/redis/[id].vue`).
- Render two tabs: **Server info** (populated) and **Keys** (placeholder message).
- Mock data fetch per server id until API integration is available.
- Update Menu navigation so server clicks open `/redis/{id}`.

## Routing & Meta

- Create `src/pages/redis/[id].vue`; the filename enables a dynamic `:id` route.
- Include `<route lang="yaml">` block with `layout: default` and `title: "Redis Server"` (optionally append id in runtime for display).
- Use `useRoute()` to read `route.params.id` and drive the mock fetch.

## Data & Mocking

- Shape for mocked detail:
  - `id: string`
  - `name: string`
  - `topology: "Cluster" | "Master-Slave" | "Standalone"`
  - `nodes: { address: string; role: "master" | "slave" | "replica" | "unknown"; hostedShards: number; }[]`
- Implement `const loading = ref(true)`, `error = ref<string | null>(null)`, `server = ref<MockServer | null>(null)`.
- Mock fetch pattern:

  ```ts
  const mockServers: Record<string, MockServer> = {
    '1': {
      id: '1',
      name: 'Primary Cluster',
      topology: 'Cluster',
      nodes: [
        { address: '10.0.0.1:6379', role: 'master', hostedShards: 8 },
        { address: '10.0.0.2:6379', role: 'replica', hostedShards: 8 },
      ],
    },
    // add a couple more entries for variety
  }

  const fetchServer = async () => {
    loading.value = true
    error.value = null
    try {
      await new Promise(resolve => setTimeout(resolve, 300))
      const result = mockServers[id.value]
      if (!result) throw new Error('Not found')
      server.value = result
    } catch (e) {
      error.value = 'Unable to load server details'
      server.value = null
    } finally {
      loading.value = false
    }
  }
  ```

- Call `fetchServer()` on mount and when `id` changes (`watch(() => route.params.id, fetchServer)`), debounced only if necessary (likely not needed now).

## Tabs & State

- Use `const tab = ref<'info' | 'keys'>('info')` bound to `v-tabs` and `v-window`.
- Tabs structure:

  ```vue
  <v-tabs
    v-model="tab"
    density="compact"
  > <v-tab value="info">Server info</v-tab> <v-tab value="keys">Keys</v-tab> </v-tabs>
  <v-window v-model="tab">
    <v-window-item value="info">...server info card...</v-window-item>
    <v-window-item value="keys">...placeholder...</v-window-item>
  </v-window>
  ```

- Keep content inside a `v-card` (tonal or flat) with padding (`class="pa-4"`).

## Server Info Tab UI

- Top summary: name and topology badge.

  ```vue
  <v-card-title class="d-flex align-center justify-space-between">
    <div class="text-h6">{{ server.name }}</div>
    <v-chip color="primary" variant="tonal">{{ server.topology }}</v-chip>
  </v-card-title>
  ```

- Details section (use `v-list` or `v-row`):
  - Name row: label + value.
  - Topology row: label + value.
- Nodes list:

  ```vue
  <v-list density="comfortable">
    <v-list-subheader>Nodes</v-list-subheader>
    <v-list-item
      v-for="node in server.nodes"
      :key="node.address"
      :title="node.address"
      :subtitle="`Role: ${formatRole(node.role)} • Hosted shards: ${node.hostedShards}`"
      prepend-icon="mdi-lan"
    />
  </v-list>
  ```

- Empty nodes handling: show `v-alert` tonal info: "No nodes reported for this server".

## Keys Tab Placeholder

- Simple placeholder card content inside the `keys` window item:

  ```vue
  <div class="d-flex flex-column align-center justify-center py-10 text-medium-emphasis">
    <v-icon icon="mdi-key" size="48" class="mb-3" />
    <div class="text-subtitle-1">Keys view coming soon</div>
    <div class="text-body-2">Browse and manage Redis keys will appear here.</div>
  </div>
  ```

## Loading & Error States

- Before data load: show centered `v-progress-circular` with accessible label (e.g., `aria-label="Loading server"`).
- On error: show `v-alert` (type error, variant tonal) with retry button calling `fetchServer()`.
- Only render tab content when `!loading && !error && server`.

## Menu Navigation Update

- In `src/components/Menu.vue`, change each server list item to navigate to `/redis/${server.id}`.
- Keep `nav` styling and prepend icon. Example:

  ```vue
  <v-list-item prepend-icon="mdi-database" :title="server.name" :to="`/redis/${server.id}`" nav />
  ```

- Remove `selectServer` logging placeholder if unused; otherwise, keep function for future selection side effects but still use `to` for navigation.

## File Touch Points

- Add: `src/pages/redis/[id].vue` (new page).
- Modify: `src/components/Menu.vue` (navigation target update).
- Optional: Update docs if mock data or patterns change.

## Verification

- Manual: click a server in Menu → lands on `/redis/{id}`; see loading then server info; tabs switch; Keys tab shows placeholder.
- Edge: unknown id shows error alert with retry, or redirect plan later.
- Run `npm run type-check` and `npm run lint` after implementation.

## Future Enhancements

- Replace mock with real API call when endpoint available; move fetch to a dedicated composable or Pinia store if reused.
- Add metrics/keys data in second tab and pagination/filtering.
- Surface breadcrumb or dynamic page title using fetched server name.
