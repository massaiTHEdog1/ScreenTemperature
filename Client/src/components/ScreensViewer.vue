<script setup lang="ts">
import { ref, watch, PropType } from 'vue';

export interface Screen {
  index: number;
  id: string;
  label: string;
  width: number;
  height: number;
  x: number;
  y: number;
  isSelected: boolean;
  isDDCSupported?: boolean;
  isBrightnessSupported?: boolean;
  brightnessMinimum?: number;
  brightnessMaximum?: number;
  currentBrightness?: number;
}

interface LocalScreen extends Screen{
  /** Returns the width in percentages. */
  widthPercentage: number;
  /** Returns the height in percentages. */
  heightPercentage: number;
  /** Returns the X position in percentages. */
  XPercentage: number;
  /** Returns the Y position in percentages. */
  YPercentage: number;
}

const props = defineProps({
  /** A list of screen to display. */
  screens: { type: Array as PropType<Screen[]>, required: true },
  /** Enable multi-selection. */
  allowMultipleSelection: { type: Boolean, required: false, default: true },
  /** If true, a click acts as a toggle. */
  clickToggle: { type: Boolean, required: false, default: false },
  /** Allow an empty selection. */
  allowEmptySelection: { type: Boolean, required: false, default: false },
  readonly: { type: Boolean, required: false, default: false },
});

const localScreens = ref<LocalScreen[]>([]);

const emit = defineEmits<{
  selectionChanged: [selection: Screen[]]
}>();

const aspectRatio = ref<string>();

const refresh = () => {
  aspectRatio.value = undefined;

  if (localScreens.value.length === 0) return;

  let rectangleLeftBorderPosition = 0;
  let rectangleRightBorderPosition = 0;
  let rectangleTopBorderPosition = 0;
  let rectangleBottomBorderPosition = 0;

  let maximumX = 0;
  let maximumY = 0;

  for (const screen of localScreens.value) {
    if (screen.x < rectangleLeftBorderPosition) rectangleLeftBorderPosition = screen.x;
    if (screen.x + screen.width > rectangleRightBorderPosition) rectangleRightBorderPosition = screen.x + screen.width;
    if (screen.y < rectangleTopBorderPosition) rectangleTopBorderPosition = screen.y;
    if (screen.y + screen.height > rectangleBottomBorderPosition) rectangleBottomBorderPosition = screen.y + screen.height;
    if (screen.x > maximumX) maximumX = screen.x;
    if (screen.y > maximumY) maximumY = screen.y;
  }

  const rectangleWidth = rectangleRightBorderPosition - rectangleLeftBorderPosition;
  const rectangleHeight = rectangleBottomBorderPosition - rectangleTopBorderPosition;

  for (const screen of localScreens.value) {
    screen.widthPercentage = screen.width * 100 / rectangleWidth;
    screen.heightPercentage = screen.height * 100 / rectangleHeight;
    screen.XPercentage = (screen.x - rectangleLeftBorderPosition) * (maximumX / rectangleWidth * 100) / maximumX;
    screen.YPercentage = (screen.y - rectangleTopBorderPosition) * (maximumY / rectangleHeight * 100) / maximumY;
  }

  aspectRatio.value = `${rectangleWidth} / ${rectangleHeight}`;
};

const selectFirstScreenIfRequired = () => {
  if (!props.allowEmptySelection && localScreens.value.length > 0) {
    select(localScreens.value[0]);
    emit('selectionChanged', [localScreens.value[0]]);
  }
};

watch(() => props.screens, (newState) => {
  localScreens.value = newState.map(x => ({
    ...x,
    widthPercentage: 0, 
    heightPercentage: 0, 
    XPercentage: 0, 
    YPercentage: 0, 
  } satisfies LocalScreen)) ?? [];

  refresh();
  selectFirstScreenIfRequired();
  
}, { immediate: true });

const deselect = (screen: Screen) => {
  if (!props.allowEmptySelection && localScreens.value.filter(x => x.isSelected)?.length == 1 && screen.id == localScreens.value.find(x => x.isSelected)!.id) return;
  screen.isSelected = false;
};

const select = (screen: Screen) => {
  if(props.readonly) return;
  screen.isSelected = true;
  if (!props.allowMultipleSelection) deselectAllScreensExcept(screen);
};

const toggleSelection = (screen: Screen) => {
  if (screen.isSelected) deselect(screen);
  else select(screen);
};

const deselectAllScreensExcept = (screenToKeepSelected: Screen) => {
  localScreens.value.forEach(screen => {
    if (screen.id != screenToKeepSelected.id) deselect(screen);
  });
};


const onScreenClick = (event: MouseEvent | KeyboardEvent, screen: Screen) => {
  const before = JSON.stringify(localScreens.value);// save current state

  if (!props.clickToggle) {
    if (!event.ctrlKey) {
      select(screen);
      deselectAllScreensExcept(screen);
    } else {
      toggleSelection(screen);
    }
  } else {
    toggleSelection(screen);
  }

  if (JSON.stringify(localScreens.value) != before)
  {
    emit('selectionChanged', localScreens.value.filter(x => x.isSelected));
    localScreens.value = JSON.parse(before);// rollback changes
  }
};

</script>

<template>
  <div
    class="relative mx-auto max-h-full"
    :style="{ aspectRatio }"
  >
    <div
      v-for="screen in localScreens"
      v-if="localScreens.length > 0"
      
      :key="screen.id"
      class="screen"
      :style="{ width: `${screen.widthPercentage}%`, height: `${screen.heightPercentage}%`, left: `${screen.XPercentage}%`, top: `${screen.YPercentage}%` }"
      :class="{ 'selected': screen.isSelected }"
      tabindex="0"
      @keyup.enter="$event => onScreenClick($event, screen)"
      @click="$event => onScreenClick($event, screen)"
    >
      <p class="absolute text-sm top-1 left-1">
        {{ screen.index }}
      </p>
      <p>{{ screen.label }}</p>
      <p>{{ screen.width }} x {{ screen.height }}</p>
      <p
        v-tooltip.top="'Current brightness'"
        v-if="screen.isBrightnessSupported"
      >
        <i class="pi pi-lightbulb" /> {{ (screen.currentBrightness! - screen.brightnessMinimum!) * 100 / (screen.brightnessMaximum! - screen.brightnessMinimum!) }}%
      </p>
    </div>
    <slot
      v-else
      name="no-screen"
    >
      <slot name="no-screen">
        <p class="text-center">
          No screen
        </p>
      </slot>
    </slot>
  </div>
</template>

<style scoped>
.screen {
  @apply truncate flex-col relative; 
  background-color: #2E2E2E;
  color: white;
  position: absolute;
  border-radius: 5px;
  display: flex;
  justify-content: center;
  align-items: center;
  text-align: center;
  border: 1px solid transparent;
  background-clip: padding-box;
}

.screen:hover {
  background-color: #454545;
  border: 2px solid white !important;
}

.screen.selected {
  background-color: #0078D7;
  border: 1px solid white;
}
</style>
