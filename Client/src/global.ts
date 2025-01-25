import { ConfigurationApplyResultDto } from "./dtos/configurations/configurationApplyResultDto";
import { ConfigurationDto } from "./dtos/configurations/configurationDto";
import { ScreenDto } from "./dtos/screenDto";

export enum Routes {
  CATEGORY_SELECTION = "home",
  CONFIGURATIONS = "configuration",
  CONFIGURATIONS_CREATE = "configuration_create",
  BINDINGS = "bindings",
  PARAMETERS = "parameters",
  CONFIGURATIONS_UPDATE = "CONFIGURATIONS_UPDATE",
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

export const applyConfiguration = async (configuration: ConfigurationDto) : Promise<ConfigurationApplyResultDto> => {
  const response = await fetch(`${import.meta.env.VITE_SERVER_BASE_URL}/api/configurations/apply`, { 
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(configuration)
  });

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

export const isNullOrWhitespace = ( input?: string ) => {
  return (typeof input === 'undefined' || input == null)
    || input.replace(/\s/g, '').length < 1;
};