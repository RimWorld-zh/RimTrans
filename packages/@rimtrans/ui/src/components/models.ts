import { RawLocation } from 'vue-router';

/**
 * Nav Item
 */
export interface NavItem {
  icon?: [string, string];
  label: string;
  to: RawLocation;
}
