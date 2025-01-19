export interface ScreenDto {
  devicePath: string;
  label: string;
  isPrimary: boolean;
  x: number;
  y: number;
  width: number;
  height: number;
  isDDCSupported: boolean;
  isBrightnessSupported: boolean;
  minBrightness: number;
  maxBrightness: number;
  currentBrightness: number;
}
