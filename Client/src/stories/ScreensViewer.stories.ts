// ScreensViewer.stories.ts

import type { Meta, StoryObj } from '@storybook/vue3';
import { action } from '@storybook/addon-actions';

import ScreensViewer from '../components/ScreensViewer.vue';
import { Screen } from '../models/screen';

const meta: Meta<typeof ScreensViewer> = {
  component: ScreensViewer,
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<typeof ScreensViewer>;

/*
 *👇 Render functions are a framework specific feature to allow you control on how the component renders.
 * See https://storybook.js.org/docs/7.0/vue/api/csf
 * to learn how to use render functions.
 */
export const Template: Story = { 
  args: {
    style: undefined,
    screens: [
      new Screen({
        DevicePath: "1",
        Label: "Screen 1",
        Width: 2560,
        Height: 1440
      }),
      new Screen({
        DevicePath: "2",
        Label: "Screen 2",
        Width: 1920,
        Height: 1080,
        X: 2560,
        Y: 0
      }),
      new Screen({
        DevicePath: "3",
        Label: "Screen 3",
        Width: 3840,
        Height: 2160,
        X: 4480,
        Y: 0
      }),
      new Screen({
        DevicePath: "4",
        Label: "Screen 4",
        Width: 1920,
        Height: 1080,
        X: 2560,
        Y: 1080
      })
    ],
    onSelectionChanged: action("selectionChanged")
  },
};