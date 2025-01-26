import { ConfigurationDto } from "./dtos/configurations/configurationDto";
import { KeyBindingDto } from "./dtos/keyBindingDto";
import { ScreenDto } from "./dtos/screenDto";

export enum Routes {
  CATEGORY_SELECTION = "CATEGORY_SELECTION",
  CONFIGURATIONS = "CONFIGURATIONS",
  CONFIGURATIONS_CREATE = "CONFIGURATIONS_CREATE",
  KEY_BINDINGS = "KEY_BINDINGS",
  PARAMETERS = "PARAMETERS",
  CONFIGURATIONS_UPDATE = "CONFIGURATIONS_UPDATE",
  KEY_BINDING_UPDATE = "KEY_BINDING_UPDATE",
  KEY_BINDING_CREATE = "KEY_BINDING_CREATE",
}

export const getScreens = async () : Promise<ScreenDto[]> => {
  const response = await fetch(`${import.meta.env.VITE_SERVER_BASE_URL}/api/screens`);

  if(response.status == 200)
    return await response.json();

  throw Error();
};

export const getConfigurations = async () : Promise<ConfigurationDto[]> => {
  const response = await fetch(`${import.meta.env.VITE_SERVER_BASE_URL}/api/configurations`);

  if(response.status == 200)
    return await response.json();

  throw Error();
};

export const saveConfiguration = async (configuration: ConfigurationDto) : Promise<ConfigurationDto> => {
  const response = await fetch(`${import.meta.env.VITE_SERVER_BASE_URL}/api/configurations/${configuration.id}`, { 
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(configuration)
  });

  if(response.status == 200)
    return await response.json();

  throw Error();
};

export const deleteConfiguration = async (configuration: ConfigurationDto) : Promise<void> => {
  const response = await fetch(`${import.meta.env.VITE_SERVER_BASE_URL}/api/configurations/${configuration.id}`, { 
    method: "DELETE",
  });

  if(response.status == 200)
    return;

  throw Error();
};

export const getKeyBindings = async () : Promise<KeyBindingDto[]> => {
  const response = await fetch(`${import.meta.env.VITE_SERVER_BASE_URL}/api/keybindings`);

  if(response.status == 200)
    return await response.json();

  throw Error();
};

export const saveKeyBinding = async (binding: KeyBindingDto) : Promise<KeyBindingDto> => {
  const response = await fetch(`${import.meta.env.VITE_SERVER_BASE_URL}/api/keybindings/${binding.id}`, { 
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(binding)
  });

  if(response.status == 200)
    return await response.json();

  throw Error();
};

export const deleteKeyBinding = async (binding: KeyBindingDto) : Promise<void> => {
  const response = await fetch(`${import.meta.env.VITE_SERVER_BASE_URL}/api/keybindings/${binding.id}`, { 
    method: "DELETE",
  });

  if(response.status == 200)
    return;

  throw Error();
};

export const isNullOrWhitespace = ( input?: string ) => {
  return (typeof input === 'undefined' || input == null)
    || input.replace(/\s/g, '').length < 1;
};