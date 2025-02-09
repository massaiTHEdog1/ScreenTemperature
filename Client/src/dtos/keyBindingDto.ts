export interface KeyBindingDto {
  id: string;
  name: string;
  configurationIds: string[];
  keyCode: number;
  alt: boolean;
  control: boolean;
}