<template>
    <div
        class="lg:w-2/4 md:w-3/4 mt-6 p-6 mx-auto bg-zinc-800 text-zinc-300 border border-zinc-400 text-lg flex flex-col justify-center items-center">
        <v-progress-circular indeterminate></v-progress-circular>
        <span>{{ $t("messages.signing-in") }}</span>
    </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { useAlertStore } from "@/stores/alert";
import { useSessionStore } from "@/stores/session";
import { useI18n } from "vue-i18n";
import router from "@/router";
import { useRoute } from 'vue-router';

const { t } = useI18n({ useScope: 'global' });
const alertStore = useAlertStore();

onMounted(async () => {
    const sessionStore = useSessionStore();
    const route = useRoute();

    if (!!route.query.error) {
        alertStore.add({
            message: t("exceptions.oauth-error", { error: route.query.error_description }),
            type: "error",
            name: "oauth-error"
        });

        router.push({ name: "sign-in" });

        return;
    }

    const state = (route.query.state?.toString() || "");
    const code = (route.query.code?.toString() || "");

    try {
        await sessionStore.oauth({
            state: state,
            code: code
        });

        router.push({ name: "events" });
    } catch (error: any) {
        alertStore.add({
            message: error.response?.status == 400 ? t("exceptions.invalid-credentials", { reason: error.response?.data }) : error.toString(),
            type: "error",
            name: "sign-in-exception"
        });

        router.push({ name: "sign-in" });
    }
})

</script>
