<template>
  <div>
    <div class="text-body-2 font-weight-medium mb-2">TTL (optional)</div>
    <div class="d-flex ga-2 align-center">
      <v-text-field
        v-model.number="days"
        density="compact"
        hide-details
        label="Days"
        min="0"
        style="max-width: 80px;"
        type="number"
        variant="outlined"
      />
      <v-text-field
        v-model.number="hours"
        density="compact"
        hide-details
        label="Hours"
        max="23"
        min="0"
        style="max-width: 80px;"
        type="number"
        variant="outlined"
      />
      <v-text-field
        v-model.number="minutes"
        density="compact"
        hide-details
        label="Min"
        max="59"
        min="0"
        style="max-width: 80px;"
        type="number"
        variant="outlined"
      />
      <v-text-field
        v-model.number="seconds"
        density="compact"
        hide-details
        label="Sec"
        max="59"
        min="0"
        style="max-width: 80px;"
        type="number"
        variant="outlined"
      />
      <v-btn
        v-if="hasValue"
        icon="mdi-close"
        size="x-small"
        title="Clear TTL"
        variant="text"
        @click="clear"
      />
    </div>
    <div v-if="hasValue" class="text-caption text-medium-emphasis mt-1">
      {{ formattedPreview }}
    </div>
  </div>
</template>

<script setup lang="ts">
  const model = defineModel<string | null>({ default: null })

  const days = ref(0)
  const hours = ref(0)
  const minutes = ref(0)
  const seconds = ref(0)

  const hasValue = computed(() => days.value > 0 || hours.value > 0 || minutes.value > 0 || seconds.value > 0)

  const formattedPreview = computed(() => {
    const parts: string[] = []
    if (days.value > 0) parts.push(`${days.value}d`)
    if (hours.value > 0) parts.push(`${hours.value}h`)
    if (minutes.value > 0) parts.push(`${minutes.value}m`)
    if (seconds.value > 0) parts.push(`${seconds.value}s`)
    return parts.length ? parts.join(' ') : ''
  })

  // .NET TimeSpan format: [d.]hh:mm:ss
  function toTimespan (): string | null {
    if (!hasValue.value) return null
    const hh = String(hours.value).padStart(2, '0')
    const mm = String(minutes.value).padStart(2, '0')
    const ss = String(seconds.value).padStart(2, '0')
    const dayPrefix = days.value > 0 ? `${days.value}.` : ''
    return `${dayPrefix}${hh}:${mm}:${ss}`
  }

  function clear () {
    days.value = 0
    hours.value = 0
    minutes.value = 0
    seconds.value = 0
  }

  watch([days, hours, minutes, seconds], () => {
    model.value = toTimespan()
  })
</script>
