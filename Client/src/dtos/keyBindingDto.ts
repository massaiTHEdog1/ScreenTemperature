export interface KeyBindingDto {
  id: string;
  name: string;
  commands: any[];
  keyCode: number;
  alt: boolean;
  shift: boolean;
  control: boolean;
}