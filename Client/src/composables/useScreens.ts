import { computed, ref } from "vue";
import { Screen } from "@/components/ScreensViewer.vue"; 
import { getScreens } from "@/global";
import { useQuery } from "@tanstack/vue-query";

const selectedScreens = ref<Screen[]>();

export const useScreens = () => {

  const { data: screenDtos, isFetching, isError } = useQuery({
    queryKey: ['screens'],
    queryFn: getScreens,
    staleTime: Infinity,
    refetchOnMount: false,
  });

  const screens = computed<Screen[]>(() => { 
    return screenDtos.value?.map((x, index) => ({
      index: index + 1,
      id: x.devicePath,
      label: x.label,
      isSelected: selectedScreens.value?.some(y => y.id == x.devicePath) ?? false,
      width: x.width,
      height: x.height,
      x: x.x,
      y: x.y,
      isDDCSupported: x.isDDCSupported,
      isBrightnessSupported: x.isBrightnessSupported,
      brightnessMinimum: x.minBrightness,
      brightnessMaximum: x.maxBrightness,
      currentBrightness: x.currentBrightness,
    } satisfies Screen)) ?? []; 
  });

  return { selectedScreens, screens, isFetching, isError };
};