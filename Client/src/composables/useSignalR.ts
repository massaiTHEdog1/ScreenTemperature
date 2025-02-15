import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { onUnmounted, ref } from "vue";

const connection = ref<HubConnection>(new HubConnectionBuilder()
  .withUrl(`${import.meta.env.VITE_SERVER_BASE_URL}/hub`)
  .withAutomaticReconnect()
  //.configureLogging(LogLevel.Trace)
  .build());

connection.value.start();

export const useSignalR = () => {

  const registrations = ref<{ methodName: string, action: (...args: any) => unknown }[]>([]);

  const on = (methodName: string, action: (...args: any) => unknown) => {

    connection.value.on(methodName, action);

    registrations.value.push({
      methodName,
      action
    });
  };

  onUnmounted(() => {
    for(const element of registrations.value)
    {
      connection.value.off(element.methodName);
    }
  });

  return { connection, on };
};