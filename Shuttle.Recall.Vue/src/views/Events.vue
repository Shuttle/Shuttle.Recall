<template>
  <r-filter-drawer @filter="refreshEvents">
    <v-text-field :label="$t('id')" v-model="specification.id" hide-details></v-text-field>
    <v-text-field :label="$t('sequence-number-start')" v-model="specification.sequenceNumberStart"
      hide-details></v-text-field>
    <v-text-field :label="$t('maximum-rows')" v-model="specification.maximumRows" hide-details></v-text-field>
    <v-select v-model="specification.eventTypes" multiple :label="$t('event-type')" :items="eventTypes"
      item-title="typeName" item-value="typeName" hide-details clearable>
      <template v-slot:selection="{ item, index }">
        <v-chip v-if="index < 2" class="whitespace-nowrap">
          <span>{{ item.title }}</span>
        </v-chip>
        <span v-if="index === 2" class="text-grey text-caption align-self-center">
          (+{{ specification.eventTypes!.length - 2 }})
        </span>
      </template>
    </v-select>
  </r-filter-drawer>
  <v-card flat>
    <v-card-title>
      <r-title :title="$t('events')"></r-title>
      <div class="mb-2">
        <v-text-field v-model="search" :label="$t('search')" :prepend-inner-icon="mdiMagnify" flat hide-details
          density="compact" single-line></v-text-field>
      </div>
    </v-card-title>
    <v-divider></v-divider>
    <v-data-table :items="events" :headers="headers" :mobile="null" mobile-breakpoint="md" v-model:search="search"
      :loading="busy" :row-props="rowProps" item-selectable v-model="selected" return-object show-select>
      <template v-slot:header.sequenceNumber>
        #{{ sequenceNumberStartDisplay }}-{{ sequenceNumberEndDisplay }}
      </template>
      <template v-slot:header.action="">
        <div class="s-strip my-2">
          <v-btn v-if="selected.length" :icon="mdiTrashCanOutline" size="x-small" @click.stop="remove()"></v-btn>
        </div>
      </template>
      <template v-slot:item.domainEvent="{ value }">
        <pre class="font-mono font-thin">{{ value }}</pre>
      </template>
    </v-data-table>
  </v-card>
</template>

<script setup lang="ts">
import { mdiMagnify, mdiTrashCanOutline } from '@mdi/js';
import { useI18n } from "vue-i18n";
import { recallApi } from '@/api'
import type { Event, EventStoreResponse, EventType, EventSpecification } from '@/recall';
import { useAlertStore } from '@/stores/alert';
import { useSessionStore } from '@/stores/session';
import { useConfirmationStore } from '@/stores/confirmation';

const alertStore = useAlertStore();
const sessionStore = useSessionStore();
const confirmationStore = useConfirmationStore();

const { t } = useI18n({ useScope: 'global' });
const search = ref('');
const busy = ref(false);
const sequenceNumberStartDisplay = ref(0);
const sequenceNumberEndDisplay = ref(0);
const sequenceNumberStart = ref(0);
const specification: Ref<EventSpecification> = ref({})
const selected: Ref<Event[]> = ref([]);

const headers: any = [
  {
    value: "action",
    headerProps: {
      class: "w-1"
    }
  },
  {
    title: "#",
    key: "sequenceNumber",
    value: "primitiveEvent.sequenceNumber",
    headerProps: {
      class: "w-2 whitespace-nowrap"
    },
    cellProps: {
      class: "py-2"
    }
  },
  {
    title: t("id"),
    value: "primitiveEvent.id",
    headerProps: {
      class: "w-80"
    },
    cellProps: {
      class: "py-2"
    }
  },
  {
    title: t("version"),
    value: "primitiveEvent.version",
    headerProps: {
      class: "w-2"
    },
    cellProps: {
      class: "py-2"
    }
  },
  {
    title: t("event-type"),
    value: "primitiveEvent.eventType",
    headerProps: {
      class: "w-80"
    },
    cellProps: {
      class: "py-2"
    }
  },
  {
    title: t("domain-event"),
    key: "domainEvent",
    value: (item: any): any => JSON.stringify(JSON.parse(item.domainEvent), null, 2),
    cellProps: {
      class: "py-2"
    }
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

const refreshEvents = async () => {
  busy.value = true;

  events.value = [];

  try {
    if (!specification.value.id) {
      specification.value.id = undefined
    }

    const { data } = await recallApi.post<EventStoreResponse<Event>>('/events/search', specification.value);

    events.value = data.items;

    if (events.value.length > 0) {
      sequenceNumberStartDisplay.value = events.value[0].primitiveEvent.sequenceNumber;
      sequenceNumberEndDisplay.value = events.value[events.value.length - 1].primitiveEvent.sequenceNumber;
      sequenceNumberStart.value = sequenceNumberEndDisplay.value + 1;
    }
  } finally {
    busy.value = false;
  };
}

const refreshEventTypes = async () => {
  busy.value = true;

  eventTypes.value = [];

  try {
    const { data } = await recallApi.post<EventStoreResponse<EventType>>('/event-types/search', {})

    eventTypes.value = data.items;
  } finally {
    busy.value = false;
  };
}

const remove = async () => {
  if (!selected.value.length || !(await confirmationStore.show({ messageKey: 'confirmation.remove' }))
    .confirmed) {
    return;
  }

  busy.value = true;

  eventTypes.value = [];

  try {
    const { data } = await recallApi.post('/events/delete', { sequenceNumbers: selected.value?.map((item: Event) => item.primitiveEvent?.sequenceNumber) ?? [] })

    eventTypes.value = data.items;
  } finally {
    busy.value = false;
  };

  await refreshEvents();
}

onMounted(() => {
  refreshEvents();
  refreshEventTypes();
})
</script>
