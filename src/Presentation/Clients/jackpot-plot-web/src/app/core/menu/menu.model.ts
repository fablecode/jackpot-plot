export interface MenuItem {
  id?: string,
  title: string;
  icon?: string;
  link?: string;
  children?: MenuItem[];
}
