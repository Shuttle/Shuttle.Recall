<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <div class="sv-title">{{ $t("events") }}</div>
      <div class="mb-2">
        <v-chip>{{ $t("sequence-number-start") }} : {{ sequenceNumberStartDisplay }}</v-chip>
        <v-chip class="ml-2">{{ $t("sequence-number-end") }} : {{ sequenceNumberEndDisplay }}</v-chip>
        <v-chip class="ml-2">{{ $t("event-count") }} : {{ events.length }}</v-chip>
      </div>
      <div class="mb-2">
        <v-text-field v-model="search" :label="$t('search')" :prepend-inner-icon="mdiMagnify" flat hide-details
          density="compact" single-line></v-text-field>
      </div>
      <form class="sv-strip" @submit.prevent="refreshEvents">
        <v-btn :icon="mdiRefresh" size="small" @click="refreshEvents" type="submit"></v-btn>
        <div class="w-48">
          <v-text-field :label="$t('sequence-number-start')" v-model="sequenceNumberStart" hide-details></v-text-field>
        </div>
        <div class="w-32">
          <v-text-field :label="$t('maximum-rows')" v-model="maximumRows" hide-details></v-text-field>
        </div>
        <div class="w-96">
          <v-select v-model="selectedEventTypes" multiple :label="$t('event-type')" :items="eventTypes"
            item-title="typeName" item-value="typeName" :append-icon="mdiRefresh" @click:append="refreshEventTypes"
            hide-details>
            <template v-slot:selection="{ item, index }">
              <v-chip v-if="index < 2">
                <span>{{ item.title }}</span>
              </v-chip>
              <span v-if="index === 2" class="text-grey text-caption align-self-center">
                (+{{ selectedEventTypes.length - 2 }})
              </span>
            </template>
          </v-select>
        </div>
      </form>
    </v-card-title>
    <v-divider></v-divider>
    <v-data-table :items="events" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
      :loading="busy" :row-props="rowProps">
      <template v-slot:item.domainEvent="{ value }">
        <pre class="font-mono font-thin">{{ value }}</pre>
      </template>
    </v-data-table>
  </v-card>
</template>

<script setup lang="ts">
import { mdiMagnify, mdiRefresh } from '@mdi/js';
import { useI18n } from "vue-i18n";
import api from '@/api'
import type { Event, EventStoreResponse, EventType } from '@/recall';
import { useAlertStore } from '@/stores/alert';
import { useSessionStore } from '@/stores/session';

const alertStore = useAlertStore();
const sessionStore = useSessionStore();

const { t } = useI18n({ useScope: 'global' });
const search = ref('');
const busy = ref(false);
const sequenceNumberStartDisplay = ref(0);
const sequenceNumberEndDisplay = ref(0);
const sequenceNumberStart = ref(0);
const maximumRows = ref(0);
const selectedEventTypes: Ref<EventType[]> = ref([]);

const headers: any = [
  {
    title: "#",
    value: "primitiveEvent.sequenceNumber",
    headerProps: {
      class: "w-2"
    },
  },
  {
    title: t("id"),
    value: "primitiveEvent.id",
    headerProps: {
      class: "w-80"
    },
  },
  {
    title: t("version"),
    value: "primitiveEvent.version",
    headerProps: {
      class: "w-2"
    },
  },
  {
    title: t("event-type"),
    value: "primitiveEvent.eventType",
    headerProps: {
      class: "w-80"
    },
  },
  {
    title: t("domain-event"),
    key: "domainEvent",
    value: (item: any): any => JSON.stringify(JSON.parse(item.domainEvent), null, 2),
  }
];

const rowProps = () => {
  return {
    class: {
      'align-top': true
    },
  };
}

const events: Ref<Event[]> = ref([]);
const eventTypes: Ref<EventType[]> = ref([]);

const refreshEvents = () => {
  busy.value = true;

  events.value = [];

  api.post<EventStoreResponse<Event>>('/events/search', {
    sequenceNumberStart: sequenceNumberStart.value,
    maximumRows: maximumRows.value,
    eventTypes: selectedEventTypes.value,
  })
    .then((response) => {
      if (!response.data.authorized) {
        unauthorized();
        return;
      }

      events.value = response.data.items;

      if (events.value.length > 0) {
        sequenceNumberStartDisplay.value = events.value[0].primitiveEvent.sequenceNumber;
        sequenceNumberEndDisplay.value = events.value[events.value.length - 1].primitiveEvent.sequenceNumber;
        sequenceNumberStart.value = sequenceNumberEndDisplay.value + 1;
      }
    })
    .finally(() => {
      busy.value = false;
    });
}

const refreshEventTypes = () => {
  busy.value = true;

  eventTypes.value = [];

  api.post<EventStoreResponse<EventType>>('/eventtypes/search', {})
    .then((response) => {
      if (!response.data.authorized) {
        unauthorized();
        return;
      }

      eventTypes.value = response.data.items;
    })
    .finally(() => {
      busy.value = false;
    });
}

onMounted(() => {
  refreshEvents();
  refreshEventTypes();
})

const unauthorized = () => {
  alertStore.add({
    message: t(sessionStore.authenticated ? "exceptions.insufficient-permission" : "exceptions.session-required"),
    type: "error",
    name: "insufficient-permission",
  });
}
</script>
