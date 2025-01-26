import { computed, ref } from "vue";
import { getScreens } from "@/global";
import { useQuery, UseQueryOptions } from "@tanstack/vue-query";
import { ScreenDto } from "@/dtos/screenDto";

export interface Screen {
  index: number;
  id: string;
  label: string;
  width: number;
  height: number;
  x: number;
  y: number;
  /** the width in percentages. */
  widthPercentage: number;
  /** the height in percentages. */
  heightPercentage: number;
  /** the X position in percentages. */
  XPercentage: number;
  /** the Y position in percentages. */
  YPercentage: number;
  isDDCSupported?: boolean;
  isBrightnessSupported?: boolean;
  brightnessMinimum?: number;
  brightnessMaximum?: number;
  currentBrightness?: number;
}

const selectedScreens = ref<string[]>([]);

export const useScreens = (options?: Partial<UseQueryOptions<ScreenDto[], Error, ScreenDto[], ScreenDto[], string[]>>) => {

  const query = useQuery({
    queryKey: ['screens'],
    queryFn: getScreens,
    staleTime: Infinity,
    refetchOnMount: false,
    ...options
  });

  const aspectRatio = ref<string>("");

  const screens = computed<Screen[]>(() => { 

    let rectangleLeftBorderPosition = 0;
    let rectangleRightBorderPosition = 0;
    let rectangleTopBorderPosition = 0;
    let rectangleBottomBorderPosition = 0;

    let maximumX = 0;
    let maximumY = 0;

    for (const screen of (query.data.value ?? [])) {
      if (screen.x < rectangleLeftBorderPosition) rectangleLeftBorderPosition = screen.x;
      if (screen.x + screen.width > rectangleRightBorderPosition) rectangleRightBorderPosition = screen.x + screen.width;
      if (screen.y < rectangleTopBorderPosition) rectangleTopBorderPosition = screen.y;
      if (screen.y + screen.height > rectangleBottomBorderPosition) rectangleBottomBorderPosition = screen.y + screen.height;
      if (screen.x > maximumX) maximumX = screen.x;
      if (screen.y > maximumY) maximumY = screen.y;
    }

    const rectangleWidth = rectangleRightBorderPosition - rectangleLeftBorderPosition;
    const rectangleHeight = rectangleBottomBorderPosition - rectangleTopBorderPosition;

    aspectRatio.value = `${rectangleWidth} / ${rectangleHeight}`;
      
    return query.data.value?.map((x, index) => ({
      index: index + 1,
      id: x.devicePath,
      label: x.label,
      width: x.width,
      height: x.height,
      x: x.x,
      y: x.y,
      widthPercentage: x.width * 100 / rectangleWidth,
      heightPercentage: x.height * 100 / rectangleHeight,
      XPercentage: (x.x - rectangleLeftBorderPosition) * (maximumX / rectangleWidth * 100) / maximumX,
      YPercentage: (x.y - rectangleTopBorderPosition) * (maximumY / rectangleHeight * 100) / maximumY,
      isDDCSupported: x.isDDCSupported,
      isBrightnessSupported: x.isBrightnessSupported,
      brightnessMinimum: x.minBrightness,
      brightnessMaximum: x.maxBrightness,
      currentBrightness: x.currentBrightness,
    } satisfies Screen)) ?? []; 
  });

  return { selectedScreens, screens, aspectRatio, ...query };
};